using System.Reflection;
using TauriDotNetBridge.Contracts;

namespace TauriDotNetBridge;

public class PluginInfo
{
    private readonly List<MethodInfo> myActions = new();

    public PluginInfo(string dllPath)
    {
        var assembly = AppDomain.CurrentDomain.Load(LoadFile(dllPath));
        var plugInName = assembly.GetName().Name;

        AppDomain.CurrentDomain.AssemblyResolve += (object? sender, ResolveEventArgs args) =>
            AssemblyDependency.AssemblyResolve(sender, args, plugInName);

        Console.WriteLine($"Loaded: {Path.GetFileNameWithoutExtension(dllPath)}, Name: {plugInName}");

        foreach (var type in assembly.GetTypes())
        {
            var compatibleMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m =>
                {
                    if (m.GetCustomAttribute<RouteMethodAttribute>() == null) return false;

                    // Verify parameters
                    var ps = m.GetParameters();
                    if (ps.Length != 1) return false;

                    // verify return
                    if (m.ReturnType.FullName != typeof(RouteResponse).FullName) return false;

                    return true;
                })
                .ToArray();

            foreach (var method in compatibleMethods)
            {
                Console.WriteLine($"RouteMethod found: {method.GetType().Name}: {method.Name}");
            }

            myActions.AddRange(compatibleMethods);
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

    public MethodInfo? TryGetAction(string controller, string action) =>
        myActions.FirstOrDefault(m =>
            (m.DeclaringType.Name.Equals(controller, StringComparison.OrdinalIgnoreCase)
                || m.DeclaringType.Name.Equals(controller + "Controller", StringComparison.OrdinalIgnoreCase))
            && m.Name.Equals(action, StringComparison.OrdinalIgnoreCase));
}
