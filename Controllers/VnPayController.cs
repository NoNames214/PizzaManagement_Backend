using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaManagement.IService;

namespace PizzaManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class VnPayController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IOrderService _orderService;

        public VnPayController(IVnPayService vnPayService, IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _orderService = orderService;
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost("create-payment-url/{orderId}")]
        public async Task<IActionResult> CreatePaymentUrl(int orderId)
        {
            decimal amount = await _orderService.GetAmountOrder(orderId);
            var paymentUrl = _vnPayService.CreatePaymentUrl(orderId, amount, HttpContext);
            return Ok(new { PaymentUrl = paymentUrl });
        }


        [AllowAnonymous]
        [HttpGet("PaymentCallBack")]
        public async Task<IActionResult> PaymentCallback()
        {
            var reponse = _vnPayService.PaymentCallBack(Request.Query);
            var orderId = Request.Query["vnp_TxnRef"];
            var vnp_ResponseCode = Request.Query["vnp_ResponseCode"];

            if (reponse && vnp_ResponseCode == "00")
            {
                await _orderService.ProcessPayment(int.Parse(orderId!));

                return Redirect($"pizzamanagement://paymentcallback?status=success&orderId={orderId}");
            }

            return Redirect($"pizzamanagement://paymentcallback?status=failed&orderId={orderId}");
        }
    }
}
