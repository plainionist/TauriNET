namespace TauriDotNetBridge.Contracts;

/// <summary>
/// This attribute specifies entry point of any route handler. This is searched by reflection.
/// <code>
/// [<see cref="RouteHandler"/>]
/// public static <see cref="RouteResponse"/> methodName(<see cref="RouteRequest"/> route)
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RouteMethodAttribute : Attribute
{
	public RouteMethodAttribute() { }

	public RouteMethodAttribute(string methodName)
	{
		MethodName = methodName;
	}

	public string? MethodName { get; }
}