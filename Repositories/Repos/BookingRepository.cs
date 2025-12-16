// File: Repositories/Repos/BookingRepository.cs
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repos;
public class BookingRepository : IBookingRepository
{
    private readonly Context _context;
    public BookingRepository() { _context = new Context(); }
    public BookingRepository(Context context) { _context = context; }

    // Booking: no book same slot booked before in 1 room, no book different date, allow different room
    public async Task<Booking?> Create(Booking booking)
    {
        if (booking == null) return null;

        // Normalize requested room-slot pairs and remove duplicates
        var requestedPairs = booking.Roomslots?
            .Select(rs => (RoomId: rs.RoomId, SlotId: rs.SlotId))
            .Distinct()
            .ToList() ?? new List<(int RoomId, int SlotId)>();

        if (!requestedPairs.Any()) return null; // nothing to book

        // Load existing bookings on the same date into memory (exclude canceled/rejected)
        // Doing this in memory allows us to use local collections to check overlaps without
        // forcing EF Core to translate complex client-side Any(...) into SQL.
        var existingBookings = await _context.Bookings
            .Where(b => b.BookingDate == booking.BookingDate && b.Status != "Canceled" && b.Status != "Rejected")
            .Include(b => b.Roomslots)
            .ToListAsync();

        // Check for conflicts in-memory
        var conflict = existingBookings.Any(b => b.Roomslots.Any(rs => requestedPairs.Any(rp => rp.RoomId == rs.RoomId && rp.SlotId == rs.SlotId)));

        if (conflict) return null;

        // Load the actual Roomslot entities from DB to ensure they exist and to attach them to the booking
        var roomSlots = new List<Roomslot>();
        foreach (var rp in requestedPairs)
        {
            // Use FindAsync with composite key (RoomId, SlotId)
            var rs = await _context.Roomslots.FindAsync(rp.RoomId, rp.SlotId);
            if (rs != null)
            {
                roomSlots.Add(rs);
            }
        }

        // If any requested pair does not exist in DB, reject the request
        if (roomSlots.Count != requestedPairs.Count)
            return null;

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // Ensure booking.Roomslots collection is empty and attach the existing roomSlots
            booking.Roomslots = new List<Roomslot>();

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Attach existing Roomslot entities to the booking (many-to-many)
            foreach (var rs in roomSlots)
            {
                booking.Roomslots.Add(rs);
            }

            await _context.SaveChangesAsync();

            // Load navigation properties for return
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

    public Task<Booking?> GetById(int id) => _context.Bookings
        .Include(b => b.Roomslots).ThenInclude(rs => rs.Room)
        .Include(b => b.Roomslots).ThenInclude(rs => rs.Slot)
        .FirstOrDefaultAsync(b => b.BookingId == id);

    public Task<List<Booking>> GetByUser(int userId) => _context.Bookings
        .Where(b => b.RequestedByUserId == userId)
        .Include(b => b.Roomslots).ThenInclude(rs => rs.Room)
        .Include(b => b.Roomslots).ThenInclude(rs => rs.Slot)
        .ToListAsync();

    public Task<List<Booking>> GetAllPending() => _context.Bookings
        .Where(b => b.Status == "Pending")
        .Include(b => b.Roomslots).ThenInclude(rs => rs.Room)
        .Include(b => b.Roomslots).ThenInclude(rs => rs.Slot)
        .ToListAsync();

    public async Task Approve(int bookingId)
    {
        var b = await _context.Bookings.FindAsync(bookingId);
        if (b == null) return;
        b.Status = "Approved";
        b.ApprovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task Reject(int bookingId)
    {
        var b = await _context.Bookings.FindAsync(bookingId);
        if (b == null) return;
        b.Status = "Rejected";
        await _context.SaveChangesAsync();
    }

    public async Task Cancel(int bookingId)
    {
        var b = await _context.Bookings.FindAsync(bookingId);
        if (b == null) return;
        b.Status = "Canceled";
        b.CancelAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
