using Microsoft.EntityFrameworkCore;
using PizzaManagement.Data;
using PizzaManagement.DataRequest;
using PizzaManagement.DataResponse;
using PizzaManagement.IService;
using PizzaManagement.model;
using PizzaManagement.Service;

namespace PizzaManagement.service
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly RecommendationService _recommendationService;
        public OrderService (AppDbContext context, RecommendationService recommendationService)
        {
            _context = context;
            _recommendationService = recommendationService;
        }

        public async Task<OrderResponse> CreateOrder(OrderRequest orderRequest, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pizzaId = orderRequest.detailsRequests!.Select(d => d.PizzaId).Distinct().ToList();
                var priceDictionary = await _context.Pizza
                    .Where(p => pizzaId.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Price);
                var order = new Order
                {
                    UserId = userId,
                    status = OrderStatus.Pending,                 
                    Details = new List<OrderDetail>(),
                    CreatedAt = DateTime.Now,
                };
                decimal totalAmount = 0;
                foreach (var d in orderRequest.detailsRequests!)
                {
                    if (!priceDictionary.ContainsKey(d.PizzaId))
                        throw new Exception($"Pizza {d.PizzaId} is not found");

                    order.Details.Add(new OrderDetail
                    {
                        PizzaId = d.PizzaId,
                        Quantity = d.Quantity,
                        PriceAtPurchase = priceDictionary[d.PizzaId]
                    });
                    totalAmount += d.Quantity * priceDictionary[d.PizzaId];
                }
                _context.Orders.Add(order);
                order.TotalPrice = totalAmount;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OrderResponse
                {
                    Id = order.Id,
                    TotalPrice = totalAmount,
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteOrder(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    throw new Exception("Order is not found");
                }
                order.isDeleted = true;
                order.DeleteAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<decimal> GetAmountOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Order is not found");
            }
            else if (order.status != OrderStatus.Pending) 
            {
                throw new Exception("Order is not pending");
            }
            return order.TotalPrice;
        }

        public async Task<string> GetOrderStatus(int orderId, int userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null)
            {
                throw new Exception("Order is not found");
            }
            return order.status!;
        }

        public async Task<bool> ProcessPayment(int orderId)
        {
            using var transactions = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Details)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
                if (order == null)
                {
                    throw new Exception("No order exists");
                }
                else if (order.status != OrderStatus.Pending)
                {
                    throw new Exception("Order is not pending");
                }
        
                order.status = OrderStatus.Paid;
                order.PaidAt = DateTime.Now;
                foreach (var detail in order.Details!)
                {
                    await _recommendationService.TrackActionAsync(order.UserId, detail.PizzaId, "purchase");
                }
                await _context.SaveChangesAsync();
                await transactions.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transactions.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrder(int id, OrderRequest orderRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orderInDb = await _context.Orders
                    .Include(o => o.Details)
                    .FirstOrDefaultAsync(o => o.Id == id);
                if (orderInDb == null)
                {
                    throw new Exception("Order not found");
                }
                var pizzaId = orderRequest.detailsRequests!
                    .Select(p => p.PizzaId).Distinct().ToList();

                var pirceDictionary = await _context.Pizza
                    .Where(p => pizzaId.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Price);

                foreach (var item in orderRequest.detailsRequests!)
                {
                    if (!pirceDictionary.ContainsKey(item.PizzaId))
                    {
                        throw new Exception($"Pizza {item.PizzaId} is not found");
                    }
                }
                _context.OrderDetails.RemoveRange(orderInDb.Details!);
                if (orderInDb.status != OrderStatus.Pending)
                {
                    throw new Exception("Only pending orders can be updated.");
                }
                orderInDb.Details = orderRequest.detailsRequests!
                .Select(d => new OrderDetail
                {
                    PizzaId = d.PizzaId,
                    Quantity = d.Quantity,
                    PriceAtPurchase = pirceDictionary[d.PizzaId],
                }).ToList();

                orderInDb.TotalPrice = orderInDb.Details.Sum(d => d.Quantity * d.PriceAtPurchase);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }   
    }
}
