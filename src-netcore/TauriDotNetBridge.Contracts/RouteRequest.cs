namespace TauriDotNetBridge.Contracts;

public class RouteRequest
{
    public string Controller { get; set; }
    public string Action { get; set; }
    public object? Data { get; set; }
}
