namespace PizzaManagement.model
{
    public class Pizza : BaseEntity
    {
        public string ? Name { get; set; }
        public bool IsGlutenFree { get; set; }
        public decimal Price { get; set; }
        public string ? Sauce { get; set; }
        public string ? Image { get; set; }
        public int CategoryId { get; set; }
        public Category ? Category { get; set; }
        public MainIngredient ? MainIngredient { get; set; }
        public List<Topping>? Toppings { get; set; }
    }
}
 