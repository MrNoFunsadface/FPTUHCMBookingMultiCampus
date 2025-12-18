using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

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

    [SwaggerOperation(Summary = "User: Get campuses", Description = "User get list of campuses.")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAll());

    [SwaggerOperation(Summary = "User: Get campus by id", Description = "User get campus details by id.")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCampusById(int id)
    {
        var campus = await _service.GetCampusById(id);
        if (campus == null) return NotFound();
        return Ok(campus);
    }

    [Authorize(Roles = "0, 3")]
    [SwaggerOperation(Summary = "Manager: Create campus", Description = "Manager create a new campus.")]
    [HttpPost]
    public async Task<IActionResult> CreateCampus([FromBody] string campusName)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var campus = new Campus
        {
            Name = campusName
        };

        var createdCampus = await _service.CreateCampus(campus);

        return Ok(createdCampus);
    }

    [Authorize(Roles = "0, 3")]
    [SwaggerOperation(Summary = "Manager: Update campus", Description = "Manager update campus information.")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCampus(int id, [FromBody] string campusName)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existed = await _service.GetCampusById(id);
        if (existed == null) return NotFound();

        var campus = new Campus
        {
            CampusId = id,
            Name = campusName
        };

        var createdCampus = await _service.UpdateCampus(campus);

        return Ok(createdCampus);
    }
}