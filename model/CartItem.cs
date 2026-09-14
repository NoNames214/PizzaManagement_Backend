namespace PizzaManagement.model
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PizzaId { get; set; }
        public int Quantity { get; set; }
        public Pizza ? Pizza { get; set; }

    }
}
