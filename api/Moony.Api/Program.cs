using System.Text.Json;
using System.Text.Json.Serialization;
using Moony.Api.Models;
using Moony.Api.Services;

var builder = WebApplication.CreateSlimBuilder(args);

// JSON source generator for AOT
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, MoonyJsonContext.Default);
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

var connStr = Environment.GetEnvironmentVariable("MOONY_DB") ?? "Host=localhost;Database=moony;Username=moony;Password=moony;Search Path=moony";
builder.Services.AddSingleton(new ScanRepository(connStr));

var app = builder.Build();

// Health check
app.MapGet("/api/health", () => Results.Ok(new HealthResponse { Status = "ok" }));

// POST /api/scans — parse and save a scan
app.MapPost("/api/scans", async (HttpRequest request, ScanRepository repo, CancellationToken ct) =>
{
    using var reader = new StreamReader(request.Body);
    var rawLog = await reader.ReadToEndAsync(ct);

    if (string.IsNullOrWhiteSpace(rawLog))
        return Results.BadRequest(new ErrorResponse { Error = "Empty scan data" });

    var parsed = MoonParser.Parse(rawLog);
    if (parsed.Moons.Count == 0)
        return Results.BadRequest(new ErrorResponse { Error = "No moons found in scan data" });

    var scanId = await repo.SaveScanAsync(rawLog, parsed, ct);

    return Results.Created($"/api/scans/{scanId}", new ScanCreateResponse
    {
        ScanId = scanId,
        MoonCount = parsed.Moons.Count,
        MaxRarity = parsed.MaxRarity,
        FormattedOutput = parsed.FormattedOutput
    });
});

// GET /api/scans — list scans
app.MapGet("/api/scans", async (ScanRepository repo, int? limit, int? offset, CancellationToken ct) =>
{
    var scans = await repo.ListScansAsync(limit ?? 20, offset ?? 0, ct);
    return Results.Ok(scans);
});

// GET /api/scans/{id} — get scan detail
app.MapGet("/api/scans/{id:long}", async (long id, ScanRepository repo, CancellationToken ct) =>
{
    var scan = await repo.GetScanAsync(id, ct);
    return scan is null ? Results.NotFound() : Results.Ok(scan);
});

app.Run();

public sealed class HealthResponse { public string Status { get; set; } = ""; }
public sealed class ErrorResponse { public string Error { get; set; } = ""; }
public sealed class ScanCreateResponse
{
    public long ScanId { get; set; }
    public int MoonCount { get; set; }
    public int MaxRarity { get; set; }
    public string FormattedOutput { get; set; } = "";
}

[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(ScanCreateResponse))]
[JsonSerializable(typeof(List<ScanSummary>))]
[JsonSerializable(typeof(ScanDetail))]
[JsonSerializable(typeof(MoonDetail))]
[JsonSerializable(typeof(OreDetail))]
internal partial class MoonyJsonContext : JsonSerializerContext { }
