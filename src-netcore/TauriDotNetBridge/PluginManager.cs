using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

public class PluginManager
{
    private static Lazy<PluginManager> myInstance = new Lazy<PluginManager>(() => new PluginManager());

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

    internal RouteResponse RouteRequest(RouteRequest routeRequest)
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
            Console.WriteLine($"No matching route found for '{routeRequest.Controller}/{routeRequest.Action}'");
            return RouteResponse.Error($"No matching route found for '{routeRequest.Controller}/{routeRequest.Action}'");
        }

        try
        {
            return (RouteResponse?)foundMethod.Invoke(null, [routeRequest]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{routeRequest.Controller}][{routeRequest.Action}] error: {ex}");
            return RouteResponse.Error($"{ex.Message}");
        }
    }

    /// <summary>
    /// This method handle all requests and redirect to any RouteHandler
    /// </summary>
    /// <param name="requestText"></param>
    /// <returns></returns>
    public static string ProcessRequest(string? requestText)
    {
        var responseSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        if (requestText is null or "") return JsonConvert.SerializeObject(new RouteResponse() { ErrorMessage = "Input is empty..." }, responseSettings);

        RouteRequest request;

        var requestSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new DefaultNamingStrategy()
            }
        };

        try
        {
            request = JsonConvert.DeserializeObject<RouteRequest>(requestText, requestSettings);
        }
        catch (Exception)
        {
            return JsonConvert.SerializeObject(new RouteResponse() { ErrorMessage = "Failed to parse request JSON" }, responseSettings);
        }

        try
        {
            var response = myInstance.Value.RouteRequest(request);
            return JsonConvert.SerializeObject(response, responseSettings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PluginManager] Failed to process request. {ex.Message}");
            return JsonConvert.SerializeObject(new RouteResponse { ErrorMessage = $"Failed to process request. {ex.Message}" }, responseSettings);
        }
    }
}
