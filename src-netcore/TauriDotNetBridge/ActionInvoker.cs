using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

public class ActionInvoker
{
    private readonly IServiceCollection myServices;
    private readonly IServiceProvider myServiceProvider;
    private readonly JsonSerializer mySerializer;

    public ActionInvoker(IServiceCollection services, JsonSerializerSettings settings)
    {
        myServices = services;

        myServiceProvider = services.BuildServiceProvider();
        mySerializer = JsonSerializer.Create(settings);
    }

    public RouteResponse? InvokeAction(string controller, string action, object? data)
    {
        var type = myServices.FirstOrDefault(x =>
            (x.ImplementationType?.Name.Equals(controller, StringComparison.OrdinalIgnoreCase) == true ||
             x.ImplementationType?.Name.Equals(controller + "Controller", StringComparison.OrdinalIgnoreCase) == true) &&
            x.ImplementationType?.IsClass == true &&
            x.ImplementationType?.IsAbstract == false)
            ?.ImplementationType;

        if (type == null)
        {
            Console.WriteLine("Controller not found.");
            return null;
        }

        var method = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(x => x.Name.Equals(action, StringComparison.OrdinalIgnoreCase)
                                 && x.GetParameters().Length == 1);

        if (method == null)
        {
            Console.WriteLine("Method not found.");
            return null;
        }

        var instance = myServiceProvider.GetService(type);
        if (instance == null)
        {
            Console.WriteLine("Failed to resolve the service instance.");
            return null;
        }

        if (data is null)
        {
            return (RouteResponse?)method.Invoke(instance, null);
        }
        else
        {
            var arg = ((JObject)data).ToObject(method.GetParameters().Single().ParameterType, mySerializer);
            return (RouteResponse?)method.Invoke(instance, [arg]);
        }
    }
}
