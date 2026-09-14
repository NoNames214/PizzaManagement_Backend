namespace PizzaManagement.DataResponse
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public string ? Status { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.Now;
        public ICollection<OrderDetailsResponse> ? DetailsResponses { get; set; }

    }
}
