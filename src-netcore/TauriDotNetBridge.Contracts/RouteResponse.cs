namespace TauriDotNetBridge.Contracts;

public class RouteResponse
{
    public string? ErrorMessage { get; set; }
    public object? Data { get; set; }

    public static RouteResponse Ok(object data) =>
        new() { Data = data };

    public static RouteResponse Error(string error) =>
        new() { ErrorMessage = error };
}
