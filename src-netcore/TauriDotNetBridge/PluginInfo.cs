using System.Reflection;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

public class PluginInfo
{
    public string DllPath { get; private set; }
    public Assembly Assembly { get; private set; }
    public Type[] Types { get; private set; }
    public List<MethodInfo> Methods { get; private set; } = new List<MethodInfo>();
    public string PluginName { get; private set; }

    public PluginInfo(string dllPath)
    {
        Assembly = AppDomain.CurrentDomain.Load(LoadFile(dllPath));
        PluginName = Assembly.GetName().Name;

        AppDomain.CurrentDomain.AssemblyResolve += (object? sender, ResolveEventArgs args) => AssemblyDependency.AssemblyResolve(sender, args, this.PluginName);

        Console.WriteLine($"Loaded dll {Path.GetFileNameWithoutExtension(dllPath)}. Name: {PluginName}");

        this.Types = this.Assembly.GetTypes();

        foreach (var type in this.Types)
        {
            var compatibleMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m =>
            {
                if (m.GetCustomAttribute<RouteMethodAttribute>() == null) return false;

                // Verify parameters
                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length != 1 && ps.Length != 2) return false;
                if (ps[0].ParameterType.FullName != typeof(RouteRequest).FullName) return false;
                if (ps[1] != null && ps[1].ParameterType.FullName != typeof(RouteResponse).FullName) return false;

                // verify return
                if (m.ReturnType.FullName != typeof(RouteResponse).FullName) return false;

                return true;
            }).ToArray();

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

    public MethodInfo? GetMethodName(string name) => Methods.FirstOrDefault(m => m.Name == name, null);
}
