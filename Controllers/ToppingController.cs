using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaManagement.Data;
using PizzaManagement.DataResponse;
using PizzaManagement.DataTranferObjects;
using PizzaManagement.model;

namespace PizzaManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ToppingController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ToppingController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToppingResponse>>> GetAll()
        {
            var ToppingDto = await _context.Topping
                .Where(t => !t.isDeleted)
                .AsNoTracking()
                .Select(t => new ToppingResponse
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToListAsync();
            return Ok(ToppingDto);
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ToppingResponse>> GetToppings(int id)
        {

            var ToppingDto = await _context.Topping

                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new ToppingResponse
                {
                    Id = t.Id,
                    Name = t.Name
                }).FirstOrDefaultAsync();
            if (ToppingDto == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(ToppingDto);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ToppingResponse>> Add(ToppingRequest toppingDto)
        {
            var newTopping = new Topping
            {
                Name = toppingDto.Name
            };
            _context.Topping.Add(newTopping);
            await _context.SaveChangesAsync();
            var toppingRespone = new ToppingResponse
            {
                Id = newTopping.Id,
                Name = newTopping.Name
            };
            return CreatedAtAction(
                nameof(GetToppings),
                new { id = newTopping.Id },
                toppingRespone
                );
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopping(int id)
        {
            var existingTopping = await _context.Topping.FindAsync(id);
            if (existingTopping == null)
            {
                return NotFound();
            }
            else
            {
                existingTopping.isDeleted = true;
                existingTopping.DeleteAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTopping(int id, ToppingRequest toppingDto)
        {
            var ToppingInDb = await _context.Topping.FindAsync(id);
            if (ToppingInDb == null)
            {
                return NotFound();
            }
            else
            {
                ToppingInDb.Name = toppingDto.Name;
                await _context.SaveChangesAsync();
                return NoContent();
            }

        }
    }
}
