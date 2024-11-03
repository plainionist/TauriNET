namespace TauriDotNetBridge.Contracts;

public class RouteResponse
{
    public string Id { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Data { get; set; }

    public RouteResponse() { }

    public RouteResponse Ok(object? data = null)
    {
        this.Data = data;
        return this;
    }

    public RouteResponse Error(string error)
    {
        Data = error;
        return this;
    }
}
