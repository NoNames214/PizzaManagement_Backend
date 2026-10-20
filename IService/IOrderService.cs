using PizzaManagement.DataRequest;
using PizzaManagement.DataResponse;
using PizzaManagement.model;

namespace PizzaManagement.IService
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrder (OrderRequest orderRequest, int userId);
        
        Task<bool> ProcessPayment (int orderId);

        Task<bool> UpdateOrder (int id, OrderRequest orderRequest);

        Task<bool> DeleteOrder(int id);

        Task<decimal> GetAmountOrder (int orderId); 

        Task<String> GetOrderStatus (int orderId, int userId);
    }
}
