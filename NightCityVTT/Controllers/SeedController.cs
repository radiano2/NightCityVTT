using Microsoft.AspNetCore.Mvc;
using NightCityVTT.Services;

namespace NightCityVTT.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly DatabaseSeeder _seeder;

    public SeedController(DatabaseSeeder seeder)
    {
        _seeder = seeder;
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunSeed()
    {
        var report = await _seeder.SeedInitialData();
        return report.Success ? Ok(report) : StatusCode(500, report);
    }

    [HttpGet("validate")]
    public async Task<IActionResult> Validate()
    {
        var report = await _seeder.ValidateDatabase();
        return report.Success ? Ok(report) : StatusCode(500, report);
    }
}
