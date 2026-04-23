namespace WTWTechTest.Utilities;

public static class FileUtilities
{
    private const string TempFolderName = "temp";

    public static string GetTempFolderPath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), TempFolderName);
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
            Console.WriteLine($"Deleted existing file at: {filePath}");
        }
    }

    public static void DeleteDirectoryIfExists(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentException("Directory path is required.", nameof(directoryPath));

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }
}
