using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

public class PluginManager
{
    private static Lazy<PluginManager> myInstance = new Lazy<PluginManager>(() => new PluginManager());

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

    private readonly IReadOnlyCollection<PluginInfo> myPlugIns;

    private PluginManager()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => AssemblyDependency.AssemblyResolve(sender, args, Directory.GetCurrentDirectory());
        myPlugIns = LoadPlugInRoutes().ToArray();
    }

    private List<PluginInfo> LoadPlugInRoutes()
    {
        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "plugins");

        if (!Directory.Exists(pluginsDirectory))
        {
            Console.WriteLine("Plugins directory does not exist");
            return [];
        }

        var assemblies = Directory.GetFiles(pluginsDirectory, "*.plugin.dll");

        var plugins = new List<PluginInfo>();
        foreach (var dllPath in assemblies)
        {
            try
            {
                plugins.Add(new PluginInfo(dllPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load {Path.GetFileName(dllPath)}: {ex}");
            }
        }

        return plugins;
    }

    public static string ProcessRequest(string? requestText)
    {
        if (requestText is null or "")
        {
            return JsonConvert.SerializeObject(new RouteResponse() { ErrorMessage = "Input is empty..." }, myResponseSettings);
        }

        RouteRequest request;
        try
        {
            request = JsonConvert.DeserializeObject<RouteRequest>(requestText, myRequestSettings);
        }
        catch (Exception)
        {
            return JsonConvert.SerializeObject(new RouteResponse() { ErrorMessage = "Failed to parse request JSON" }, myResponseSettings);
        }

        try
        {
            var response = myInstance.Value.RouteRequest(request);
            return SerializeResponse(response);
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new RouteResponse { ErrorMessage = $"Failed to process request: {ex}" }, myResponseSettings);
        }
    }

    private static string SerializeResponse(object obj) =>
        JsonConvert.SerializeObject(obj, myResponseSettings);

    private RouteResponse RouteRequest(RouteRequest routeRequest)
    {
        if (routeRequest == null) return RouteResponse.Error("Object RouteRequest is required");
        if (routeRequest.Controller == null) return RouteResponse.Error("string parameter plugin is required");
        if (routeRequest.Action == null) return RouteResponse.Error("string parameter method is required");

        // Convert to object
        if (routeRequest.Data.GetType().FullName == typeof(JObject).FullName)
        {
            routeRequest.Data = ((JObject)routeRequest.Data).ToObject(typeof(object));
        }

        var foundMethod = myPlugIns
            .Select(p => p.TryGetAction(routeRequest.Controller, routeRequest.Action))
            .FirstOrDefault(m => m != null);

        if (foundMethod == null)
        {
            return RouteResponse.Error($"No matching route found for '{routeRequest.Controller}/{routeRequest.Action}'");
        }

        try
        {
            var serializer = JsonSerializer.Create(myRequestSettings);
            var arg = ((JObject)routeRequest.Data).ToObject(foundMethod.GetParameters().Single().ParameterType, serializer);
            return (RouteResponse?)foundMethod.Invoke(null, [arg]);
        }
        catch (Exception ex)
        {
            return RouteResponse.Error($"[{routeRequest.Controller}][{routeRequest.Action}] error: {ex}");
        }
    }
}
