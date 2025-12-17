using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

namespace BookingWebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CampusController : ControllerBase
{
    private readonly ICampusService _service;
    public CampusController(ICampusService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCampusById(int id)
    {
        var campus = await _service.GetCampusById(id);
        if (campus == null) return NotFound();
        return Ok(campus);
    }

    [Authorize(Roles = "0, 3")]
    [HttpPost]
    public async Task<IActionResult> CreateCampus([FromBody] Campus campus)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var createdCampus = await _service.CreateCampus(campus);

        return Ok(createdCampus);
    }

    [Authorize(Roles = "0, 3")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCampus(int id, [FromBody] Campus campus)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existed = await _service.GetCampusById(id);
        if (existed == null) return NotFound();

        var createdCampus = await _service.UpdateCampus(campus);

        return Ok(createdCampus);
    }
}