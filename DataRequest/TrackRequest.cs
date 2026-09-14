using System.ComponentModel.DataAnnotations;

namespace PizzaManagement.DataRequest
{
    public class TrackRequest
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int PizzaId { get; set; }
        [Required]
        public string? EventType { get; set; }
    }
}
