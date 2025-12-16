using Microsoft.AspNetCore.Mvc;
using Services;

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

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAll());
}