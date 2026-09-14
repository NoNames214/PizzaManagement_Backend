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
    public class CategoryController : ControllerBase 
    {
        private readonly AppDbContext _context;
        public CategoryController (AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
        {
            var CategoryDto = await _context.Category
                .Where(c => !c.isDeleted)
                .AsNoTracking()
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Image = c.Image
                }).ToListAsync();
            return Ok(CategoryDto);
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponse>> GetCategories (int id)
        {

            var CategoryDto = await _context.Category
                .Where(c => !c.isDeleted)
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Image = c.Image
                }).FirstOrDefaultAsync();
            if (CategoryDto == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(CategoryDto);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CategoryRequest>> Add([FromForm]CategoryRequest categorydto)
        {
            var newCategory = new Category
            {
                Name = categorydto.Name
            };
            if (categorydto.ImageFile != null && categorydto.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(categorydto.ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImageCategory", fileName);
                var allowedExtensions = new[] { ".jpg", ".png" };
                var ext = Path.GetExtension(categorydto.ImageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    return BadRequest();
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await categorydto.ImageFile.CopyToAsync(stream);
                }
                newCategory.Image = "/ImageCategory/" + fileName;
            }
            _context.Category.Add(newCategory);
            await _context.SaveChangesAsync();
            var categoryRespone = new CategoryResponse
            {
                Id = newCategory.Id,
                Name = newCategory.Name,
                Image = newCategory.Image
            };
            return CreatedAtAction(
                nameof(GetCategories),
                new {id = newCategory.Id},
                categoryRespone
                );
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategories (int id)
        {
            var existingCategory = await _context.Category.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }
            else
            {
                existingCategory.isDeleted = true;
                existingCategory.DeleteAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")] 
        public async Task<IActionResult> UpdateCategories (int id, [FromForm]CategoryRequest categoryDto)
        {
            var CategoryInDb = await _context.Category.FindAsync(id);
            if (CategoryInDb == null)
            {
                return NotFound();
            }
            CategoryInDb.Name = categoryDto.Name;

            if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(CategoryInDb.Image))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", CategoryInDb.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryDto.ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImageCategory", fileName);
                var allowedExtensions = new[] { ".jpg", ".png" };
                var ext = Path.GetExtension(categoryDto.ImageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    return BadRequest();
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await categoryDto.ImageFile.CopyToAsync(stream);
                }
                CategoryInDb.Image = "/ImageCategory/" + fileName;
            }
            await _context.SaveChangesAsync();
            return NoContent();

        }
    }
}
