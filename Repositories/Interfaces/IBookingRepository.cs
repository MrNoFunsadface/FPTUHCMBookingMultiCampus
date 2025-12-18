using Repositories.Models;
using Repositories.ModelExtensions;
namespace Repositories.Interfaces;
public interface IBookingRepository
{
    public Task<Booking?> Create(Booking booking);
    public Task<bool> Cancel(int bookingId);
    public Task<PaginationResult<Booking>> GetByUser(int userId, int currentPage, int pageSize);
    public Task<Booking?> GetById(int id);
    public Task<PaginationResult<Booking>> GetAllPending(int currentPage, int pageSize);
    public Task<bool> Approve(int bookingId);
    public Task<bool> Reject(int bookingId);
}
