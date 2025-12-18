using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
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
    [SwaggerOperation(Summary = "User: Create a booking"
        , Description = "User create a new booking, can have multiple room, multiple slots per room." +
        "Booking must not have duplicate roomslot, must be tomorrow or later.")]
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
    [SwaggerOperation(Summary = "User: Cancel booking"
        , Description = "User cancel a booking. Cannot cancel booking same date or in the past.")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) { await _service.Cancel(id); return Ok(); }

    [Authorize]
    [SwaggerOperation(Summary = "User: Get booking history"
        , Description = "User get booking history.")]
    [HttpGet("booking-history")]
    public async Task<IActionResult> GetBookingHistory(
        [FromQuery] int currentPage = 1,
        [FromQuery] int pageSize = 10)
    {
        // Find user by email
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var user = await _userService.GetByEmail(email);
        if (user == null) return NotFound("User not found.");

        var page = await _service.GetByUser(user.UserId, currentPage, pageSize);
        return Ok(page);
    }

    [Authorize(Roles = "0, 3")]
    [SwaggerOperation(Summary = "Manager: Get booking history by UserId"
        , Description = "Manager get booking history by UserId")]
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetByUserId(int userId,
        [FromQuery] int currentPage = 1,
        [FromQuery] int pageSize = 10)
    {
        var user = await _userService.GetById(userId);
        if (user == null) return NotFound("User not found.");
        return Ok(await _service.GetByUser(userId, currentPage, pageSize));
    }

    [Authorize(Roles = "0, 3")]
    [SwaggerOperation(Summary = "Manager: Get booking by ID"
        , Description = "Manager get booking by ID")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _service.GetById(id));

    [Authorize(Roles = "0, 3")]
    [SwaggerOperation(Summary = "Manager: Get pending bookings"
        , Description = "Manager get pending bookings.")]
    [HttpGet("get-pending")]
    public async Task<IActionResult> GetPending([FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10) => Ok(await _service.GetPending(currentPage, pageSize));

    [Authorize(Roles = "0, 3")]
    [SwaggerOperation(Summary = "Manager: Approve booking"
        , Description = "Manager approve a booking.")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id) => Ok(await _service.Approve(id));

    [Authorize(Roles = "0, 3")]
    [SwaggerOperation(Summary = "Manager: Reject booking"
        , Description = "Manager reject a booking.")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id) => Ok(await _service.Reject(id));

    public sealed record CreateBookingRequest(DateOnly BookingDate, List<RoomSlotRequest> Rooms);

    public sealed record RoomSlotRequest(int RoomId, List<int> SlotIds);
}