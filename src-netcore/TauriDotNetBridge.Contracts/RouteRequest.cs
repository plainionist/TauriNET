namespace TauriDotNetBridge.Contracts;

public class RouteRequest
{
    public string Id { get; set; }
    public string PlugIn { get; set; }
    public string Method { get; set; }
    public object? Data { get; set; }
}
