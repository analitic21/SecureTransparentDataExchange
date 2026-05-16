using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureTransparentDataExchange.Models.Orders.Create;
using SecureTransparentDataExchange.Services;
using System.Security.Claims;

namespace SecureTransparentDataExchange.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            var loginId = GetLoginId();

            var order = await _orderService.CreateForCurrentUserAsync(
                loginId,
                request.TotalAmount,
                request.Currency,
                request.ExternalOrderNumber,
                request.Source
            );

            return Ok(new
            {
                id = order.Id,
                orderNumber = order.OrderNumber,
                shipmentId = order.ShipmentId,
                totalAmount = order.TotalAmount,
                currency = order.Currency,
                paymentStatus = order.PaymentStatus.ToString(),
                source = order.Source,
                externalOrderNumber = order.ExternalOrderNumber
            });
        }


        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var loginId = GetLoginId();
            var orders = await _orderService.GetByLoginIdAsync(loginId);

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var loginId = GetLoginId();

            var order = await _orderService.GetByIdAsync(id);

            if (order == null)
                return NotFound(new { message = "Order not found." });

            if (!IsStaff() && order.LoginId != loginId)
                return Forbid();

            return Ok(order);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var loginId = GetLoginId();
                var ok = await _orderService.CancelAsync(id, loginId);

                if (!ok)
                    return BadRequest(new { message = "Cannot cancel this order." });

                return Ok(new { message = "Order cancelled successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "Employee,Admin,Administrator")]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var orders = await _orderService.GetActiveOrdersAsync();
            return Ok(orders);
        }

        [Authorize(Roles = "Admin,Administrator")]
        [HttpGet("client/{clientId:int}")]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            var orders = await _orderService.GetByClientIdAsync(clientId);
            return Ok(orders);
        }

        private int GetLoginId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(value, out var loginId))
                throw new UnauthorizedAccessException("Invalid user token.");

            return loginId;
        }

        private bool IsStaff()
        {
            var role =
                User.FindFirstValue(ClaimTypes.Role) ??
                User.FindFirstValue("role") ??
                "";

            return role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Administrator", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Employee", StringComparison.OrdinalIgnoreCase);
        }
    }
}