using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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

    private readonly ActionInvoker myActionInvoker;

    private PluginManager()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => AssemblyDependency.AssemblyResolve(sender, args, Directory.GetCurrentDirectory());

        var services = new ServiceCollection();
        LoadPlugInRoutes(services);
        myActionInvoker = new ActionInvoker(services, myRequestSettings);
    }

    private void LoadPlugInRoutes(ServiceCollection services)
    {
        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "plugins");

        if (!Directory.Exists(pluginsDirectory))
        {
            Console.WriteLine("Plugins directory does not exist");
            return;
        }

        var assemblies = Directory.GetFiles(pluginsDirectory, "*.plugin.dll");

        foreach (var dllPath in assemblies)
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.Load(LoadFile(dllPath));
                var plugInName = assembly.GetName().Name;

                AppDomain.CurrentDomain.AssemblyResolve += (object? sender, ResolveEventArgs args) =>
                    AssemblyDependency.AssemblyResolve(sender, args, plugInName);

                Console.WriteLine($"Loading '{Path.GetFileNameWithoutExtension(dllPath)}' ... ");

                foreach (var type in assembly.GetTypes().Where(x => typeof(IPlugIn).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract))
                {
                    var instance = (IPlugIn)Activator.CreateInstance(type)!;

                    Console.WriteLine($"  Initializing '{type}' ... ");

                    instance.Initialize(services);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load {Path.GetFileName(dllPath)}: {ex}");
            }

            // TODO: unregister AssemblyResolve
        }
    }

    private static byte[] LoadFile(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Open);
        byte[] buffer = new byte[(int)fs.Length];
        fs.Read(buffer, 0, buffer.Length);
        fs.Close();

        return buffer;
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
