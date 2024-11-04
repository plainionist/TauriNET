using Newtonsoft.Json.Linq;
using TauriDotNetBridge.Contracts;

namespace TestApp;

public class LogInInfo
{
    public string User { get; set; }
    public string Password { get; set; }
}

public class HomeController
{
    private int myCounter = 0;

    [RouteMethod]
    public RouteResponse Login(LogInInfo loginInfo)
    {
        var currentPath = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(currentPath, "Test.txt");

        if (loginInfo == null || loginInfo.User == "" || loginInfo.User == null)
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
        File.WriteAllText(filePath, $"Last login: {loginInfo.User}");

        return RouteResponse.Ok($"Logged in {loginInfo.User} - Login count: {++myCounter}");
    }
}
