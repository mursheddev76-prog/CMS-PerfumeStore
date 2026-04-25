using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using PerfumeStore.Web.Models;
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
        var model = await _landingPageService.BuildLandingPageAsync(User.Identity?.Name, cancellationToken);
        return View(model);
    }

    [HttpGet("/Landing/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
