using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using WTWTechTest.Configuration;
using WTWTechTest.Utilities;
using static WTWTechTest.Utilities.FileUtilities;
using static WTWTechTest.Utilities.SqlUtilities;

namespace WTWTechTest.Context;

public class DatabaseContext : IDisposable
{
    public const string TableASterling = "TableA_Sterling";
    public const string TableBEuro = "TableB_Euro";
    public const string TableCEuroWithErrors = "TableC_EuroWithErrors";

    private string _dbFileName = "test.db";
    private bool _isDisposed;

    public SqliteConnection? Connection { get; private set; }
    public string ConnectionString { get; private set; } = string.Empty;
    public IConfiguration Configuration { get; private set; }

    public DatabaseContext()
    {
        Configuration = TestConfigurationFactory.Create();
    }

    public void InitializeDatabase()
    {
        DeleteDirectoryIfExists(GetTempFolderPath());

        _dbFileName = CreateUniqueDatabaseFileName(Configuration.GetRequiredDefaultConnectionString());

        ConnectionString = SqlUtilities.InitializeDatabase(_dbFileName);

        var tableASqlCommands = BuildCurrencyTableCommands(TableASterling, new[]
        {
            new CurrencyTableRow("Product 1", 10, 12, 14, 45),
            new CurrencyTableRow("Product 2", 20, 15, 24, null),
            new CurrencyTableRow("Product 3", 22, 60, null, null),
            new CurrencyTableRow("Product 4", 28, null, null, null),
            new CurrencyTableRow("Total", 80, 87, 38, 45)
        });

        var tableBSqlCommands = BuildCurrencyTableCommands(TableBEuro, new[]
        {
            new CurrencyTableRow("Product 1", 15, 18, 21, 67.5),
            new CurrencyTableRow("Product 2", 30, 22.5, 36, null),
            new CurrencyTableRow("Product 3", 33, 90, null, null),
            new CurrencyTableRow("Product 4", 42, null, null, null),
            new CurrencyTableRow("Total", 120, 130.5, 57, 67.5)
        });

        var tableCSqlCommands = BuildCurrencyTableCommands(TableCEuroWithErrors, new[]
        {
            new CurrencyTableRow("Product 1", 15, 18, 20, 67.5),
            new CurrencyTableRow("Product 2", 31, 22.5, 36, null),
            new CurrencyTableRow("Product 3", 33, 88, null, null),
            new CurrencyTableRow("Product 4", 42, null, null, null),
            new CurrencyTableRow("Total", 121, 130.5, 57, 66.5)
        });

        ExecuteSql(ConnectionString, tableASqlCommands.Concat(tableBSqlCommands).Concat(tableCSqlCommands).ToArray());

        Connection = OpenConnection(ConnectionString);
    }

    public void Cleanup()
    {
        try
        {
            Connection?.Close();
        }
        catch
        {
            // Ignore errors during close
        }

        try
        {
            Connection?.Dispose();
            Connection = null;
        }
        catch
        {
            // Ignore errors during dispose
        }

        // Force garbage collection to release SQLite connections
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Give the database file a moment to be released
        System.Threading.Thread.Sleep(200);

        try
        {
            DeleteFileIfExists(GetDatabaseFilePath(_dbFileName));
        }
        catch (IOException)
        {
            // If file is still locked, ignore it - it will be cleaned up later
            Console.WriteLine($"Warning: Could not delete database file {_dbFileName} - file may still be in use");
        }

        try
        {
            DeleteDirectoryIfExists(GetTempFolderPath());
        }
        catch (IOException)
        {
            // If directory is still locked, ignore it
            Console.WriteLine("Warning: Could not delete temp directory - directory may still be in use");
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        Cleanup();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
