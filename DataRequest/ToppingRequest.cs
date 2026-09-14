using System.ComponentModel.DataAnnotations;

namespace PizzaManagement.DataTranferObjects
{
    public class ToppingRequest
    {
        [Required]
        public string ? Name { get; set; }
    }
}
