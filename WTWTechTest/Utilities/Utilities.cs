using Microsoft.Data.Sqlite;
using CsvHelper;
using System.Globalization;


// Currently this file contains various helpers and utilities for both creating the test tables that this set of tests works off
// Environment config and other stuff. The custom assertion tool for checking values from one table to another table 
// after getting the current exchange rate is in this file. In a real implementation I would split this file out into 
// different Utility / helper files separated by category, ie. SqlUtilities, FileUtilities, ValidationUtilities etc. 

namespace WTWTechTest.Utilities;

public sealed class ValidationResult
{
    public int ComparedRows { get; set; }
    public List<string> Errors { get; } = new();
}

public sealed record CurrencyTableRow(
    string Product,
    double? Variety1,
    double? Variety2,
    double? Variety3,
    double? Variety4);

public static class Utilities
{
    public const string DefaultDatabaseFileName = "test.db";
    private const string TempFolderName = "temp";

    public static string GetTempFolderPath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), TempFolderName);
    }

    public static void DeleteDirectoryIfExists(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentException("Directory path is required.", nameof(directoryPath));

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }

    public static string GetCsvFilePath(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));

        return Path.Combine(GetTempFolderPath(), $"{filename}.csv");
    }

    public static string GetDatabaseFilePath(string dbFileName)
    {
        if (string.IsNullOrWhiteSpace(dbFileName)) throw new ArgumentException("Database filename is required.", nameof(dbFileName));

        return Path.Combine(Directory.GetCurrentDirectory(), dbFileName);
    }

    public static void DeleteFileIfExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path is required.", nameof(filePath));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

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

        return $"{nameWithoutExtension}_{Guid.NewGuid():N}{extension}";
    }

    public static string[] BuildCurrencyTableCommands(string tableName, IEnumerable<CurrencyTableRow> rows)
    {
        if (!IsSafeSqlIdentifier(tableName)) throw new ArgumentException("Table name is invalid.", nameof(tableName));
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

    public static void ExecuteQueryToCsv(string connectionString, string query, string filename)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string is required.", nameof(connectionString));
        if (string.IsNullOrEmpty(query)) throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        if (string.IsNullOrEmpty(filename)) throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));

        using var connection = OpenConnection(connectionString);
        using var command = new SqliteCommand(query, connection);
        using var reader = command.ExecuteReader();

        var tempFolder = GetTempFolderPath();
        Directory.CreateDirectory(tempFolder);
        var filePath = GetCsvFilePath(filename);

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write headers
        for (int i = 0; i < reader.FieldCount; i++)
        {
            csv.WriteField(reader.GetName(i));
        }
        csv.NextRecord();

        // Write data
        while (reader.Read())
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                csv.WriteField(reader[i]);
            }
            csv.NextRecord();
        }
    }

    public static string InitializeDatabase(string dbFileName)
    {
        if (string.IsNullOrWhiteSpace(dbFileName)) throw new ArgumentException("Database filename is required.", nameof(dbFileName));

        var dbFilePath = GetDatabaseFilePath(dbFileName);
        DeleteFileIfExists(dbFilePath);

        var connectionString = BuildConnectionString(dbFileName);

        using var connection = OpenConnection(connectionString);

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

    public static ValidationResult ValidateConversionBetweenTables(
        SqliteConnection connection,
        string sourceTableName,
        string targetTableName,
        string sourceCurrency,
        string targetCurrency,
        double tolerance = 1e-9)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));
        if (string.IsNullOrWhiteSpace(sourceTableName)) throw new ArgumentException("Source table is required.", nameof(sourceTableName));
        if (string.IsNullOrWhiteSpace(targetTableName)) throw new ArgumentException("Target table is required.", nameof(targetTableName));
        if (string.IsNullOrWhiteSpace(sourceCurrency)) throw new ArgumentException("Source currency is required.", nameof(sourceCurrency));
        if (string.IsNullOrWhiteSpace(targetCurrency)) throw new ArgumentException("Target currency is required.", nameof(targetCurrency));
        if (tolerance < 0) throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be zero or positive.");
        if (!IsSafeSqlIdentifier(sourceTableName)) throw new ArgumentException("Source table name is invalid.", nameof(sourceTableName));
        if (!IsSafeSqlIdentifier(targetTableName)) throw new ArgumentException("Target table name is invalid.", nameof(targetTableName));

        var result = new ValidationResult();
        var columns = new[] { "Variety1", "Variety2", "Variety3", "Variety4" };
        var rate = GetLatestExchangeRate(sourceCurrency, targetCurrency);

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT
                a.Product,
                a.Variety1 AS AVariety1, a.Variety2 AS AVariety2, a.Variety3 AS AVariety3, a.Variety4 AS AVariety4,
                b.Variety1 AS BVariety1, b.Variety2 AS BVariety2, b.Variety3 AS BVariety3, b.Variety4 AS BVariety4
            FROM {sourceTableName} a
            INNER JOIN {targetTableName} b ON b.Product = a.Product
            ORDER BY a.Product;";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.ComparedRows++;
            var product = reader.GetString(0);

            for (var i = 0; i < columns.Length; i++)
            {
                var aIndex = 1 + i;
                var bIndex = 5 + i;
                var aIsNull = reader.IsDBNull(aIndex);
                var bIsNull = reader.IsDBNull(bIndex);

                if (aIsNull != bIsNull)
                {
                    result.Errors.Add($"Null mismatch for Product='{product}', Column='{columns[i]}'");
                    continue;
                }

                if (aIsNull)
                {
                    continue;
                }

                var sourceTableValue = reader.GetDouble(aIndex);
                var targetTableValue = reader.GetDouble(bIndex);
                var expectedTargetTableValue = sourceTableValue * rate;

                if (Math.Abs(targetTableValue - expectedTargetTableValue) > tolerance)
                {
                    result.Errors.Add(
                        $"Value mismatch for Product='{product}', Column='{columns[i]}': expected {expectedTargetTableValue}, actual {targetTableValue}");
                }
            }
        }

        if (result.ComparedRows == 0)
        {
            result.Errors.Add("No joined rows were found to compare.");
        }

        return result;
    }

    public static double GetLatestExchangeRate(string currency1, string currency2)
    {
        // Stub implementation: returns a flat rate of 1.5
        return 1.5;
    }

    private static string BuildConnectionString(string dbFileName)
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = dbFileName,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    public static SqliteConnection OpenConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string is required.", nameof(connectionString));

        var builder = new SqliteConnectionStringBuilder(connectionString)
        {
            Mode = SqliteOpenMode.ReadWriteCreate
        };

        var connection = new SqliteConnection(builder.ToString());
        connection.Open();
        return connection;
    }

    private static string BuildCurrencyInsertCommand(string tableName, CurrencyTableRow row)
    {
        if (!IsSafeSqlIdentifier(tableName)) throw new ArgumentException("Table name is invalid.", nameof(tableName));
        if (row == null) throw new ArgumentNullException(nameof(row));

        return string.Format(
            CultureInfo.InvariantCulture,
            "INSERT OR REPLACE INTO {0} (Product, Variety1, Variety2, Variety3, Variety4) VALUES ('{1}', {2}, {3}, {4}, {5});",
            tableName,
            EscapeSqlString(row.Product),
            FormatNullableNumber(row.Variety1),
            FormatNullableNumber(row.Variety2),
            FormatNullableNumber(row.Variety3),
            FormatNullableNumber(row.Variety4));
    }

    private static string FormatNullableNumber(double? value)
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

    private static bool IsSafeSqlIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!(char.IsLetter(value[0]) || value[0] == '_'))
        {
            return false;
        }

        for (var i = 1; i < value.Length; i++)
        {
            if (!(char.IsLetterOrDigit(value[i]) || value[i] == '_'))
            {
                return false;
            }
        }

        return true;
    }
}
