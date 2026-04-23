using Microsoft.Data.Sqlite;

namespace WTWTechTest.Utilities;

public sealed class ValidationResult
{
    public int ComparedRows { get; set; }
    public List<string> Errors { get; } = new();
}

public static class ValidationUtilities
{
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

                var sourceValue = reader.GetDouble(aIndex);
                var targetValue = reader.GetDouble(bIndex);
                var expectedValue = sourceValue * rate;

                if (Math.Abs(targetValue - expectedValue) > tolerance)
                {
                    result.Errors.Add(
                        $"Value mismatch for Product='{product}', Column='{columns[i]}': expected {expectedValue}, actual {targetValue}");
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
}
