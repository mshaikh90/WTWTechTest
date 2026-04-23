using WTWTechTest.Utilities;

namespace WTWTechTest.Context;

public class ScenarioTestContext
{
    public DatabaseContext DatabaseContext { get; set; } = new();
    public ValidationResult? ValidationResult { get; set; }
    public string? CsvFilePath { get; set; }
}
