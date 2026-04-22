using Npgsql;
using Moony.Api.Models;

namespace Moony.Api.Services;

public sealed class ScanRepository
{
    private readonly string _connectionString;

    public ScanRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> SaveScanAsync(string rawLog, ParsedScan parsed, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        // Insert scan
        await using var scanCmd = new NpgsqlCommand("SELECT moony.fn_scan_insert(@raw, @fmt, @cnt, @rar)", conn, tx);
        scanCmd.Parameters.AddWithValue("raw", rawLog);
        scanCmd.Parameters.AddWithValue("fmt", parsed.FormattedOutput);
        scanCmd.Parameters.AddWithValue("cnt", parsed.Moons.Count);
        scanCmd.Parameters.AddWithValue("rar", parsed.MaxRarity);
        var scanId = (long)(await scanCmd.ExecuteScalarAsync(ct))!;

        foreach (var moon in parsed.Moons)
        {
            await using var moonCmd = new NpgsqlCommand(
                "SELECT moony.fn_moon_insert(@sid, @name, @sys, @pnum, @mnum, @rar, @sysid, @pid, @mid)", conn, tx);
            moonCmd.Parameters.AddWithValue("sid", scanId);
            moonCmd.Parameters.AddWithValue("name", moon.FullName);
            moonCmd.Parameters.AddWithValue("sys", moon.SolarSystem);
            moonCmd.Parameters.AddWithValue("pnum", moon.PlanetNumber);
            moonCmd.Parameters.AddWithValue("mnum", moon.MoonNumber);
            moonCmd.Parameters.AddWithValue("rar", moon.Rarity);
            moonCmd.Parameters.AddWithValue("sysid", moon.SolarSystemId == 0 ? DBNull.Value : moon.SolarSystemId);
            moonCmd.Parameters.AddWithValue("pid", moon.PlanetId == 0 ? DBNull.Value : moon.PlanetId);
            moonCmd.Parameters.AddWithValue("mid", moon.EveMoonId == 0 ? DBNull.Value : moon.EveMoonId);
            var moonId = (long)(await moonCmd.ExecuteScalarAsync(ct))!;

            foreach (var ore in moon.Ores)
            {
                await using var oreCmd = new NpgsqlCommand(
                    "SELECT moony.fn_ore_insert(@mid, @type, @tid, @pct, @rar, @srt)", conn, tx);
                oreCmd.Parameters.AddWithValue("mid", moonId);
                oreCmd.Parameters.AddWithValue("type", ore.OreType);
                oreCmd.Parameters.AddWithValue("tid", ore.OreTypeId);
                oreCmd.Parameters.AddWithValue("pct", ore.Percentage);
                oreCmd.Parameters.AddWithValue("rar", ore.Rarity);
                oreCmd.Parameters.AddWithValue("srt", ore.SortOrder);
                await oreCmd.ExecuteScalarAsync(ct);
            }
        }

        await tx.CommitAsync(ct);
        return scanId;
    }

    public async Task<List<ScanSummary>> ListScansAsync(int limit, int offset, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT * FROM moony.fn_scan_list(@lim, @off)", conn);
        cmd.Parameters.AddWithValue("lim", limit);
        cmd.Parameters.AddWithValue("off", offset);

        var results = new List<ScanSummary>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            results.Add(new ScanSummary
            {
                ScanId = reader.GetInt64(0),
                MoonCount = reader.GetInt32(1),
                MaxRarity = reader.GetInt32(2),
                SubmittedAt = reader.GetDateTime(3)
            });
        }
        return results;
    }

    public async Task<ScanDetail?> GetScanAsync(long scanId, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        // Get scan header
        await using var scanCmd = new NpgsqlCommand("SELECT * FROM moony.fn_scan_get(@id)", conn);
        scanCmd.Parameters.AddWithValue("id", scanId);
        await using var scanReader = await scanCmd.ExecuteReaderAsync(ct);
        if (!await scanReader.ReadAsync(ct)) return null;

        var detail = new ScanDetail
        {
            ScanId = scanReader.GetInt64(0),
            RawLog = scanReader.GetString(1),
            FormattedOutput = scanReader.GetString(2),
            MoonCount = scanReader.GetInt32(3),
            MaxRarity = scanReader.GetInt32(4),
            SubmittedAt = scanReader.GetDateTime(5)
        };
        await scanReader.CloseAsync();

        // Get moons
        await using var moonCmd = new NpgsqlCommand("SELECT * FROM moony.fn_moons_by_scan(@id)", conn);
        moonCmd.Parameters.AddWithValue("id", scanId);
        await using var moonReader = await moonCmd.ExecuteReaderAsync(ct);
        while (await moonReader.ReadAsync(ct))
        {
            var moon = new MoonDetail
            {
                MoonId = moonReader.GetInt64(0),
                FullName = moonReader.GetString(1),
                SolarSystem = moonReader.GetString(2),
                PlanetNumber = moonReader.GetInt32(3),
                MoonNumber = moonReader.GetInt32(4),
                Rarity = moonReader.GetInt32(5)
            };
            detail.Moons.Add(moon);
        }
        await moonReader.CloseAsync();

        // Get ores for each moon
        foreach (var moon in detail.Moons)
        {
            await using var oreCmd = new NpgsqlCommand("SELECT * FROM moony.fn_ores_by_moon(@mid)", conn);
            oreCmd.Parameters.AddWithValue("mid", moon.MoonId);
            await using var oreReader = await oreCmd.ExecuteReaderAsync(ct);
            while (await oreReader.ReadAsync(ct))
            {
                moon.Ores.Add(new OreDetail
                {
                    OreId = oreReader.GetInt64(0),
                    OreType = oreReader.GetString(1),
                    OreTypeId = oreReader.GetInt32(2),
                    Percentage = oreReader.GetInt32(3),
                    Rarity = oreReader.GetInt32(4),
                    SortOrder = oreReader.GetInt32(5)
                });
            }
        }

        return detail;
    }
}

public sealed class ScanSummary
{
    public long ScanId { get; set; }
    public int MoonCount { get; set; }
    public int MaxRarity { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public sealed class ScanDetail
{
    public long ScanId { get; set; }
    public string RawLog { get; set; } = "";
    public string FormattedOutput { get; set; } = "";
    public int MoonCount { get; set; }
    public int MaxRarity { get; set; }
    public DateTime SubmittedAt { get; set; }
    public List<MoonDetail> Moons { get; set; } = new();
}

public sealed class MoonDetail
{
    public long MoonId { get; set; }
    public string FullName { get; set; } = "";
    public string SolarSystem { get; set; } = "";
    public int PlanetNumber { get; set; }
    public int MoonNumber { get; set; }
    public int Rarity { get; set; }
    public List<OreDetail> Ores { get; set; } = new();
}

public sealed class OreDetail
{
    public long OreId { get; set; }
    public string OreType { get; set; } = "";
    public int OreTypeId { get; set; }
    public int Percentage { get; set; }
    public int Rarity { get; set; }
    public int SortOrder { get; set; }
}
