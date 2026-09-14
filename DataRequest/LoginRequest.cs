using System.ComponentModel.DataAnnotations;

namespace PizzaManagement.DataTranferObjects
{
    public class LoginRequest
    {
        [Required]
        public string ? UserName { get; set; }    
        [Required]
        public string ? PassWord { get; set; }
    }
}
