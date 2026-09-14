using PizzaManagement.model;

namespace PizzaManagement.DataResponse
{
    public class PizzaResponse
    {
        public int Id { get; set; }
        public string ? Name { get; set; }
        public decimal Price { get; set; }
        public bool IsGlutenFree { get; set; }
        public string? Sauce { get; set; }
        public string? Image { get; set; }
        public string ? CategoryName { get; set; }
        public MainIngredient ? MainIngredient { get; set; }
        public List<string> ? ToppingNames { get; set; }
    }
}
