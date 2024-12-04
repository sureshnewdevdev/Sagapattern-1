using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    [HttpPost("send")]
    public IActionResult SendNotification([FromBody] int orderId)
    {
        if (orderId % 3 == 0) // Simulate failure for multiples of 3
        {
            return StatusCode(500, new { OrderId = orderId, Status = "Notification Failed" });
        }
        return Ok(new { OrderId = orderId, Status = "Notification Sent" });
    }
}
