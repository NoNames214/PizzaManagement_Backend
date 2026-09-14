using PizzaManagement.DataRequest;
using PizzaManagement.DataResponse;

namespace PizzaManagement.service
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrder (OrderRequest orderRequest, int userId);
        
        Task<bool> ProcessPayment (int orderId);

        Task<bool> UpdateOrder (int id, OrderRequest orderRequest);

        Task<bool> DeleteOrder(int id);
    }
}
