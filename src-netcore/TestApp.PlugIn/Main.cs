using Newtonsoft.Json.Linq;
using TauriDotNetBridge.Contracts;

namespace TestApp;

class User
{
    public string user { get; set; }
    public string pass { get; set; }
}

public static class Main
{
    [RouteMethod]
    public static RouteResponse Login(RouteRequest request)
    {
        User? loginInfo = null;

        if (request.Data != null)
        {
            try
            {
                loginInfo = ((JObject)request.Data).ToObject<User>()!;
            }
            catch (Exception ex)
            {
                return RouteResponse.Error($"Failed to parse JSON User object: {ex.Message}");
            }
        }

        var currentPath = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(currentPath, "Test.txt");

        if (loginInfo == null || loginInfo.user == "" || loginInfo.user == null)
        {
            if (File.Exists(filePath))
            {
                var txtLogin = File.ReadAllText(filePath);
                var loginName = txtLogin.Substring("Last login: ".Length);

                return RouteResponse.Ok($"Welcome back, {loginName}");
            }

            return RouteResponse.Ok("Woops... User is not saved");
        }

        if (!File.Exists(filePath)) File.Create(filePath).Close();
        File.WriteAllText(filePath, $"Last login: {loginInfo.user}");

        return RouteResponse.Ok($"Logged in {loginInfo.user}");
    }
}
