using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaManagement.Data;
using PizzaManagement.DataRequest;
using PizzaManagement.DataResponse;
using PizzaManagement.IService;
using PizzaManagement.model;

namespace PizzaManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOrderService _orderService;
        public OrderController(AppDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll()
        {
            int user = int.Parse(User.FindFirst("UserId")!.Value);
            var orderDto = await _context.Orders!
                .Include(o => o.Details)!.ThenInclude(od => od.Pizza)
                        .ThenInclude(p => p!.Category)
                .Where(o => o.UserId == user)
                .AsNoTracking()
                .Select(x => new OrderResponse
                {
                    Id = x.Id,
                    TotalPrice = x.TotalPrice,
                    Status = x.status,
                    PaidAt = x.PaidAt,
                    DetailsResponses = x.Details!.Select(od => new OrderDetailsResponse
                    {
                        Id = od.Id,
                        PizzaName = od.Pizza!.Name,
                        PriceAtPurchase = od.PriceAtPurchase,
                        Quantity = od.Quantity,
                        pizza = new PizzaResponse
                        {
                            Id = od.Pizza.Id,
                            Name = od.Pizza.Name,
                            Price = od.Pizza.Price,
                            Image = od.Pizza.Image,
                            CategoryName = od.Pizza.Category!.Name
                        }
                    }).ToList()
                }).ToListAsync();
            return Ok(orderDto);
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponse>> GetById(int id)
        {
            var user = int.Parse(User.FindFirst("UserId")!.Value);
            var orderDto = await _context.Orders
                .Where (o => o.UserId == user)
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new OrderResponse
                {
                    Id = x.Id,
                    TotalPrice = x.TotalPrice,
                    Status = x.status,
                    PaidAt = x.PaidAt,
                    DetailsResponses = x.Details!.Select(od => new OrderDetailsResponse
                    {
                        Id = od.Id,
                        PizzaName = od.Pizza!.Name,
                        PriceAtPurchase = od.PriceAtPurchase,
                        Quantity = od.Quantity,
                        pizza = new PizzaResponse
                        {
                            Id = od.Pizza.Id,
                            Name = od.Pizza.Name,
                            Price = od.Pizza.Price,
                            Image = od.Pizza.Image,
                            CategoryName = od.Pizza.Category!.Name
                        }
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (orderDto == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(orderDto);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder (int id)
        {
            await _orderService.DeleteOrder(id);
            return NoContent();
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> AddOrder(OrderRequest orderRequest)
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);
            var result = await _orderService.CreateOrder(orderRequest, userId);
            return CreatedAtAction(
                nameof(GetById),
                new {id = result.Id},
                result
                );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder (int id, OrderRequest orderRequest )
        {
            await _orderService.UpdateOrder(id, orderRequest);
            return NoContent();
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> ProcessPayment (int id)
        {
            await _orderService.ProcessPayment(id);
            return NoContent();
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet("{orderId}/status")]
        public async Task<ActionResult<string>> GetOrderStatus(int orderId)
        {
            int userId = int.Parse(User.FindFirst("UserId")!.Value);
            var status = await _orderService.GetOrderStatus(orderId, userId);
            return Ok(status);
        }
    }
}
