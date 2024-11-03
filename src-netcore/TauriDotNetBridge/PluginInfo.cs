using System.Reflection;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

public class PluginInfo
{
    public List<MethodInfo> Methods { get; private set; } = new List<MethodInfo>();
    public string PluginName { get; private set; }

    public PluginInfo(string dllPath)
    {
        var assembly = AppDomain.CurrentDomain.Load(LoadFile(dllPath));
        PluginName = assembly.GetName().Name;

        AppDomain.CurrentDomain.AssemblyResolve += (object? sender, ResolveEventArgs args) => AssemblyDependency.AssemblyResolve(sender, args, this.PluginName);

        Console.WriteLine($"Loaded: {Path.GetFileNameWithoutExtension(dllPath)}, Name: {PluginName}");

        foreach (var type in assembly.GetTypes())
        {
            var compatibleMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m =>
                {
                    if (m.GetCustomAttribute<RouteMethodAttribute>() == null) return false;

                    // Verify parameters
                    var ps = m.GetParameters();
                    if (ps.Length != 1) return false;
                    if (ps[0].ParameterType.FullName != typeof(RouteRequest).FullName) return false;

                    // verify return
                    if (m.ReturnType.FullName != typeof(RouteResponse).FullName) return false;

                    return true;
                })
                .ToArray();

            foreach (var method in compatibleMethods)
            {
                Console.WriteLine($"RouteMethod found: {method.GetType().Name}: {method.Name}");
            }

            Methods.AddRange(compatibleMethods);
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
}
