using Microsoft.AspNetCore.Mvc;
using Services;
using Swashbuckle.AspNetCore.Annotations;

namespace BookingWebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SlotController : ControllerBase
{
    private readonly ISlotService _service;
    public SlotController(ISlotService service)
    {
        _service = service;
    }

    [SwaggerOperation(Summary = "User: Get slots", Description = "User get list of available time slots. Time slots are fixed value.")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAll());
}