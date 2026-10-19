namespace Smart.Shared;

public class SmartMeter
{
    public string DeviceMac { get; set; } = string.Empty;
    public double Wattage { get; set; }

    public SmartMeter() { }
    public SmartMeter(string mac, double wattage) { DeviceMac = mac; Wattage = wattage; }

    // Operator Overloading: Allows Meter3 = Meter1 + Meter2
    public static SmartMeter operator +(SmartMeter a, SmartMeter b)
        => new SmartMeter($"{a.DeviceMac}+{b.DeviceMac}", a.Wattage + b.Wattage);

    public static SmartMeter operator -(SmartMeter a, SmartMeter b)
        => new SmartMeter($"{a.DeviceMac}-{b.DeviceMac}", a.Wattage - b.Wattage);
}