using Microsoft.Extensions.Configuration;

namespace WTWTechTest.Configuration;

public static class TestConfigurationExtensions
{
    private const string DefaultConnectionName = "DefaultConnection";

    public static string GetRequiredDefaultConnectionString(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(DefaultConnectionName);

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException($"Connection string '{DefaultConnectionName}' is not configured.")
            : connectionString;
    }
}
