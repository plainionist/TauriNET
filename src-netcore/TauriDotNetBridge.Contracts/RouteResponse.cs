namespace TauriDotNetBridge.Contracts;

public class RouteResponse
{
    public string? ErrorMessage { get; set; }
    public object? Data { get; set; }

    public RouteResponse Ok(object? data = null)
    {
        Data = data;
        return this;
    }

    public RouteResponse Error(string error)
    {
        Data = error;
        return this;
    }
}
