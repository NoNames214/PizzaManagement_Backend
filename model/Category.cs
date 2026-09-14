namespace PizzaManagement.model
{
    public class Category : BaseEntity
    {
        public string ? Name { get; set; }
        public string? Image { get; set; }
        public List<Pizza> ? Pizzas { get; set; }
    }
}
