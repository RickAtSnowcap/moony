namespace Moony.Api.Models;

public sealed class ParsedMoonOre
{
    public string OreType { get; set; } = "";
    public int OreTypeId { get; set; }
    public int Percentage { get; set; }  // integer, e.g. 25 for 0.25
    public int Rarity { get; set; }
    public int SortOrder { get; set; }
}

public sealed class ParsedMoon
{
    public string FullName { get; set; } = "";
    public string SolarSystem { get; set; } = "";
    public int PlanetNumber { get; set; }
    public int MoonNumber { get; set; }
    public int Rarity { get; set; }
    public long SolarSystemId { get; set; }
    public long PlanetId { get; set; }
    public long EveMoonId { get; set; }
    public List<ParsedMoonOre> Ores { get; set; } = new();
}

public sealed class ParsedScan
{
    public List<ParsedMoon> Moons { get; set; } = new();
    public string FormattedOutput { get; set; } = "";
    public int MaxRarity { get; set; }
    public string FirstSystem { get; set; } = "";
}
