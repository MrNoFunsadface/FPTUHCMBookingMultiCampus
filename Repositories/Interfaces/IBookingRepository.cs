using Repositories.Models;
namespace Repositories.Interfaces;
public interface IBookingRepository
{
    Task<Booking?> Create(Booking booking);
    Task<Booking?> GetById(int id);
    Task<List<Booking>> GetByUser(int userId);
    Task<List<Booking>> GetAllPending();
    Task Approve(int bookingId);
    Task Reject(int bookingId);
    Task Cancel(int bookingId);
}
