namespace NightCityVTT.Shared.Models;

public class GearItemDto
{
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ForRole { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CostEurobucks { get; set; }
    public string Concealability { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public string Acc { get; set; } = string.Empty;
    public string Damage { get; set; } = string.Empty;
    public string Range { get; set; } = string.Empty;
    public string Capacity { get; set; } = string.Empty;
    public string ROF { get; set; } = string.Empty;
    public string SP { get; set; } = string.Empty;
    public string EV { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public string? HumanityCostDice { get; set; }  // null = no humanity cost; "2d6" = roll 2d6 on install

    public string Seats { get; set; } = string.Empty;
    public string TopSpeed { get; set; } = string.Empty;
    public string SDP { get; set; } = string.Empty;
}
