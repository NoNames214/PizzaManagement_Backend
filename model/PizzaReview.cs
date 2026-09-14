namespace PizzaManagement.model
{
    public class PizzaReview
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int PizzaId { get; set; }
        public Pizza? Pizza { get; set; }
        public int OrderDetailId { get; set; }
        public OrderDetail ? OrderDetail { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
