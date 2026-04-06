using Microsoft.Extensions.Configuration;

namespace WTWTechTest.Configuration;

/*
 * This class is responsible for setting the correct environment. If there is no "DOTNET_ENVIRONMENT" variable set on
 * your machine, it will default to "Develompent" and the appsettings.Development.json file will be used, if there is no
 * appsetting.Development.json file, it will default to using appsettings.json, this is a required file and the test
 * will not build without it. 
 */
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
