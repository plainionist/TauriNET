using Microsoft.Extensions.DependencyInjection;

namespace TauriDotNetBridge.Contracts;

public interface IPlugIn
{
    void Initialize(IServiceCollection services);
}
