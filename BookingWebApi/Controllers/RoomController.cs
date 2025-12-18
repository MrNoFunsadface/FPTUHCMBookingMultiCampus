using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

namespace BookingWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _service;

        public RoomController(IRoomService service)
        {
            _service = service;
        }

        [SwaggerOperation(Summary = "User: Get rooms", Description = "User get paginated list of rooms.")]
        [HttpGet]
        public async Task<IActionResult> GetRooms(
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 10)
        {
            var rooms = await _service.GetRooms(currentPage, pageSize);
            return Ok(rooms);
        }

        [SwaggerOperation(Summary = "User: Get room by id", Description = "User get room details by id.")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _service.GetRoomById(id);
            if (room == null) return NotFound("Room not found.");
            return Ok(room);
        }

        [SwaggerOperation(Summary = "User: Get rooms by campus", Description = "User get paginated rooms filtered by campus.")]
        [HttpGet("by-campus")]
        public async Task<IActionResult> GetRoomsByCampus(
            [FromQuery] int campusId,
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 10)
        {
            var rooms = await _service.GetRoomsByCampus(campusId, currentPage, pageSize);
            return Ok(rooms);
        }

        [SwaggerOperation(Summary = "User: Get room by code and campus", Description = "User get room details by code and campus id.")]
        [HttpGet("by-code-and-campus")]
        public async Task<IActionResult> GetRoomByCodeAndCampusId(
            [FromQuery] string code,
            [FromQuery] int campusId)
        {
            var room = await _service.GetRoomByCodeAndCampusId(code, campusId);
            if (room == null) return NotFound("Room not found.");

            return Ok(room);
        }

        [Authorize(Roles = "0, 3")]
        [SwaggerOperation(Summary = "Manager: Create room", Description = "Manager create a new room.")]
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdRoom = await _service.CreateRoom(room);
            if (createdRoom == null)
                return BadRequest("Room code already exists in this campus.");

            return Ok(createdRoom);
        }

        [Authorize(Roles = "0, 3")]
        [SwaggerOperation(Summary = "Manager: Update room", Description = "Manager update room information.")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existed = await _service.GetRoomById(id);
            if (existed == null) return NotFound();

            var room = new Room
            {
                RoomId = id,
                Code = request.Code,
                CampusId = request.CampusId,
                RoomType = request.RoomType,
                Capacity = request.Capacity,
                IsAvailable = request.IsAvailable
            };

            var updatedRoom = await _service.UpdateRoom(room);
            if (updatedRoom == null)
                return BadRequest("Room code already exists in this campus.");

            return Ok(updatedRoom);
        }

        [Authorize(Roles = "0, 3")]
        [SwaggerOperation(Summary = "Manager: Enable room", Description = "Manager enable a room (set available).")]
        // PUT api/rooms/{id}/enable
        [HttpPut("{id}/enable")]
        public async Task<IActionResult> EnableRoom(int id)
        {
            var room = await _service.EnableRoom(id);
            if (room == null) return NotFound("Room not found.");

            return Ok(room);
        }

        [Authorize(Roles = "0, 3")]
        [SwaggerOperation(Summary = "Manager: Disable room", Description = "Manager disable a room (set unavailable).")]
        // PUT api/rooms/{id}/disable
        [HttpPut("{id}/disable")]
        public async Task<IActionResult> DisableRoom(int id)
        {
            var room = await _service.DisableRoom(id);
            if (room == null) return NotFound("Room not found.");

            return Ok(room);
        }

        public sealed record UpdateRoomRequest(string Code, int CampusId, string RoomType, int Capacity, bool IsAvailable);
    }
}
