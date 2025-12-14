using Repositories.Interfaces;
using Repositories.Models;

namespace Services;
public interface IBookingService
{
    Task<Booking?> Create(int userId, Booking booking);
    Task<List<Booking>> GetHistory(int userId);
    Task<List<Booking>> GetPending();
    Task Approve(int id);
    Task Reject(int id);
    Task Cancel(int id);
    Task<Booking?> GetById(int id);
}

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Booking?> Create(int userId, Booking booking)
    {
        if (booking == null) return null;

        // Prepare booking entity
        booking.RequestedByUserId = userId;
        booking.Status = "Pending";
        booking.RequestedAt = DateTime.UtcNow;

        var created = await _repository.Create(booking);
        return created;
    }

    public async Task<List<Booking>> GetHistory(int userId)
    {
        var bookings = await _repository.GetByUser(userId);
        return bookings;
    }

    public async Task<List<Booking>> GetPending()
    {
        var bookings = await _repository.GetAllPending();
        return bookings;
    }

    public Task Approve(int id) => _repository.Approve(id);
    public Task Reject(int id) => _repository.Reject(id);
    public Task Cancel(int id) => _repository.Cancel(id);

    public async Task<Booking?> GetById(int id)
    {
        var b = await _repository.GetById(id);
        return b;
    }
}