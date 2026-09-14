using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaManagement.Data;
using PizzaManagement.DataRequest;
using PizzaManagement.DataResponse;
using PizzaManagement.model;

namespace PizzaManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemResponse>>> GetAll()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }
            var cart = await _context.CartItems
                .Include(c => c.Pizza)
                .Where(c => c.UserId == userIdInt)
                .AsNoTracking()
                .Select(s => new CartItemResponse
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    PizzaId = s.PizzaId,
                    Quantity = s.Quantity,
                    Pizza = new PizzaResponse
                    {
                        Id = s.Pizza!.Id,
                        Name = s.Pizza.Name,
                        Price = s.Pizza.Price,
                        Image = s.Pizza.Image
                    }
                }).ToListAsync();
            return Ok(cart);
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetbyId (int id)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }
            var cart = await _context.CartItems
                .Include(c => c.Pizza)
                .Where(c => c.UserId == userIdInt && c.Id == id)
                .AsNoTracking()
                .Select(s => new CartItemResponse
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    PizzaId = s.PizzaId,
                    Quantity = s.Quantity,
                    Pizza = new PizzaResponse
                    {
                        Id = s.Pizza!.Id,
                        Name = s.Pizza.Name,
                        Price = s.Pizza.Price,
                        Image = s.Pizza.Image
                    }
                }).FirstOrDefaultAsync();
            return Ok(cart);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cart = await _context.CartItems.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            _context.CartItems.Remove(cart);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<CartItemRequest>> Add(CartItemRequest cartItemRequest)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userIdInt && c.PizzaId == cartItemRequest.PizzaId);
            CartItem cartItem;
            if (existingItem != null)
            {
                existingItem.Quantity += cartItemRequest.Quantity;
                cartItem = existingItem;
            }
            else
            {
                cartItem = new CartItem
                {
                    PizzaId = cartItemRequest.PizzaId,
                    UserId = int.Parse(userId),
                    Quantity = cartItemRequest.Quantity,
                };
                _context.CartItems.Add(cartItem);
            }
            await _context.SaveChangesAsync();
            var cartItemResponse = new CartItemResponse
            {
                Id = cartItem.Id,
                PizzaId = cartItem.PizzaId,
                UserId = cartItem.UserId,
                Quantity = cartItem.Quantity,
            };
            return CreatedAtAction(
                nameof(GetbyId),
                new { id = cartItem.Id },
                cartItemResponse
            );
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, CartItemRequest request)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userIdInt && c.Id == id);
            if (cartItem == null)
            {
                return NotFound();
            }
            else
            {
                cartItem.Quantity = request.Quantity;
                await _context.SaveChangesAsync();
                return Ok();
            }
        }
    }
}
