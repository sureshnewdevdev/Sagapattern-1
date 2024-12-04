using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    [HttpPost("process")]
    public IActionResult ProcessPayment([FromBody] int orderId)
    {
        if (orderId % 2 == 0) // Simulate failure for odd IDs
        {
            return StatusCode(500, new { OrderId = orderId, Status = "Payment Failed" });
        }
        return Ok(new { OrderId = orderId, Status = "Payment Successful" });
    }
}
