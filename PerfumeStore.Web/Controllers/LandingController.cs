using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Services;

namespace PerfumeStore.Web.Controllers;

public class LandingController : Controller
{
    private readonly LandingPageService _landingPageService;

    public LandingController(LandingPageService landingPageService)
    {
        _landingPageService = landingPageService;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _landingPageService.BuildLandingPageAsync(cancellationToken);
        return View(model);
    }
}

