namespace Smart.Shared;

// Uses `T : struct` to avoid expensive boxing/unboxing of floats/ints/bools.
public class TelemetryPacket<T> where T : struct
{
    public string DeviceMac { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public T SensorValue { get; set; }

    public TelemetryPacket() { }
    public TelemetryPacket(string mac, T value) { DeviceMac = mac; SensorValue = value; }
}