using Repositories.ModelExtensions;
using Repositories.Models;
namespace Repositories.Interfaces;
public interface IRoomRepository
{
    public Task<PaginationResult<Room>> GetRooms(int currentPage, int pageSize);
    public Task<Room?> GetRoomByCodeAndCampusId(string code, int campusId);
    public Task<Room?> GetRoomById(int roomId);
    public Task<List<Room>> GetRoomsByCampus(int campusId);
    public Task<Room?> EnableRoom(int roomId);
    public Task<Room?> DisableRoom(int roomId);
    public Task<Room?> CreateRoom(Room room);
    public Task<Room?> UpdateRoom(Room room);
    public Task<bool> DeleteRoom(int roomId);
}