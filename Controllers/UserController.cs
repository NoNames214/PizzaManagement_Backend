using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaManagement.Data;
using PizzaManagement.DataRequest;
using PizzaManagement.DataResponse;

namespace PizzaManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController (AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("profile")]
        public async Task<ActionResult<UserResponse>> GetAll()
        {
            var userId = User.FindFirst("UserId")!.Value;
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == int.Parse(userId))
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Address = u.Address,
                    AvatarUrl = u.AvatarUrl,
                    Email = u.Email,
                    FullName = u.FullName,
                    Role = u.Role,
                    UserName = u.UserName,
                }).FirstOrDefaultAsync();
            return Ok(user);
        }
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile (ProfileRequest profileRequest)
        {
            var userId = User.FindFirst("UserId")!.Value;
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }
            user.FullName = profileRequest.FullName;
            user.Email = profileRequest.Email;
            user.Address = profileRequest.Address;
            user.AvatarUrl = profileRequest.AvatarUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = User.FindFirst("UserId")!.Value;
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }
            user.isDeleted = true;
            user.DeleteAt = DateTime.UtcNow; 
            user.FullName = "Deleted User";
            user.Email = $"deleted_{userId}@pizza.com";
 
            user.AvatarUrl = null;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
