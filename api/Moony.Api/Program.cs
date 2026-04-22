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
var adminPassword = Environment.GetEnvironmentVariable("MOONY_ADMIN_PASSWORD") ?? "";
builder.Services.AddSingleton(new ScanRepository(connStr));

var app = builder.Build();

// Health check
app.MapGet("/api/health", () => Results.Ok(new HealthResponse { Status = "ok" }));

// POST /api/scans — parse and save a scan (public)
app.MapPost("/api/scans", async (HttpRequest request, ScanRepository repo, CancellationToken ct) =>
{
    using var reader = new StreamReader(request.Body);
    var rawLog = await reader.ReadToEndAsync(ct);

    if (string.IsNullOrWhiteSpace(rawLog))
        return Results.BadRequest(new ErrorResponse { Error = "Empty scan data" });

    var parsed = MoonParser.Parse(rawLog);
    if (parsed.Moons.Count == 0)
        return Results.BadRequest(new ErrorResponse { Error = "No moons found in scan data" });

    // Determine scan name from the first system encountered (before sorting)
    var system = parsed.FirstSystem;
    var count = await repo.CountBySystemAsync(system, ct);
    var name = $"{system} #{count + 1}";

    var scanId = await repo.SaveScanAsync(rawLog, parsed, name, ct);

    return Results.Created($"/api/scans/{scanId}", new ScanCreateResponse
    {
        ScanId = scanId,
        Name = name,
        MoonCount = parsed.Moons.Count,
        MaxRarity = parsed.MaxRarity,
        FormattedOutput = parsed.FormattedOutput
    });
});

// POST /api/auth — validate admin password
app.MapPost("/api/auth", (AuthRequest auth) =>
{
    if (string.IsNullOrEmpty(adminPassword) || auth.Password != adminPassword)
        return Results.Json(new AuthResponse { Valid = false }, MoonyJsonContext.Default.AuthResponse, statusCode: 401);
    return Results.Ok(new AuthResponse { Valid = true });
});

// GET /api/scans — list scans (admin only)
app.MapGet("/api/scans", async (HttpRequest request, ScanRepository repo, int? limit, int? offset, CancellationToken ct) =>
{
    if (request.Headers["X-Admin-Password"].FirstOrDefault() != adminPassword || string.IsNullOrEmpty(adminPassword))
        return Results.Unauthorized();

    var scans = await repo.ListScansAsync(limit ?? 20, offset ?? 0, ct);
    return Results.Ok(scans);
});

// GET /api/scans/{id} — get scan detail (admin only)
app.MapGet("/api/scans/{id:long}", async (long id, HttpRequest request, ScanRepository repo, CancellationToken ct) =>
{
    if (request.Headers["X-Admin-Password"].FirstOrDefault() != adminPassword || string.IsNullOrEmpty(adminPassword))
        return Results.Unauthorized();

    var scan = await repo.GetScanAsync(id, ct);
    return scan is null ? Results.NotFound() : Results.Ok(scan);
});

app.Run();

public sealed class HealthResponse { public string Status { get; set; } = ""; }
public sealed class ErrorResponse { public string Error { get; set; } = ""; }
public sealed class ScanCreateResponse
{
    public long ScanId { get; set; }
    public string Name { get; set; } = "";
    public int MoonCount { get; set; }
    public int MaxRarity { get; set; }
    public string FormattedOutput { get; set; } = "";
}
public sealed class AuthRequest { public string Password { get; set; } = ""; }
public sealed class AuthResponse { public bool Valid { get; set; } }

[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(ScanCreateResponse))]
[JsonSerializable(typeof(List<ScanSummary>))]
[JsonSerializable(typeof(ScanDetail))]
[JsonSerializable(typeof(MoonDetail))]
[JsonSerializable(typeof(OreDetail))]
[JsonSerializable(typeof(AuthRequest))]
[JsonSerializable(typeof(AuthResponse))]
internal partial class MoonyJsonContext : JsonSerializerContext { }
