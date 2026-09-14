namespace PizzaManagement.model
{
    public class UserInteraction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PizzaId { get; set; }
        public string ? InteractionType { get; set; }
        public float Reward { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsTrained { get; set; }
    }
}
