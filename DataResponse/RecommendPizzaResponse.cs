namespace PizzaManagement.DataResponse
{
    public class RecommendPizzaResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public double Score { get; set; }
    }
}
