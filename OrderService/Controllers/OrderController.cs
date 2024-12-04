using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private static Dictionary<int, string> Orders = new();

    [HttpPost("create")]
    public IActionResult CreateOrder([FromBody] int orderId)
    {
        Orders[orderId] = "Created";
        return Ok(new { OrderId = orderId, Status = "Order Created" });
    }

    [HttpPost("rollback")]
    public IActionResult RollbackOrder([FromBody] int orderId)
    {
        if (Orders.ContainsKey(orderId))
        {
            Orders.Remove(orderId);
            return Ok(new { OrderId = orderId, Status = "Order Rolled Back" });
        }
        return NotFound(new { OrderId = orderId, Status = "Order Not Found" });
    }
}
