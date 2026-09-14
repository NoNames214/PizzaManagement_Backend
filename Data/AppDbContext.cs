using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using PizzaManagement.model;

namespace PizzaManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> Options) 
            : base(Options)
        {

        }
        public DbSet<Pizza> Pizza { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Topping> Topping { get; set; } 
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserInteraction> UserInteractions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<PizzaReview> Reviews { get; set; }
        public static async Task SeedData (AppDbContext context)
        {
            if (!context.Pizza.Any())
            {
                var random = new Random();
                var pizzas = new List<Pizza>();
                var allTopping = await context.Topping.ToListAsync();
                var allCategories = await context.Category.ToListAsync();
                var imagePizza = new[] 
                {
                    "pizza1_jpg.webp",
                    "pizza2_jpg.webp",
                    "pizza3_jpg.webp",
                    "pizza4_jpg.webp",
                    "pizza5_jpg.webp",
                    "pizza6_jpg.webp",
                    "pizza7_jpg.webp",
                    "pizza8_jpg.webp",
                    "pizza9_jpg.webp",
                    "pizza10_jpg.webp",
                    "pizza11_jpg.webp",
                    "pizza12_jpg.webp",
                    "pizza13_jpg.webp",
                    "pizza14_jpg.webp",
                    "pizza15_jpg.webp"
                };
                for (int i = 1; i <= 100; i++)
                {
                    var randomTopping = allTopping.OrderBy(x => random.Next()).Take(3).ToList();
                    pizzas.Add(new Pizza
                    {
                        Name = $"Pizza{i}",
                        Price = 100000 + (i * 5000),
                        IsGlutenFree = i % 2 == 0,
                        Sauce = i % 3 == 0 ? "Honey" : "BBQ",
                        CategoryId = allCategories[random.Next(allCategories.Count)].Id,
                        Toppings = randomTopping,
                        Image =  "/ImagePizza/" + imagePizza[random.Next(imagePizza.Length)],
                        MainIngredient = new MainIngredient
                        {
                            SpicinessLevel = random.Next(0, 5),
                            CrustType = random.Next(0, 6),
                            HasPineapple = i % 5 == 0,
                            HasSeafood = i % 3 == 0,
                            HasCheese = i % 4 == 0,
                            PriceRange = 100000 + (i * 5000),
                            Size = random.Next(1, 3)
                        }
                    });
                }
                context.AddRange(pizzas);
                await context.SaveChangesAsync();
            }
        }
        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
                }
            }
            modelBuilder.Entity<Pizza>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Pizza>()
                .HasMany(p => p.Toppings)
                .WithMany(t => t.Pizzas)
                .UsingEntity(j => j.ToTable("PizzaToppings"));
            modelBuilder.Entity<Pizza>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Pizzas)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserInteraction>()
                .HasOne<User>() 
                .WithMany()
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserInteraction>()
                .HasOne<Pizza>()
                .WithMany()
                .HasForeignKey(p => p.PizzaId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(o => o.Order)
                .WithMany(or => or.Details)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(o => o.Pizza)
                .WithMany()
                .HasForeignKey(o => o.PizzaId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderDetail>()
                .Property(o => o.PriceAtPurchase)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Pizza>().OwnsOne(p => p.MainIngredient);
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Pizza)
                .WithMany()
                .HasForeignKey(c => c.PizzaId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PizzaReview>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);
            modelBuilder.Entity<PizzaReview>()
                .HasOne(r => r.Pizza)
                .WithMany()
                .HasForeignKey(r => r.PizzaId);
            modelBuilder.Entity<PizzaReview>()
                .HasIndex(r => new { r.UserId, r.PizzaId })
                .IsUnique();
        }
        private static LambdaExpression ConvertFilterExpression (Type type)
        {
            var parameter = Expression.Parameter(type, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.isDeleted));
            var falseContant = Expression.Constant(false); 
            var comparison = Expression.Equal(property, falseContant);
            return Expression.Lambda(comparison, parameter);
        }
    }
}
