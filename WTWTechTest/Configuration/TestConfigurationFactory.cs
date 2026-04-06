using Microsoft.Extensions.Configuration;

namespace WTWTechTest.Configuration;

public static class TestConfigurationFactory
{
    private const string DefaultEnvironmentName = "Development";
    private const string EnvironmentVariableName = "DOTNET_ENVIRONMENT";

    public static IConfiguration Create()
    {
        var environmentName = GetEnvironmentName();

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .Build();
    }

    private static string GetEnvironmentName()
    {
        var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableName);

        return string.IsNullOrWhiteSpace(environmentName)
            ? DefaultEnvironmentName
            : environmentName;
    }
}
