using System.Runtime.InteropServices;
using System.Text;

namespace TauriDotNetBridge;

public static class Bridge
{
	[UnmanagedCallersOnly]
	public static unsafe byte* ProcessRequest(/* byte* */ IntPtr requestPtr, int requestLength)
	{
		var request = Marshal.PtrToStringUTF8(requestPtr, requestLength);
		if (request == null || request.Length == 0)
		{
			request = null;
		}

		var response = Router.RouteRequest(request);

		var responseBytes = Encoding.UTF8.GetBytes(response);
		var responsePtr = Marshal.AllocHGlobal(responseBytes.Length + 1);

		Marshal.Copy(responseBytes, 0, responsePtr, responseBytes.Length);
		Marshal.WriteByte(responsePtr, responseBytes.Length, 0);

		return (byte*)responsePtr;
	}
}
