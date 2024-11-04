using TauriDotNetBridge.Contracts;

namespace TauriApp.PlugIn;

public class LogInInfo
{
    public string? User { get; set; }
    public string? Password { get; set; }
}

public class HomeController
{
    private int myCounter = 0;

    [RouteMethod]
    public RouteResponse Login(LogInInfo loginInfo)
    {
        var currentPath = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(currentPath, "Test.txt");

        if (File.Exists(filePath))
        {
            var txtLogin = File.ReadAllText(filePath);
            if (txtLogin.Equals(loginInfo.User, StringComparison.OrdinalIgnoreCase))
            {
                return RouteResponse.Ok($"Welcome back, {loginInfo.User} - Login count: {++myCounter}");
            }
        }

        File.WriteAllText(filePath, loginInfo.User);

        return RouteResponse.Ok($"Logged in {loginInfo.User} - Login count: {++myCounter}");
    }
}
