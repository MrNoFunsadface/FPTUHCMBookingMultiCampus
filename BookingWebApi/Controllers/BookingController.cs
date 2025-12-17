using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using System.Security.Claims;

namespace BookingWebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _service;
    private readonly IUserService _userService;

    public BookingController(
        IBookingService service,
        IUserService userService
    )
    {
        _service = service;
        _userService = userService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Booking date must be tomorrow or later
        if (dto.BookingDate < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            return BadRequest("Booking date must be tomorrow or later.");

        // Find user by email
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var user = await _userService.GetByEmail(email);
        if (user == null) return NotFound("User not found.");

        // Turn rooms and slots into roomslots
        var roomSlots = dto.Rooms
            .SelectMany(r =>
                r.SlotIds.Select(sid =>
                    new Roomslot
                    {
                        RoomId = r.RoomId,
                        SlotId = sid
                    }))
            .DistinctBy(rs => new { rs.RoomId, rs.SlotId })
            .ToList();

        if (!roomSlots.Any())
            return BadRequest("No room-slot selected.");

        // Create booking
        var booking = new Booking
        {
            BookingDate = dto.BookingDate,
            Roomslots = roomSlots
        };

        var result = await _service.Create(user.UserId, booking);

        if (result == null)
            return Conflict("Room/slot does not exist or booked.");

        return Ok(result);
    }


    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        return Ok(await _service.GetHistory(userId));
    }

    [Authorize(Roles = "0, 3")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending() => Ok(await _service.GetPending());

    [Authorize(Roles = "0, 3")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id) { await _service.Approve(id); return Ok(); }

    [Authorize(Roles = "0, 3")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id) { await _service.Reject(id); return Ok(); }

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) { await _service.Cancel(id); return Ok(); }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var b = await _service.GetById(id);
        if (b == null) return NotFound();
        return Ok(b);
    }

    public sealed record CreateBookingRequest(DateOnly BookingDate, List<RoomSlotRequest> Rooms);

    public sealed record RoomSlotRequest(int RoomId, List<int> SlotIds);
}