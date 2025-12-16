using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Models;
using Repositories.ModelExtensions;

namespace Repositories.Repos
{
    public class RoomRepository : IRoomRepository
    {
        private readonly Context _context;
        private readonly PaginationExtension _paginationExtension;

        public RoomRepository()
        {
            _context ??= new Context();
            _paginationExtension ??= new PaginationExtension();
        }

        public RoomRepository(Context context, PaginationExtension paginationExtension)
        {
            _context = context;
            _paginationExtension = paginationExtension;
        }

        // GetRooms: có pagination
        public async Task<PaginationResult<Room>> GetRooms(int currentPage, int pageSize)
        {
            IQueryable<Room> rooms = _context.Rooms
                .Include(r => r.Campus)
                .OrderBy(r => r.Code);

            return await _paginationExtension.PaginateAsync(rooms, currentPage, pageSize);
        }

        // GetRoomById
        public async Task<Room?> GetRoomById(int roomId)
        {
            if (roomId == 0) return null;

            return await _context.Rooms
                .Include(r => r.Campus)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);
        }

        // GetRooms by Campus: có pagination
        public async Task<PaginationResult<Room>> GetRoomsByCampus(int campusId, int currentPage, int pageSize)
        {
            IQueryable<Room> rooms = _context.Rooms
                .Include(r => r.Campus)
                .Where(r => r.CampusId == campusId)
                .OrderBy(r => r.Code);

            return await _paginationExtension.PaginateAsync(rooms, currentPage, pageSize);
        }

        // GetRoomByCodeAndCampusId
        public async Task<Room?> GetRoomByCodeAndCampusId(string code, int campusId)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            return await _context.Rooms
                .Include(r => r.Campus)
                .FirstOrDefaultAsync(r => r.Code == code && r.CampusId == campusId);
        }

        // EnableRoom: đặt IsAvailable = true
        public async Task<Room?> EnableRoom(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return null;

            room.IsAvailable = true;
            await _context.SaveChangesAsync();
            return room;
        }

        // DisableRoom: đặt IsAvailable = false
        public async Task<Room?> DisableRoom(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return null;

            room.IsAvailable = false;
            await _context.SaveChangesAsync();
            return room;
        }

        // Create Room: không được trùng Code trong cùng Campus
        public async Task<Room?> CreateRoom(Room room)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.Code))
            {
                return null;
            }

            var existed = await _context.Set<Room>()
                .FirstOrDefaultAsync(r => r.Code == room.Code && r.CampusId == room.CampusId);

            if (existed != null)
            {
                return null;
            }

            _context.Set<Room>().Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        // Update Room: không được trùng Code trong cùng Campus
        public async Task<Room?> UpdateRoom(Room room)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.Code))
            {
                return null;
            }

            var existed = await _context.Set<Room>()
                .FirstOrDefaultAsync(r => r.Code == room.Code && r.CampusId == room.CampusId);

            if (existed != null)
            {
                return null;
            }

            _context.Set<Room>().Update(room);
            await _context.SaveChangesAsync();
            return room;
        }

        // soft delete room
        public async Task<bool> DeleteRoom(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return false;

            room.IsAvailable = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
