namespace PizzaManagement.IService
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(int orderId, decimal amount, HttpContext context);

        bool PaymentCallBack(IQueryCollection collections);
    }
}
