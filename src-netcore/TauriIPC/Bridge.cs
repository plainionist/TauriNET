using System.Runtime.InteropServices;
using System.Text;

namespace TauriIPC;

public static class Bridge
{
	[UnmanagedCallersOnly]
	public static unsafe byte* ProcessRequest(/* byte* */ IntPtr textPtr, int textLength)
	{
		var text = Marshal.PtrToStringUTF8(textPtr, textLength);
		if (text == null || text.Length == 0) text = null;

		var response = TauriCommunication.PluginManager.ProcessRequest(text);

		byte[] responseBytes = Encoding.UTF8.GetBytes(response);
		IntPtr unmanagedPointer = Marshal.AllocHGlobal(responseBytes.Length + 1);

		Marshal.Copy(responseBytes, 0, unmanagedPointer, responseBytes.Length);
		Marshal.WriteByte(unmanagedPointer, responseBytes.Length, 0);

		return (byte*)unmanagedPointer;
	}
}
