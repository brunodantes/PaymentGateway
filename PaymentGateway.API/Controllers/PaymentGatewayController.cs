using PaymentGateway.Contract.Requests;
using PaymentGateway.Domain.Services;
using PaymentGateway.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace PaymentGateway.API.Controllers;

[ApiController]
[Route("payment")]
public class PaymentGatewayController(IPaymentGatewayService paymentGatewayService) : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService = paymentGatewayService;

    [HttpGet]
    [ProducesResponseType<PaymentModel>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetPaymentDetail([FromQuery][Required] Guid paymentId)
    {
        var result = await _paymentGatewayService.GetPaymentDetails(paymentId);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<PaymentModel>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> PublishPaymentRequest([FromBody][Required] PaymentRequest paymentRequest)
    {
        var result = await _paymentGatewayService.AddPayment(paymentRequest);

        return Ok(result);
    }
}
