using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaManagement.Data;
using PizzaManagement.DataResponse;
using PizzaManagement.model;

namespace PizzaManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminController (AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("status/user-count")]
        public async Task<IActionResult> UserCount()
        {
            var userCount = await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "User")
                .CountAsync();
            return Ok(new
            {
                totalCount = userCount
            });
        }
        [HttpGet("user-list")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUser() 
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "User")
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Role = u.Role,
                    Email = u.Email,
                    FullName = u.FullName,
                    isLocked = u.isLocked
                }).ToListAsync();
            return Ok(user);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.isDeleted = true;
            user.DeleteAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPut("lock/{id}")]
        public async Task<IActionResult> LockUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.isLocked = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPut("unlock/{id}")]
        public async Task<IActionResult> UnlockUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.isLocked = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(DateTime ? startDate, DateTime ? endDate)
        {
            var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = endDate ?? DateTime.Now;

            var query = _context.Orders
                .AsNoTracking()
                .Where(o =>
                     o.status == OrderStatus.Paid &&
                     o.CreatedAt >= start &&
                     o.CreatedAt <= end).AsQueryable();

            var totalRevenue = await query.SumAsync(o => o.TotalPrice);
            var totalOrders = await query.CountAsync();

            var chart = await _context.Orders
                .AsNoTracking()
                .Where(o => o.status == OrderStatus.Paid
                && o.CreatedAt >= start && o.CreatedAt <= end)
                .GroupBy(o => new
                {
                    o.CreatedAt!.Value.Year,
                    o.CreatedAt.Value.Month,
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(o => o.TotalPrice),
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(new
            {
                FromDate = start,
                ToDate = end,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
                Chart = chart
            });
        }
    }
}
