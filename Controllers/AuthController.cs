using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PizzaManagement.Data;
using PizzaManagement.DataRequest;
using PizzaManagement.DataTranferObjects;
using PizzaManagement.model;

namespace PizzaManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _context.Users.
                FirstOrDefaultAsync(u => u.UserName == model.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.PassWord, user.PasswordHash))
            {
                return Unauthorized();
            }
            if (user.isLocked)
            {
                return BadRequest("Your account has been locked.");
            }
            else
            {
                var token = GenerateJwtToken(user);
                var refreshToken = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString(),
                    Expires = DateTime.Now.AddDays(7),
                    CreatedAt = DateTime.Now,
                    UserId = user.Id
                };
                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Token = token,
                    expireAt = DateTime.Now.AddHours(3),
                    username = model.UserName,
                    RefreshToken = refreshToken.Token
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest userdto)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == userdto.UserName))
            {
                return BadRequest();
            }
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userdto.PassWord);
            var newUser = new User
            {
                UserName = userdto.UserName,
                Email = userdto.Email,
                FullName = userdto.FullName,
                AvatarUrl = null,
                PasswordHash = passwordHash,
                Role = "User"

            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(newUser);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(u => u.User)
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);
            if (refreshToken == null || refreshToken.Expires < DateTime.Now)
            {
                return Unauthorized();
            }
            var newToken = GenerateJwtToken(refreshToken.User!);
            return Ok(new
            {
                AccessToken = newToken
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Role, user.Role!),
                new Claim("UserId", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: cred
             );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
