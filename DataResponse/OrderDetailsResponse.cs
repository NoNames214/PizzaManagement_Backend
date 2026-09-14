using PizzaManagement.model;

namespace PizzaManagement.DataResponse
{
    public class OrderDetailsResponse
    {
        public int Id { get; set; }
        public int PizzaId { get; set; }
        public string ? PizzaName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public PizzaResponse ? pizza { get; set; }
    }
}
