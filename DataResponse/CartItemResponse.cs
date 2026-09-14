using PizzaManagement.model;

namespace PizzaManagement.DataResponse
{
    public class CartItemResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PizzaId { get; set; }
        public int Quantity { get; set; }
        public PizzaResponse? Pizza { get; set; }
    }
}
