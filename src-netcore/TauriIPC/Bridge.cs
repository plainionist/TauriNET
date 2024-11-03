using System.Runtime.InteropServices;

namespace TauriIPC;

public static class Bridge
{
	private static unsafe delegate*<char*, int, byte*> CopyToCString;

	[UnmanagedCallersOnly]
	public static unsafe void SetCopyToCStringFunctionPtr(delegate*<char*, int, byte*> copyToCString) => CopyToCString = copyToCString;

	[UnmanagedCallersOnly]
	public static unsafe byte* process_request(/* byte* */IntPtr textPtr, int textLength)
	{
		var text = Marshal.PtrToStringUTF8(textPtr, textLength);
		if (text == null || text.Length == 0) text = null;

		var response = TauriCommunication.PluginManager.processRequest(text);

		fixed (char* ptr = response)
		{
			return CopyToCString(ptr, response.Length);
		}
	}
}
