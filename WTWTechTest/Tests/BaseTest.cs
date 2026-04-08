using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using WTWTechTest.Configuration;
using WTWTechTest.Reporting;
using WTWTechTest.Utilities;


/*
 *  For the Purposes of the test I am using SQLite, in reality I would set up an SQL connection here in baseTest as an
 * NUnit SetUp method, which would initialise the DB connection. I was considering using docker to spin up an SQL server
 * and use an actual connection string, but I wasn't sure if docker was somehthing you guys use on a day to day basis
 * and thought I'd avoid the typical "works on my machine" scenario!
 *
 * The test spins up an SQLite DB on SetUp, and then deletes the DB on teardown, I know this is not what a real test
 * would do in the real world scenario, we would use SQL to connect to an actual DB with the connection string, and
 * read data from there.
 */

namespace WTWTechTest.Tests;

public class BaseTest
{
    protected const string TableASterling = "TableA_Sterling";
    protected const string TableBEuro = "TableB_Euro";
    protected const string TableCEuroWithErrors = "TableC_EuroWithErrors";

    private string _dbFileName = "test.db";

    protected SqliteConnection? Connection { get; private set; }
    protected string ConnectionString { get; private set; } = string.Empty;
    protected IConfiguration Configuration { get; private set; }

    /*
     * The two [SetUp] and [TearDown] hooks are around reporting only, they will run for each test method, this ensures
     * That that test cases are separated cleanly in the report. The ReportingSetup.cs file is responsible for
     * initialising reporting on the run context level.
     */
    
    [SetUp]
    public void StartTestReporting()
    {
        ExtentReportManager.StartTest(TestContext.CurrentContext);
    }

    [TearDown]
    public void CompleteTestReporting()
    {
        ExtentReportManager.CompleteTest(TestContext.CurrentContext);
    }

    [OneTimeSetUp]
    public void Setup()
    {
        Configuration = TestConfigurationFactory.Create();

        Utilities.Utilities.DeleteDirectoryIfExists(Utilities.Utilities.GetTempFolderPath());

        _dbFileName = Utilities.Utilities.CreateUniqueDatabaseFileName(Configuration.GetRequiredDefaultConnectionString());

        ConnectionString = Utilities.Utilities.InitializeDatabase(_dbFileName);

        var tableASqlCommands = Utilities.Utilities.BuildCurrencyTableCommands(TableASterling, new[]
        {
            new CurrencyTableRow("Product 1", 10, 12, 14, 45),
            new CurrencyTableRow("Product 2", 20, 15, 24, null),
            new CurrencyTableRow("Product 3", 22, 60, null, null),
            new CurrencyTableRow("Product 4", 28, null, null, null),
            new CurrencyTableRow("Total", 80, 87, 38, 45)
        });

        var tableBSqlCommands = Utilities.Utilities.BuildCurrencyTableCommands(TableBEuro, new[]
        {
            new CurrencyTableRow("Product 1", 15, 18, 21, 67.5),
            new CurrencyTableRow("Product 2", 30, 22.5, 36, null),
            new CurrencyTableRow("Product 3", 33, 90, null, null),
            new CurrencyTableRow("Product 4", 42, null, null, null),
            new CurrencyTableRow("Total", 120, 130.5, 57, 67.5)
        });

        var tableCSqlCommands = Utilities.Utilities.BuildCurrencyTableCommands(TableCEuroWithErrors, new[]
        {
            new CurrencyTableRow("Product 1", 15, 18, 20, 67.5),
            new CurrencyTableRow("Product 2", 31, 22.5, 36, null),
            new CurrencyTableRow("Product 3", 33, 88, null, null),
            new CurrencyTableRow("Product 4", 42, null, null, null),
            new CurrencyTableRow("Total", 121, 130.5, 57, 66.5)
        });

        Utilities.Utilities.ExecuteSql(ConnectionString, tableASqlCommands.Concat(tableBSqlCommands).Concat(tableCSqlCommands).ToArray());

        Connection = Utilities.Utilities.OpenConnection(ConnectionString);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Connection?.Close();
        Connection?.Dispose();

        Utilities.Utilities.DeleteFileIfExists(Utilities.Utilities.GetDatabaseFilePath(_dbFileName));
    }
}
