using System.ComponentModel.DataAnnotations;

namespace PizzaManagement.DataRequest
{
    public class ReviewRequest
    {
        [Required]
        public int PizzaId { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; } 
        [Required]
        public int OrderDetailsId { get; set; }
        public string? Comment { get; set; }
    }
}
