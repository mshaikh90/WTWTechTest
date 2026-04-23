using CsvHelper;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace WTWTechTest.Utilities;

public static class CsvUtilities
{
    public static void ExecuteQueryToCsv(string connectionString, string query, string filename)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string is required.", nameof(connectionString));
        if (string.IsNullOrEmpty(query)) throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        if (string.IsNullOrEmpty(filename)) throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));

        using var connection = SqlUtilities.OpenConnection(connectionString);
        using var command = new SqliteCommand(query, connection);
        using var reader = command.ExecuteReader();

        var tempFolder = FileUtilities.GetTempFolderPath();
        Directory.CreateDirectory(tempFolder);
        var filePath = FileUtilities.GetCsvFilePath(filename);

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            csv.WriteField(reader.GetName(i));
        }
        csv.NextRecord();

        while (reader.Read())
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                csv.WriteField(reader[i]);
            }
            csv.NextRecord();
        }
    }
}
