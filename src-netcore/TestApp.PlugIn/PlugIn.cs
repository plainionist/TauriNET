using Microsoft.Extensions.DependencyInjection;
using TauriDotNetBridge.Contracts;

namespace TestApp.PlugIn;

public class PlugIn : IPlugIn
{
    public void Initialize(IServiceCollection services)
    {
        services.AddSingleton<HomeController>();
    }
}