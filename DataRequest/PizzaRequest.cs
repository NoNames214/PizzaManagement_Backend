using System.ComponentModel.DataAnnotations;
using PizzaManagement.model;

namespace PizzaManagement.DataTranferObjects
{
    public class PizzaRequest
    {
        [Required]
        public string ? Name { get; set; }
        [Range(1, 100000)]
        public decimal Price { get; set; }
        public bool IsGlutenFree { get; set; }
        public string? Sauce { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [MinLength(1)]
        public List<int>? ToppingId { get; set; }

    }
}
