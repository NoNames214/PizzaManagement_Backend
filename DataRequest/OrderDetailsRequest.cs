using System.ComponentModel.DataAnnotations;

namespace PizzaManagement.DataRequest
{
    public class OrderDetailsRequest
    {
        [Required]
        public int PizzaId { get; set; }
        [Range (1, 30)]
        public int Quantity { get; set; }
    }
}
