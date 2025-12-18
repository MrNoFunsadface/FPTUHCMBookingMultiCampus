using Repositories.Interfaces;
using Repositories.Models;
using Repositories.ModelExtensions;

namespace Services;
public interface IBookingService
{
    public Task<Booking?> Create(int userId, Booking booking);
    public Task<bool> Cancel(int id);
    public Task<PaginationResult<Booking>> GetByUser(int userId, int currentPage, int pageSize);
    public Task<Booking?> GetById(int id);
    public Task<PaginationResult<Booking>> GetPending(int currentPage, int pageSize);
    public Task<bool> Approve(int id);
    public Task<bool> Reject(int id);
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
    public async Task<bool> Cancel(int id) => await _repository.Cancel(id);
    public async Task<PaginationResult<Booking>> GetByUser(int userId, int currentPage, int pageSize) => await _repository.GetByUser(userId, currentPage, pageSize);
    public async Task<Booking?> GetById(int id) => await _repository.GetById(id);
    public async Task<PaginationResult<Booking>> GetPending(int currentPage, int pageSize) => await _repository.GetAllPending(currentPage, pageSize);
    public async Task<bool> Approve(int id) => await _repository.Approve(id);
    public async Task<bool> Reject(int id) => await _repository.Reject(id);
}