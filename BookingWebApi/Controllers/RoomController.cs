using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

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

        [HttpGet]
        public async Task<IActionResult> GetRooms(
            [FromQuery] int currentPage = 1,
            [FromQuery] int pageSize = 10)
        {
            var rooms = await _service.GetRooms(currentPage, pageSize);
            return Ok(rooms);
        }

        [HttpGet("by-code")]
        public async Task<IActionResult> GetRoomByCodeAndCampusId(
            [FromQuery] string code,
            [FromQuery] int campusId)
        {
            var room = await _service.GetRoomByCodeAndCampusId(code, campusId);
            if (room == null) return NotFound("Room not found.");

            return Ok(room);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _service.GetRoomById(id);
            if (room == null) return NotFound("Room not found.");
            return Ok(room);
        }

        // GET api/room/by-campus/{campusId}
        [HttpGet("by-campus/{campusId}")]
        public async Task<IActionResult> GetRoomsByCampus(int campusId)
        {
            var rooms = await _service.GetRoomsByCampus(campusId);
            return Ok(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdRoom = await _service.CreateRoom(room);
            if (createdRoom == null)
                return BadRequest("Room code already exists in this campus.");

            return Ok(createdRoom);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] Room room)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existed = await _service.GetRoomById(id);
            if (existed == null) return NotFound();

            var updatedRoom = await _service.UpdateRoom(room);
            if (updatedRoom == null)
                return BadRequest("Room code already exists in this campus.");

            return Ok(updatedRoom);
        }

        // PUT api/rooms/{id}/enable
        [HttpPut("{id}/enable")]
        public async Task<IActionResult> EnableRoom(int id)
        {
            var room = await _service.EnableRoom(id);
            if (room == null) return NotFound("Room not found.");

            return Ok(room);
        }

        // PUT api/rooms/{id}/disable
        [HttpPut("{id}/disable")]
        public async Task<IActionResult> DisableRoom(int id)
        {
            var room = await _service.DisableRoom(id);
            if (room == null) return NotFound("Room not found.");

            return Ok(room);
        }

        // DELETE api/room/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var ok = await _service.DeleteRoom(id);
            if (!ok) return NotFound("Room not found.");
            return NoContent();
        }
    }
}
