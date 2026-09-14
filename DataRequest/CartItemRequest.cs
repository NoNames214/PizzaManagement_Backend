using System.ComponentModel.DataAnnotations;

namespace PizzaManagement.DataRequest
{
    public class CartItemRequest
    {
        [Required]
        public int PizzaId { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
