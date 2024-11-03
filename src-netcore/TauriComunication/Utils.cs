using Newtonsoft.Json.Linq;

namespace TauriCommunication {
	public class Utils {
		public static T ParseObject<T>(object data) {
			if (data == null) throw new ArgumentNullException("parameter data is null");
			return ((JObject)data).ToObject<T>()!;
		}
	}
}
