using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

public class PluginManager
{
    private static PluginManager myInstance;

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

        string[] dllList = Directory.GetFiles(pluginsDirectory, "*.plugin.dll");

        // Instance with list of nulls
        List<PluginInfo> plugins = new List<PluginInfo>();

        foreach (var dllPath in dllList)
        {
            try
            {
                plugins.Add(new PluginInfo(dllPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load {Path.GetFileName(dllPath)}: {ex.Message}");
            }
        }

        return plugins;
    }

    internal RouteResponse handleRoute(RouteRequest routeRequest)
    {
        RouteResponse preResponse = new RouteResponse() { Id = routeRequest.Id };

        if (routeRequest == null) return preResponse.Error("Object RouteRequest is required");
        if (routeRequest.Id == null) return preResponse.Error("String parameter id is required");
        if (routeRequest.PlugIn == null) return preResponse.Error("string parameter plugin is required");
        if (routeRequest.Method == null) return preResponse.Error("string parameter method is required");

        // Convert to object
        if (routeRequest.Data.GetType().FullName == typeof(JObject).FullName) routeRequest.Data = ((JObject)routeRequest.Data).ToObject(typeof(object));

        PluginInfo? foundPlugin = null;
        try
        {
            foundPlugin = this.myPlugIns.Where(x => x.PluginName == routeRequest.PlugIn || x.PluginName == $"{routeRequest.PlugIn}.plugin").First();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"[PluginManager] Plugin {routeRequest.PlugIn} not found...");
            return preResponse.Error($"Plugin {routeRequest.PlugIn} not found...");
        }
        MethodInfo? foundMethod;
        try
        {
            foundMethod = foundPlugin.Methods
                .Where(m => m.Name.Equals(routeRequest.Method, StringComparison.OrdinalIgnoreCase))
                .First();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"[{routeRequest.PlugIn}] Method {routeRequest.Method} not found...");
            return preResponse.Error($"Method {routeRequest.Method} not found...");
        }

        try
        {
            return (RouteResponse?)foundMethod.Invoke(null, [routeRequest, preResponse]); ;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{routeRequest.PlugIn}][{routeRequest.Method}] error: {ex.ToString()}");
            return preResponse.Error($"{ex.Message}");
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
        if (myInstance == null) myInstance = new PluginManager();

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
            RouteResponse response = myInstance.handleRoute(request);
            return JsonConvert.SerializeObject(response, responseSettings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PluginManager] Failed to process request. {ex.Message}");
            return JsonConvert.SerializeObject(new RouteResponse() { ErrorMessage = $"Failed to process request. {ex.Message}" }, responseSettings);
        }
    }
}
