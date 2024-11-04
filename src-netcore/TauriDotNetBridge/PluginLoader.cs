using Microsoft.Extensions.DependencyInjection;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

internal class PluginLoader
{
    public void Load(ServiceCollection services)
    {
        var home = Path.GetDirectoryName(GetType().Assembly.Location);

        System.Reflection.Assembly? onGlobalAssemblyResolve(object? sender, ResolveEventArgs args) =>
            AssemblyDependency.AssemblyResolve(sender, args, home);

        AppDomain.CurrentDomain.AssemblyResolve += onGlobalAssemblyResolve;

        var pluginsDirectory = Path.Combine(home, "plugins");

        if (!Directory.Exists(pluginsDirectory))
        {
            Console.WriteLine("Plugins directory does not exist");
            return;
        }

        var assemblies = Directory.GetFiles(pluginsDirectory, "*.plugin.dll");

        foreach (var dllPath in assemblies)
        {
            ResolveEventHandler? onAssemblyResolve = null;

            try
            {
                var assembly = AppDomain.CurrentDomain.Load(LoadFile(dllPath));
                var plugInName = assembly.GetName().Name;

                onAssemblyResolve = (object? sender, ResolveEventArgs args) => AssemblyDependency.AssemblyResolve(sender, args, plugInName);
                AppDomain.CurrentDomain.AssemblyResolve += onAssemblyResolve;
                
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
            finally
            {
                if (onAssemblyResolve != null)
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= onAssemblyResolve;
                }
            }
        }

        AppDomain.CurrentDomain.AssemblyResolve -= onGlobalAssemblyResolve;
    }

    private static byte[] LoadFile(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Open);

        byte[] buffer = new byte[(int)fs.Length];
        fs.Read(buffer, 0, buffer.Length);
        fs.Close();

        return buffer;
    }
}