using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrchestratorController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<int> _failedNotifications = new();
    HttpClient orderClient = new HttpClient();

    public OrchestratorController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessOrder([FromBody] OrderRequest orderRequest)
    {
        if (orderRequest == null || orderRequest.OrderId <= 0)
        {
            return BadRequest(new { Message = "Invalid Order Request" });
        }

        int orderId = orderRequest.OrderId;

        try
        {
            // 1. Create Order
            orderClient = _httpClientFactory.CreateClient();

            var orderResponse = await orderClient.PostAsJsonAsync("https://localhost:7177/api/Order/create", orderId);

            if (!orderResponse.IsSuccessStatusCode)
            {
                return Problem("Order creation failed.", statusCode: 500);
            }

            

            // 3. Send Notification
            var notificationClient = _httpClientFactory.CreateClient();
            // Success scenario
            //var notificationResponse = await notificationClient.PostAsJsonAsync("https://localhost:7092/api/notification/send",  orderId );

            // Failure Scenario
            var notificationResponse = await notificationClient.PostAsJsonAsync("https://localhost:7092/api/notification/send", new { OrderId = orderId });

            if (!notificationResponse.IsSuccessStatusCode)
            {
                // Add to queue for failed notifications
                _failedNotifications.Add(orderId);
                return StatusCode(207, new { OrderId = orderId, Status = "Notification Failed. Added to Retry Queue" });
            }

            return Ok(new { OrderId = orderId, Status = "Order Completed Successfully" });
        }
        catch (Exception ex)
        {
            // Log the exception (optional: use a logging framework like Serilog or NLog)
            return Problem($"An unexpected error occurred: {ex.Message}", statusCode: 500);
        }

        
    }
    public async Task<IActionResult> PaymentOrder([FromBody] OrderRequest orderRequest)
    {
        orderClient = _httpClientFactory.CreateClient();

        // 2. Process Payment
        var paymentClient = _httpClientFactory.CreateClient();
        var paymentResponse = await paymentClient.PostAsJsonAsync("https://localhost:7060/api/payment/process", orderRequest.OrderId);

        if (!paymentResponse.IsSuccessStatusCode)
        {
            // Rollback Order if Payment fails
            var rollbackResponse = await orderClient.PostAsJsonAsync("https://localhost:7177/api/order/rollback", orderRequest.OrderId);
            if (rollbackResponse.IsSuccessStatusCode)
            {
                return StatusCode(500, new { OrderId = orderRequest.OrderId, Status = "Payment Failed. Order Rolled Back" });
            }
            return Problem("Payment failed, and rollback unsuccessful.", statusCode: 500);
        }
        return StatusCode(500, new { OrderId = orderRequest.OrderId, Status = "Payment Failed. Order Rolled Back" });

    }

    [HttpGet("failed-notifications")]
public IActionResult GetFailedNotifications()
{
    return Ok(_failedNotifications);
}
}

public class OrderRequest
{
    private int oId;
    
    int coRelationId { get; set; } = 0;
    public int OrderId 
    {
        get { return ReadOrderIdFromDB(10); }
        set { StoreInDB(value); }
    }

    public int OId { get => ReadOrderIdFromDB(oId); set => StoreInDB(value); }

    private void StoreInDB(int value)
    {
        throw new NotImplementedException();
    }

    private int ReadOrderIdFromDB(int oId)
    {
        throw new NotImplementedException();
    }
}
