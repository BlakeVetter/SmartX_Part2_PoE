namespace Smart.Shared;

public class SensorRegistration
{
    public string DeviceMacAddress { get; set; } = string.Empty;
    public string DeploymentLocation { get; set; } = string.Empty;
    public SensorCategory Category { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}