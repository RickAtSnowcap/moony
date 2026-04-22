using System.Text;
using Moony.Api.Models;

namespace Moony.Api.Services;

public static class MoonParser
{
    private static readonly Dictionary<string, int> RomanNumerals = new()
    {
        ["I"] = 1, ["II"] = 2, ["III"] = 3, ["IV"] = 4, ["V"] = 5,
        ["VI"] = 6, ["VII"] = 7, ["VIII"] = 8, ["IX"] = 9, ["X"] = 10,
        ["XI"] = 11, ["XII"] = 12, ["XIII"] = 13, ["XIV"] = 14, ["XV"] = 15,
        ["XVI"] = 16, ["XVII"] = 17, ["XVIII"] = 18, ["XIX"] = 19, ["XX"] = 20,
        ["XXI"] = 21, ["XXII"] = 22, ["XXIII"] = 23, ["XXIV"] = 24, ["XXV"] = 25,
        ["XXVI"] = 26, ["XXVII"] = 27, ["XXVIII"] = 28, ["XXIX"] = 29, ["XXX"] = 30
    };

    public static ParsedScan Parse(string rawLog)
    {
        var lines = rawLog.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        var moons = new List<ParsedMoon>();
        ParsedMoon? currentMoon = null;
        var oreOrder = 0;
        string? firstSystem = null;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Skip header row (first non-empty line that doesn't contain " - Moon ")
            if (i == 0 && !line.Contains(" - Moon ")) continue;

            if (line.Contains(" - Moon "))
            {
                // Moon header line
                currentMoon = ParseMoonHeader(line);
                if (currentMoon != null)
                {
                    firstSystem ??= currentMoon.SolarSystem;
                    moons.Add(currentMoon);
                    oreOrder = 0;
                }
            }
            else if (currentMoon != null)
            {
                // Ore line — tab-delimited
                var ore = ParseOreLine(line, oreOrder);
                if (ore != null)
                {
                    currentMoon.Ores.Add(ore);
                    if (ore.Rarity > currentMoon.Rarity)
                        currentMoon.Rarity = ore.Rarity;
                    oreOrder++;
                }
            }
        }

        // Sort moons by system, planet, moon number
        moons.Sort((a, b) =>
        {
            var cmp = string.Compare(a.SolarSystem, b.SolarSystem, StringComparison.OrdinalIgnoreCase);
            if (cmp != 0) return cmp;
            cmp = a.PlanetNumber.CompareTo(b.PlanetNumber);
            return cmp != 0 ? cmp : a.MoonNumber.CompareTo(b.MoonNumber);
        });

        var formatted = GenerateFormattedOutput(moons);
        var maxRarity = moons.Count > 0 ? moons.Max(m => m.Rarity) : 0;

        return new ParsedScan
        {
            Moons = moons,
            FormattedOutput = formatted,
            MaxRarity = maxRarity,
            FirstSystem = firstSystem ?? ""
        };
    }

    private static ParsedMoon? ParseMoonHeader(string line)
    {
        // Format: "SystemName RomanNumeral - Moon MoonNum"
        // Split on spaces
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 5) return null;

        var system = parts[0];
        var romanStr = parts[1];
        // parts[2] = "-"
        // parts[3] = "Moon"
        if (!int.TryParse(parts[4], out var moonNum)) return null;
        if (!RomanNumerals.TryGetValue(romanStr, out var planetNum)) return null;

        return new ParsedMoon
        {
            FullName = line,
            SolarSystem = system,
            PlanetNumber = planetNum,
            MoonNumber = moonNum
        };
    }

    private static ParsedMoonOre? ParseOreLine(string line, int order)
    {
        // Tab-delimited: OreType, Percentage, Quantity, OreTypeID, SolarSystemID, PlanetID, MoonID
        // Some formats may have a leading tab (empty first field)
        var fields = line.Split('\t');

        // Find the meaningful fields — skip empty leading fields
        var nonEmpty = new List<string>();
        foreach (var f in fields)
        {
            var trimmed = f.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                nonEmpty.Add(trimmed);
        }

        if (nonEmpty.Count < 3) return null;

        var oreType = nonEmpty[0];
        if (!double.TryParse(nonEmpty[1], System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var pctDecimal)) return null;

        // OreTypeID is at index 2 in real EVE scan data (no Quantity column)
        if (!int.TryParse(nonEmpty[2], out var oreTypeId)) return null;

        var percentage = (int)Math.Round(pctDecimal * 100);
        var rarity = OreInfo.GetRarity(oreTypeId);

        return new ParsedMoonOre
        {
            OreType = oreType,
            OreTypeId = oreTypeId,
            Percentage = percentage,
            Rarity = rarity,
            SortOrder = order
        };
    }

    private static string GenerateFormattedOutput(List<ParsedMoon> moons)
    {
        // Tab-delimited: Moon\tRarity\tSurvey
        // Survey = comma-separated OreType:Percentage% pairs
        var sb = new StringBuilder();
        foreach (var moon in moons)
        {
            var rarityLabel = moon.Rarity > 0 ? $"R{moon.Rarity}" : "R0";
            var survey = string.Join(", ", moon.Ores.Select(o => $"{o.OreType}:{o.Percentage}%"));
            sb.AppendLine($"{moon.FullName}\t{rarityLabel}\t{survey}");
        }
        return sb.ToString().TrimEnd();
    }
}
