namespace PizzaManagement.model
{
    public class Topping : BaseEntity
    {
        public string ? Name { get; set; }
        public List<Pizza> ? Pizzas { get; set; }

    }
}
