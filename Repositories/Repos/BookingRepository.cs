// File: Repositories/Repos/BookingRepository.cs
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using Repositories.ModelExtensions;

namespace Repositories.Repos;
public class BookingRepository : IBookingRepository
{
    private readonly Context _context;
    public BookingRepository() { _context = new Context(); }
    public BookingRepository(Context context) { _context = context; }

    // Booking: no book same slot booked before in 1 room, no book different date, allow different room, multiple slots of the same room
    public async Task<Booking?> Create(Booking booking)
    {
        if (booking == null) return null;

        var requestedPairs = booking.Roomslots?
            .Select(rs => (RoomId: rs.RoomId, SlotId: rs.SlotId))
            .Distinct()
            .ToList() ?? new List<(int RoomId, int SlotId)>();

        if (!requestedPairs.Any()) return null;

        var existingBookings = await _context.Bookings
            .Where(b => b.BookingDate == booking.BookingDate && b.Status != "Canceled" && b.Status != "Rejected")
            .Include(b => b.Roomslots)
            .ToListAsync();

        var conflict = existingBookings.Any(b => b.Roomslots.Any(rs => requestedPairs.Any(rp => rp.RoomId == rs.RoomId && rp.SlotId == rs.SlotId)));
        if (conflict) return null;

        var roomIds = requestedPairs.Select(p => p.RoomId).Distinct().ToList();
        var candidateRoomSlots = await _context.Roomslots
            .Where(rs => roomIds.Contains(rs.RoomId))
            .ToListAsync();

        var requestedSet = new HashSet<(int, int)>(requestedPairs);
        var roomSlots = candidateRoomSlots
            .Where(rs => requestedSet.Contains((rs.RoomId, rs.SlotId)))
            .ToList();

        if (roomSlots.Count != requestedPairs.Count)
            return null;

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            booking.Roomslots = new List<Roomslot>();

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            foreach (var rs in roomSlots)
            {
                booking.Roomslots.Add(rs);
            }

            await _context.SaveChangesAsync();

            await _context.Entry(booking).Collection(b => b.Roomslots).Query().Include(rs => rs.Room).Include(rs => rs.Slot).LoadAsync();

            await tx.CommitAsync();
            return booking;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> Cancel(int bookingId)
    {
        var b = await _context.Bookings.FindAsync(bookingId);
        if (b == null) return false;

        // Already canceled cannot be canceled again
        if (string.Equals(b.Status, "Canceled", StringComparison.OrdinalIgnoreCase))
            return false;

        // Cannot change from Rejected to Canceled
        if (string.Equals(b.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            return false;

        // If booking already approved, it can be canceled (allowed)
        // Do not allow cancel during or after the booking date
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        if (todayUtc >= b.BookingDate)
            return false;

        b.Status = "Canceled";
        b.CancelAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<PaginationResult<Booking>> GetByUser(int userId, int currentPage, int pageSize)
    {
        IQueryable<Booking> query = _context.Bookings
            .Where(b => b.RequestedByUserId == userId)
            .Include(b => b.Roomslots).ThenInclude(rs => rs.Room)
            .Include(b => b.Roomslots).ThenInclude(rs => rs.Slot)
            .OrderByDescending(b => b.BookingDate).ThenByDescending(b => b.RequestedAt);

        return PaginationExtension.PaginateAsync(query, currentPage, pageSize);
    }

    public Task<Booking?> GetById(int id) => _context.Bookings
        .Include(b => b.Roomslots).ThenInclude(rs => rs.Room)
        .Include(b => b.Roomslots).ThenInclude(rs => rs.Slot)
        .FirstOrDefaultAsync(b => b.BookingId == id);

    public Task<PaginationResult<Booking>> GetAllPending(int currentPage, int pageSize)
    {
        IQueryable<Booking> query = _context.Bookings
            .Where(b => b.Status == "Pending")
            .Include(b => b.Roomslots).ThenInclude(rs => rs.Room)
            .Include(b => b.Roomslots).ThenInclude(rs => rs.Slot)
            .OrderBy(b => b.BookingDate).ThenBy(b => b.RequestedAt);

        return PaginationExtension.PaginateAsync(query, currentPage, pageSize);
    }

    public async Task<bool> Approve(int bookingId)
    {
        var b = await _context.Bookings.FindAsync(bookingId);
        if (b == null) return false;

        // Already approved cannot be approved again
        if (string.Equals(b.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            return false;

        // Cannot change from Rejected to Approved
        if (string.Equals(b.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            return false;

        // Cannot change from Canceled to Approved
        if (string.Equals(b.Status, "Canceled", StringComparison.OrdinalIgnoreCase))
            return false;

        // Approve only from Pending
        if (!string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return false;

        b.Status = "Approved";
        b.ApprovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Reject(int bookingId)
    {
        var b = await _context.Bookings.FindAsync(bookingId);
        if (b == null) return false;

        // Already rejected cannot be rejected again
        if (string.Equals(b.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            return false;

        // Cannot change from Approved to Rejected
        if (string.Equals(b.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            return false;

        // Cannot change from Canceled to Rejected
        if (string.Equals(b.Status, "Canceled", StringComparison.OrdinalIgnoreCase))
            return false;

        // Reject only from Pending
        if (!string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return false;

        b.Status = "Rejected";
        await _context.SaveChangesAsync();
        return true;
    }
}
