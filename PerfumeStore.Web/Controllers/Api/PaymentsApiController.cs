using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Data.Repositories;

namespace PerfumeStore.Web.Controllers.Api;

[ApiController]
[Route("api/configuration")]
public class PaymentsApiController : ControllerBase
{
    private readonly ICommerceRepository _repository;

    public PaymentsApiController(ICommerceRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods(CancellationToken cancellationToken)
    {
        var payments = await _repository.GetPaymentMethodsAsync(cancellationToken);
        return Ok(payments);
    }

    [HttpGet("delivery-options")]
    public async Task<IActionResult> GetDeliveryOptions(CancellationToken cancellationToken)
    {
        var delivery = await _repository.GetDeliveryOptionsAsync(cancellationToken);
        return Ok(delivery);
    }
}

