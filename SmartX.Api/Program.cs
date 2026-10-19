using Smart.Shared;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register the Real-Time Engine as a Singleton
builder.Services.AddSingleton<RealTimeEngine>();

// Allow large file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB
});

// CORS so the Blazor client can call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowClient");

// Ensure uploads folder exists
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

// ============================================================
// ENDPOINTS
// ============================================================

// -- PART 1: SENSOR MANAGEMENT --
app.MapPost("/api/sensors/register", (SensorRegistration reg, RealTimeEngine engine) =>
{
    if (string.IsNullOrWhiteSpace(reg.DeviceMacAddress))
        return Results.BadRequest("MAC is required.");

    engine.LiveDevices[reg.DeviceMacAddress] = reg;
    return Results.Ok(new { message = "Sensor registered", total = engine.LiveDevices.Count });
});

app.MapGet("/api/sensors", (RealTimeEngine engine) => Results.Ok(engine.LiveDevices.Values));

app.MapPut("/api/sensors/{mac}", (string mac, SensorRegistration updated, RealTimeEngine engine) =>
{
    if (!engine.LiveDevices.ContainsKey(mac)) return Results.NotFound();
    updated.DeviceMacAddress = mac;
    engine.LiveDevices[mac] = updated;
    return Results.Ok(new { message = "Sensor updated" });
});

app.MapDelete("/api/sensors/{mac}", (string mac, RealTimeEngine engine) =>
{
    if (!engine.LiveDevices.ContainsKey(mac)) return Results.NotFound();
    engine.LiveDevices.Remove(mac);
    return Results.Ok(new { message = "Sensor deleted" });
});

// -- PART 2: TELEMETRY ENGINE (Queues & Priority Queues) --
app.MapPost("/api/telemetry/ingest", (TelemetryPacket<float> packet, RealTimeEngine engine) =>
{
    engine.TelemetryQueue.Enqueue(packet);
    engine.HistoricalLogs[DateTime.UtcNow.AddTicks(packet.Timestamp.Ticks)] = packet;

    if (packet.SensorValue > 100)
    {
        engine.CriticalAlertsQueue.Enqueue(packet, 0); // Priority 0
        return Results.Ok(new { message = "Critical anomaly detected! Bypassed queue.", priority = true });
    }

    return Results.Ok(new { message = "Telemetry queued normally.", priority = false });
});

// ✅ FIXED: Changed `int take` to `int? take` so it is truly optional
app.MapGet("/api/telemetry/recent", (int? take, RealTimeEngine engine) =>
{
    int count = take ?? 50; // Default to 50 if not provided

    var recent = engine.HistoricalLogs.Values
        .OrderByDescending(t => t.Timestamp)
        .Take(count)
        .ToList();
    return Results.Ok(recent);
});

// -- PART 2: ENGINE STATUS --
app.MapGet("/api/engine/queues", (RealTimeEngine engine) =>
{
    return Results.Ok(new
    {
        standardQueueCount = engine.TelemetryQueue.Count,
        criticalQueueCount = engine.CriticalAlertsQueue.Count,
        disconnectedCount = engine.DisconnectedNodes.Count
    });
});

// -- PART 2: COMMANDS & UNDO (Stack) --
app.MapPost("/api/commands/execute", (string command, RealTimeEngine engine) =>
{
    engine.CommandHistory.Push(command);
    return Results.Ok(new { message = $"Command '{command}' executed." });
});

app.MapPost("/api/commands/undo", (RealTimeEngine engine) =>
{
    if (engine.CommandHistory.TryPop(out var lastCommand))
    {
        return Results.Ok(new { message = $"Undid command: '{lastCommand}'" });
    }
    return Results.BadRequest("No commands to undo.");
});

// -- PART 2: DISCONNECTED NODES (HashSet) --
app.MapPost("/api/sensors/{mac}/disconnect", (string mac, RealTimeEngine engine) =>
{
    bool added = engine.DisconnectedNodes.Add(mac);
    if (!added) return Results.Ok(new { message = "Node already disconnected. Duplicate alert ignored." });

    return Results.Ok(new { message = "Node disconnected. Alert sent." });
});

app.MapPost("/api/sensors/{mac}/reconnect", (string mac, RealTimeEngine engine) =>
{
    engine.DisconnectedNodes.Remove(mac);
    return Results.Ok(new { message = "Node reconnected." });
});

// -- PART 2: PREDICTIVE ENGINE --
app.MapPost("/api/predictive/search", (string searchTerm, RealTimeEngine engine) =>
{
    engine.LogSearch(searchTerm);
    return Results.Ok(new { logged = true });
});

app.MapGet("/api/predictive/recommendations", (RealTimeEngine engine) =>
{
    var recommendation = engine.GetSmartRecommendation();
    return Results.Ok(new { recommendation = recommendation });
});

// -- PART 1: FILE UPLOAD --
app.MapPost("/api/sensors/{mac}/attachments", async (string mac, HttpRequest request) =>
{
    if (!request.HasFormContentType) return Results.BadRequest("Expected multipart/form-data.");
    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    if (file is null || file.Length == 0) return Results.BadRequest("No file.");

    var safeMac = string.Concat(mac.Where(char.IsLetterOrDigit));
    var savedName = $"{safeMac}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
    var fullPath = Path.Combine(uploadsPath, savedName);

    using (var stream = File.Create(fullPath)) { await file.CopyToAsync(stream); }

    return Results.Ok(new { message = "File uploaded", savedAs = savedName });
});

// -- PART 1: DEMO ENDPOINT --
app.MapGet("/api/demo/advanced", () =>
{
    var m1 = new SmartMeter("METER-A", 120.5);
    var m2 = new SmartMeter("METER-B", 80.25);
    var aggregate = m1 + m2;

    var root = new DeploymentNode("Facility");
    var zone1 = new DeploymentNode("Zone 1");
    zone1.Children.Add(new DeploymentNode("Sub-Zone B"));
    root.Children.Add(zone1);

    var isValid = DeploymentNode.ValidateDeploymentPath(root, "Sub-Zone B");

    return Results.Ok(new
    {
        aggregateMeter = aggregate.DeviceMac,
        aggregateWatts = aggregate.Wattage,
        nodeValid = isValid
    });
});

app.Run();