namespace PizzaManagement.DataResponse
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserResponse ? UserResponse { get; set; }
        public int PizzaId { get; set; }
        public PizzaResponse ? PizzaResponse { get; set; }
        public int OrderDetailsId { get; set; }
        public OrderDetailsResponse ? OrderDetailsResponse { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
