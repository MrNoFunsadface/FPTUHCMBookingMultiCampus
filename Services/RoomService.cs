using Repositories.Interfaces;
using Repositories.ModelExtensions;
using Repositories.Models;

namespace Services
{
    public interface IRoomService
    {
        public Task<PaginationResult<Room>> GetRooms(int currentPage, int pageSize);
        public Task<Room?> GetRoomById(int roomId);
        public Task<PaginationResult<Room>> GetRoomsByCampus(int campusId, int currentPage, int pageSize);
        public Task<Room?> GetRoomByCodeAndCampusId(string code, int campusId);
        public Task<Room?> CreateRoom(Room room);
        public Task<Room?> UpdateRoom(Room room);
        public Task<Room?> EnableRoom(int roomId);
        public Task<Room?> DisableRoom(int roomId);
    }

    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repository;

        public RoomService(IRoomRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginationResult<Room>> GetRooms(int currentPage, int pageSize)
            => await _repository.GetRooms(currentPage, pageSize);
        public async Task<Room?> GetRoomById(int roomId)
            => await _repository.GetRoomById(roomId);
        public async Task<PaginationResult<Room>> GetRoomsByCampus(int campusId, int currentPage, int pageSize)
            => await _repository.GetRoomsByCampus(campusId, currentPage, pageSize);
        public async Task<Room?> GetRoomByCodeAndCampusId(string code, int campusId)
            => await _repository.GetRoomByCodeAndCampusId(code, campusId);
        public async Task<Room?> CreateRoom(Room room)
            => await _repository.CreateRoom(room);
        public async Task<Room?> UpdateRoom(Room room)
            => await _repository.UpdateRoom(room);
        public async Task<Room?> EnableRoom(int roomId)
            => await _repository.EnableRoom(roomId);
        public async Task<Room?> DisableRoom(int roomId)
            => await _repository.DisableRoom(roomId);
    }
}
