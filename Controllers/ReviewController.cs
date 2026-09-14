using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tensorboard;
using PizzaManagement.Data;
using PizzaManagement.DataRequest;
using PizzaManagement.DataResponse;
using PizzaManagement.model;

namespace PizzaManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetAll()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Pizza)
                .Where(r => r.UserId == userIdInt)
                .AsNoTracking()
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserResponse = new UserResponse
                    {
                        Id = r.User!.Id,
                        FullName = r.User.FullName,
                    },
                    PizzaId = r.PizzaId,
                    PizzaResponse = new PizzaResponse
                    {
                        Id = r.Pizza!.Id,
                        Name = r.Pizza.Name,
                        Price = r.Pizza.Price,
                        Image = r.Pizza.Image
                    },
                    OrderDetailsId = r.OrderDetailId,
                    OrderDetailsResponse = new OrderDetailsResponse
                    {
                        Id = r.OrderDetail!.Id,
                    },
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();
            return Ok(reviews);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> GetById(int id)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Pizza)
                .Where(r => r.Id == id && r.UserId == userIdInt)
                .AsNoTracking()
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserResponse = new UserResponse
                    {
                        Id = r.User!.Id,
                        FullName = r.User.FullName,
                    },
                    PizzaId = r.PizzaId,
                    PizzaResponse = new PizzaResponse
                    {
                        Id = r.Pizza!.Id,
                        Name = r.Pizza.Name,
                        Price = r.Pizza.Price,
                        Image = r.Pizza.Image
                    },
                    OrderDetailsId = r.OrderDetailId,
                    OrderDetailsResponse = new OrderDetailsResponse
                    {
                        Id = r.OrderDetail!.Id,
                    },
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).FirstOrDefaultAsync();
            if (review == null)
            {
                return NotFound();
            }
            return Ok(review);
        }
        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<ActionResult<ReviewRequest>> Create(ReviewRequest reviewRequest)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }
            var user = await _context.Users.FindAsync(userIdInt);
            var existingPizza = await _context.Pizza.FindAsync(reviewRequest.PizzaId);
            if (existingPizza == null)
            {
                return NotFound();
            }
            var orderDetail = await _context.OrderDetails
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == reviewRequest.OrderDetailsId &&
                x.Order!.UserId == userIdInt);
            if (orderDetail == null)
            {
                return BadRequest("Order detail is not found.");
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userIdInt && r.PizzaId == reviewRequest.PizzaId);
            if (existingReview != null)
            {
                return BadRequest("You have already reviewed this pizza.");
            }
            var newReview = new PizzaReview
            {
                UserId = userIdInt,
                PizzaId = reviewRequest.PizzaId,
                OrderDetailId = reviewRequest.OrderDetailsId,
                Rating = reviewRequest.Rating,
                Comment = reviewRequest.Comment,    
                CreatedAt = DateTime.UtcNow
            };
            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();
            var newReviewResponse = new ReviewResponse
            {
                Id = newReview.Id,
                UserId = userIdInt,
                UserResponse = new UserResponse
                {
                    Id = user!.Id,
                    FullName = user.FullName
                },
                PizzaId = newReview.PizzaId,
                PizzaResponse = new PizzaResponse
                {
                    Id = existingPizza.Id,
                    Name = existingPizza.Name,
                    Price = existingPizza.Price,
                    Image = existingPizza.Image
                },
                OrderDetailsId = newReview.OrderDetailId,
                OrderDetailsResponse = new OrderDetailsResponse
                {
                    Id = orderDetail.Id,
                    Quantity = orderDetail.Quantity,
                },
                Rating = newReview.Rating,
                Comment = newReview.Comment,
                CreatedAt = newReview.CreatedAt
            };
            return CreatedAtAction(
                nameof(GetById),
                new { id = newReview.Id },
                newReviewResponse
             );
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }
            var review = await _context.Reviews.
                FirstOrDefaultAsync(r => r.Id == id && r.UserId == userIdInt);
            if (review == null)
            {
                return NotFound();
            }
            else
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();
            }
            return NoContent();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Update(int id, ReviewRequest reviewRequest)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userIdInt);
            if (review == null)
            {
                return NotFound();
            }
            else
            {
                review.Rating = reviewRequest.Rating;
                review.Comment = reviewRequest.Comment;
                await _context.SaveChangesAsync();
                return Ok();
            }

        }
    }
}
