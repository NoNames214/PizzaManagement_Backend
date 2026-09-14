using PizzaManagement.model;

namespace PizzaManagement.DataRequest
{
    public class OrderRequest
    {
        public ICollection<OrderDetailsRequest>? detailsRequests { get; set; }

    }
}
