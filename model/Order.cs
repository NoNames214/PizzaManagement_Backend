using Microsoft.EntityFrameworkCore;

namespace PizzaManagement.model
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public string ? status { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.Now;
        public ICollection<OrderDetail> ? Details { get; set; }

    }
}
