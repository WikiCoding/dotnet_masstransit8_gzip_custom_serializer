using Microsoft.AspNetCore.Mvc;

namespace MT8_Demo.Controllers;

[ApiController]
[Route("[controller]")]
public class PublishController : ControllerBase
{
    private readonly ILogger<PublishController> _logger;
    private readonly MsgProducer _msgProducer;

    public PublishController(ILogger<PublishController> logger, MsgProducer msgProducer)
    {
        _logger = logger;
        _msgProducer = msgProducer;
    }

    [HttpPost]
    public async Task<IActionResult> Produce()
    {
        await _msgProducer.ExecuteAsync();

        return Ok();
    }
}
