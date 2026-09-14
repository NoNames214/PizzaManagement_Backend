namespace PizzaManagement.model
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public bool isDeleted { get; set; } = false;
        public DateTime ? DeleteAt { get; set; }
        public DateTime ? CreatedAt { get; set; } = DateTime.Now;
    }
}