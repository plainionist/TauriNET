using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

public class Router
{
    private static Lazy<Router> myInstance = new Lazy<Router>(() => new Router());

    private static readonly JsonSerializerSettings myResponseSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    private static readonly JsonSerializerSettings myRequestSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new DefaultNamingStrategy()
        }
    };

    private readonly ActionInvoker myActionInvoker;

    private Router()
    {
        var services = new ServiceCollection();

        var loader = new PluginLoader();
        loader.Load(services);

        myActionInvoker = new ActionInvoker(services, myRequestSettings);
    }

    public static string RouteRequest(string? requestText)
    {
        if (requestText is null or "")
        {
            return SerializeResponse(new RouteResponse() { ErrorMessage = "Input is empty..." });
        }

        RouteRequest? request;
        try
        {
            request = JsonConvert.DeserializeObject<RouteRequest>(requestText, myRequestSettings);
            if (request == null)
            {
                return SerializeResponse(new RouteResponse() { ErrorMessage = "Failed to parse request JSON" });
            }
        }
        catch (Exception)
        {
            return SerializeResponse(new RouteResponse() { ErrorMessage = "Failed to parse request JSON" });
        }

        try
        {
            var response = myInstance.Value.RouteRequest(request);
            return SerializeResponse(response);
        }
        catch (Exception ex)
        {
            return SerializeResponse(new RouteResponse { ErrorMessage = $"Failed to process request: {ex}" });
        }
    }

    private static string SerializeResponse(RouteResponse? obj) =>
        obj == null ? string.Empty : JsonConvert.SerializeObject(obj, myResponseSettings);

    private RouteResponse? RouteRequest(RouteRequest routeRequest)
    {
        if (routeRequest == null) return RouteResponse.Error("Object RouteRequest is required");

        routeRequest.Controller ??= "Home";
        routeRequest.Action ??= "Index";

        try
        {
            return myActionInvoker.InvokeAction(routeRequest.Controller, routeRequest.Action, routeRequest.Data);
        }
        catch (Exception ex)
        {
            return RouteResponse.Error($"[{routeRequest.Controller}][{routeRequest.Action}] error: {ex}");
        }
    }
}
