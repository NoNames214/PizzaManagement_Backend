using System.ComponentModel.DataAnnotations;

namespace PizzaManagement.DataRequest
{
    public class RegisterRequest
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? PassWord { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        
    }
}
