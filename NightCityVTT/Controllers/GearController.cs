using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NightCityVTT.Models;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GearController : ControllerBase
{
    private readonly IMongoCollection<GearItem> _gear;

    public GearController(IMongoClient mongoClient, IConfiguration configuration)
    {
        var dbName = configuration["MongoDB:DatabaseName"] ?? "NightCityVTT";
        var db = mongoClient.GetDatabase(dbName);
        _gear = db.GetCollection<GearItem>("StartingGear");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _gear.Find(_ => true).ToListAsync();
        var dtos = items.Select(i => new GearItemDto
        {
            Id = i.Id,
            Name = i.Name,
            Category = i.Category,
            ForRole = i.ForRole,
            Description = i.Description,
            CostEurobucks = i.CostEurobucks,
            Concealability = i.Concealability,
            Availability = i.Availability,
            Acc = i.Acc,
            Damage = i.Damage,
            Range = i.Range,
            Capacity = i.Capacity,
            ROF = i.ROF,
            SP = i.SP,
            EV = i.EV,
            WeightKg = i.WeightKg,
            Seats = i.Seats,
            TopSpeed = i.TopSpeed,
            SDP = i.SDP
        }).ToList();

        return Ok(dtos);
    }
}
