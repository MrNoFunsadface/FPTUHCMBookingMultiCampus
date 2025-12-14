using BookingWebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;
using Repositories.Models;

namespace BookingWebApi.Controllers;
[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _service;
    public BookingController(IBookingService service) { _service = service; }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // Map DTO -> Booking entity
        var booking = new Booking
        {
            BookingDate = DateOnly.FromDateTime(dto.BookingDate),
            Roomslots = dto.SlotIds.Select(sid => new Roomslot { RoomId = dto.RoomId, SlotId = sid }).ToList()
        };

        var result = await _service.Create(userId, booking);
        if (result == null) return Conflict("Conflict or invalid room/slots");
        return CreatedAtAction(nameof(Get), new { id = result.BookingId }, result);
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        return Ok(await _service.GetHistory(userId));
    }

    [Authorize(Roles = "2")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending() => Ok(await _service.GetPending());

    [Authorize(Roles = "2")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id) { await _service.Approve(id); return Ok(); }

    [Authorize(Roles = "2")]
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
}