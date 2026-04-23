using Microsoft.Data.Sqlite;
using System.Globalization;

namespace WTWTechTest.Utilities;

public sealed record CurrencyTableRow(
    string Product,
    double? Variety1,
    double? Variety2,
    double? Variety3,
    double? Variety4);

public static class SqlUtilities
{
    public const string DefaultDatabaseFileName = "test.db";

    public static string CreateUniqueDatabaseFileName(string? configuredConnectionString)
    {
        var configuredBuilder = new SqliteConnectionStringBuilder(configuredConnectionString ?? string.Empty);
        var configuredFileName = string.IsNullOrWhiteSpace(configuredBuilder.DataSource)
            ? DefaultDatabaseFileName
            : configuredBuilder.DataSource;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(configuredFileName);
        var extension = Path.GetExtension(configuredFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".db";
        }

        Console.WriteLine($"Generated unique database filename based on configured connection string: {configuredConnectionString} -> {nameWithoutExtension}_{Guid.NewGuid():N}{extension}");
        return $"{nameWithoutExtension}_{Guid.NewGuid():N}{extension}";
    }

    public static string InitializeDatabase(string dbFileName)
    {
        if (string.IsNullOrWhiteSpace(dbFileName)) throw new ArgumentException("Database filename is required.", nameof(dbFileName));

        FileUtilities.DeleteFileIfExists(FileUtilities.GetDatabaseFilePath(dbFileName));
        var connectionString = BuildConnectionString(dbFileName);
        Console.WriteLine($"Initialized database at: {FileUtilities.GetDatabaseFilePath(dbFileName)}");
        return connectionString;
    }

    public static void ExecuteSql(string connectionString, params string[] sqlCommands)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string is required.", nameof(connectionString));
        if (sqlCommands == null) throw new ArgumentNullException(nameof(sqlCommands));

        var executableCommands = sqlCommands
            .Where(sql => !string.IsNullOrWhiteSpace(sql))
            .ToArray();

        if (executableCommands.Length == 0)
        {
            throw new ArgumentException("At least one SQL command is required.", nameof(sqlCommands));
        }

        using var connection = OpenConnection(connectionString);

        foreach (var sql in executableCommands)
        {
            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }
    }

    public static SqliteConnection OpenConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string is required.", nameof(connectionString));

        var connection = new SqliteConnection(connectionString);
        connection.Open();
        Console.WriteLine($"Opened connection to database: {new SqliteConnectionStringBuilder(connectionString).DataSource}");
        return connection;
    }

    public static string[] BuildCurrencyTableCommands(string tableName, IEnumerable<CurrencyTableRow> rows)
    {
        if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name is required.", nameof(tableName));
        if (rows == null) throw new ArgumentNullException(nameof(rows));

        return
        [
            $@"
            CREATE TABLE IF NOT EXISTS {tableName} (
                Product TEXT,
                Variety1 REAL,
                Variety2 REAL,
                Variety3 REAL,
                Variety4 REAL
            );",
            .. rows.Select(row => BuildCurrencyInsertCommand(tableName, row))
        ];
    }

    private static string BuildConnectionString(string dbFileName)
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = dbFileName,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    private static string BuildCurrencyInsertCommand(string tableName, CurrencyTableRow row)
    {
        if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name is required.", nameof(tableName));
        if (row == null) throw new ArgumentNullException(nameof(row));

        return string.Format(
            CultureInfo.InvariantCulture,
            "INSERT OR REPLACE INTO {0} (Product, Variety1, Variety2, Variety3, Variety4) VALUES ('{1}', {2}, {3}, {4}, {5});",
            tableName,
            EscapeSqlString(row.Product),
            FormatNullableDouble(row.Variety1),
            FormatNullableDouble(row.Variety2),
            FormatNullableDouble(row.Variety3),
            FormatNullableDouble(row.Variety4));
    }

    private static string FormatNullableDouble(double? value)
    {
        return value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : "NULL";
    }

    private static string EscapeSqlString(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return value.Replace("'", "''");
    }
}
