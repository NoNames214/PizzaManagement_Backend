using System.ComponentModel.DataAnnotations;

namespace PizzaManagement.DataTranferObjects
{
    public class CategoryRequest
    {
        [Required]
        public string ? Name { get; set; }
        [Required]
        public IFormFile ? ImageFile { get; set; }
    }
}
