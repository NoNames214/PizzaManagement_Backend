using PizzaManagement.Helpers;
using PizzaManagement.IService;

namespace PizzaManagement.Service
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(int orderId, decimal amount, HttpContext context)
        {
            var pay = new VnPayLibrary();
            var timeNow = DateTime.Now;

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]!);
            pay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString()); 
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"Pay for the order #{orderId}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", _configuration["VnPay:ReturnUrl"]!);
            pay.AddRequestData("vnp_TxnRef", orderId.ToString());

            return pay.CreateRequestUrl(_configuration["VnPay:VnPayUrl"]!, _configuration["VnPay:HashSecret"]!);
        }

        public bool PaymentCallBack(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(value) && key.StartsWith("vnp_"))
                {
                    pay.AddResponseData(key, value!);
                }
            }

            var vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;

            bool checkSignature = pay.ValidateSignature(vnp_SecureHash!, _configuration["VnPay:HashSecret"]!);
            return checkSignature && vnp_ResponseCode == "00";
        }
    }
}
