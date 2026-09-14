using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaManagement.Data;
using PizzaManagement.DataResponse;
using PizzaManagement.DataTranferObjects;
using PizzaManagement.model;
using PizzaManagement.Page;

namespace PizzaManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PizzaController : ControllerBase 
    {
        private readonly AppDbContext _context;
        public PizzaController(AppDbContext context) 
        {
            _context = context;
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<PizzaResponse>>> GetByCategoryId(
            [FromQuery] PaginationParams @params, int categoryId)
        {
            var pizzaDtos = _context.Pizza
                .Where(p => !p.isDeleted && p.CategoryId == categoryId)
                .AsNoTracking()
                .Select(p => new PizzaResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    IsGlutenFree = p.IsGlutenFree,
                    Sauce = p.Sauce,
                    CategoryName = p.Category != null ? p.Category.Name : "N/A",
                    MainIngredient = p.MainIngredient,
                    Image = p.Image,
                    ToppingNames = p.Toppings!.Select(t => t.Name!).ToList()
                }).OrderBy(p => p.Id);
            var pagedData = await pizzaDtos
                .Skip((@params.PageNumber - 1) * @params.PageSize)
                .Take(@params.PageSize)
                .ToListAsync();
            return Ok(pagedData);
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PizzaResponse>>> GetAll(
            [FromQuery] PaginationParams @params)
        {
            var pizzaDtos = _context.Pizza
                .Where(p => !p.isDeleted)
                .AsNoTracking()
                .Select(p => new PizzaResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    IsGlutenFree = p.IsGlutenFree,
                    Sauce = p.Sauce,
                    CategoryName = p.Category != null ? p.Category.Name : "N/A",
                    MainIngredient = p.MainIngredient,
                    Image = p.Image,
                    ToppingNames = p.Toppings!.Select(t => t.Name!).ToList()
                }).OrderBy(p => p.Id);
            var pagedData = await pizzaDtos
                .Skip((@params.PageNumber - 1) * @params.PageSize)
                .Take(@params.PageSize)
                .ToListAsync();
            return Ok(pagedData);
        }
        [Authorize(Roles = "Admin, User")]
        [HttpGet("{id}")] 
        public async Task<ActionResult<PizzaResponse>> GetPizza(int id)
        {
            var pizza = await _context.Pizza
                .Where(p => !p.isDeleted)
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PizzaResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    IsGlutenFree = p.IsGlutenFree,
                    Sauce = p.Sauce,
                    CategoryName = p.Category != null ? p.Category.Name : "N/A",
                    MainIngredient = p.MainIngredient,
                    Image = p.Image,
                    ToppingNames = p.Toppings!.Select(t => t.Name!).ToList()
                }).FirstOrDefaultAsync();
            if (pizza == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(pizza);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")] 
        public  async Task<IActionResult> DeletePizza(int id)
        {
            var pizza = await _context.Pizza.FindAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }
            else
            {
                pizza.isDeleted = true;
                pizza.DeleteAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return NoContent(); 
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost] 
        public async Task<ActionResult<PizzaResponse>> AddPizza (PizzaRequest dto)
        {
            var imagePizza = new[]
            {
                    "pizza1_jpg.webp",
                    "pizza2_jpg.webp",
                    "pizza3_jpg.webp",
                    "pizza4_jpg.webp",
                    "pizza5_jpg.webp",
                    "pizza6_jpg.webp",
             };
            var random = new Random();

            var newPizza = new Pizza
            {
                Name = dto.Name,
                Price = dto.Price,
                IsGlutenFree = dto.IsGlutenFree,
                Sauce = dto.Sauce,
                CategoryId = dto.CategoryId,
                Image = "/ImagePizza/" + imagePizza[random.Next(imagePizza.Length)],

                MainIngredient = new MainIngredient
                {
                    SpicinessLevel = random.Next(0, 5),
                    CrustType = random.Next(0, 6),
                    HasPineapple = false,
                    HasSeafood = false,
                    HasCheese = true,
                    PriceRange = (int)dto.Price,
                    Size = random.Next(1, 3)
                }
            };
            var selectedToppings = new List<Topping>();
            if (dto.ToppingId != null && dto.ToppingId!.Any())
            {
                selectedToppings = await _context.Topping
                    .Where(t => dto.ToppingId!.Contains(t.Id))
                    .ToListAsync();
                newPizza.Toppings = selectedToppings;
            }

            _context.Pizza.Add(newPizza);
            await _context.SaveChangesAsync();
            await _context.Entry(newPizza).Reference(c => c.Category).LoadAsync();
            await _context.Entry(newPizza).Collection(t => t.Toppings!).LoadAsync();
            var pizzaRespone = new PizzaResponse
            {
                Id = newPizza.Id,
                Name = newPizza.Name,
                Price = newPizza.Price,
                IsGlutenFree = newPizza.IsGlutenFree,
                Sauce = newPizza.Sauce,
                CategoryName = newPizza.Category?.Name ?? "N/A",
                Image = newPizza.Image,
                ToppingNames = newPizza.Toppings?.Select(t => t.Name!).ToList() ?? new List<string>()
            };
            return CreatedAtAction(
                nameof(GetPizza),
                new { id = newPizza.Id},
                pizzaRespone
                );
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PizzaRequest dto)
        {
            var pizzaInDb = await _context.Pizza
                .Include(p => p.Toppings)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pizzaInDb == null)
            {
                return NotFound();
            }
            pizzaInDb.Name = dto.Name;
            pizzaInDb.Price = dto.Price;
            pizzaInDb.IsGlutenFree = dto.IsGlutenFree;
            pizzaInDb.Sauce = dto.Sauce;
            pizzaInDb.CategoryId = dto.CategoryId;

            pizzaInDb.Toppings!.Clear();
            if (dto.ToppingId != null && dto.ToppingId.Any()) {
                var newToppings = await _context.Topping    
                   .Where(t => dto.ToppingId!.Contains(t.Id))
                   .ToListAsync();
                pizzaInDb.Toppings = newToppings;
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpGet("search")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<IEnumerable<PizzaResponse>>> SearchPizza(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest("KeyWord is required");
            }
            keyword = keyword.Trim();
            const string collection = "Latin1_General_CI_AI";
            var pizzas = await _context.Pizza
                .Where(p => !p.isDeleted && EF.Functions.Collate(p.Name!, collection)
                .Contains(keyword))
                .AsNoTracking()
                .Select(p => new PizzaResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    IsGlutenFree = p.IsGlutenFree,
                    Sauce = p.Sauce,
                    CategoryName = p.Category != null ? p.Category.Name : "N/A",
                    MainIngredient = p.MainIngredient,
                    Image = p.Image,
                    ToppingNames = p.Toppings!.Select(t => t.Name!).ToList()
                }).ToListAsync();
            return Ok(pizzas);
        }
    }
}
