using System.Reflection;

namespace TauriDotNetBridge;

public class AssemblyDependency
{
	public static Assembly? AssemblyResolve(object sender, ResolveEventArgs args, string pluginName)
	{
		if (args.Name.StartsWith("System")) return null;

		var name = new AssemblyName(args.Name).Name + ".dll";

		try
		{
			var compatible = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.FullName == args.Name).First();
			//Console.WriteLine($"Requested loaded ASM, returning {compatible.GetName()}");
			return compatible;
		}
		catch (Exception)
		{
		}

		string currentDirectory = Directory.GetCurrentDirectory();
		string dependencyPath = Path.Combine(currentDirectory, "plugins", pluginName, "Dependencies", name);

		return Assembly.LoadFile(dependencyPath);
	}
}
