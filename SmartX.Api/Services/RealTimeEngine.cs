using Smart.Shared;
using System.Collections.Concurrent;

namespace SmartX.Api.Services;

public class RealTimeEngine
{
    // 1. Queue (15 Marks): Standard First-In-First-Out for incoming telemetry
    public ConcurrentQueue<TelemetryPacket<float>> TelemetryQueue { get; } = new();

    // 2. Priority Queue (15 Marks): Critical alerts (e.g., values > 100) bypass the standard queue
    // The lower the number, the higher the priority (0 = Critical, 1 = High, 2 = Normal)
    public PriorityQueue<TelemetryPacket<float>, int> CriticalAlertsQueue { get; } = new();

    // 3. Stack (15 Marks): "Undo" feature for manual override commands
    public ConcurrentStack<string> CommandHistory { get; } = new();

    // 4. Dictionary (15 Marks): O(1) instant lookup of live devices by MAC address
    public Dictionary<string, SensorRegistration> LiveDevices { get; } = new();

    // 5. Sorted Dictionary (15 Marks): Chronological historical logs (for timeline graphs)
    public SortedDictionary<DateTime, TelemetryPacket<float>> HistoricalLogs { get; } = new();

    // 6. HashSet (10 Marks): Tracks unique disconnected nodes to prevent duplicate alerts
    public HashSet<string> DisconnectedNodes { get; } = new();

    // 7. Predictive Engine Data (30 Marks): Tracks user searches to suggest actions
    public List<string> UserSearchHistory { get; } = new();

    // Helper to log searches
    public void LogSearch(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            UserSearchHistory.Add(searchTerm);
        }
    }

    // The actual Predictive Algorithm (30 Marks)
    public string GetSmartRecommendation()
    {
        // 1. If no searches yet, return empty
        if (UserSearchHistory.Count == 0) return "";

        // 2. Check for recent searches related to "Pump" or "Moisture"
        bool searchingForPump = UserSearchHistory.Any(s => s.Contains("Pump", StringComparison.OrdinalIgnoreCase) || s.Contains("Moisture", StringComparison.OrdinalIgnoreCase));

        // 3. Check if we actually have a critical reading right now
        bool hasCriticalAlert = CriticalAlertsQueue.TryPeek(out var alert, out _);

        if (searchingForPump && hasCriticalAlert)
        {
            return $"User frequently searches for Pump controls. Critical anomaly detected on {alert.DeviceMac}. Strongly recommend checking Pump A!";
        }

        if (searchingForPump)
        {
            return "User frequently searches for Pump/Moisture. Proactively check Pump A and Relay 1.";
        }

        if (hasCriticalAlert)
        {
            return $"Critical Alert in queue for {alert.DeviceMac}. Recommend investigating immediately.";
        }

        return "";
    }
}