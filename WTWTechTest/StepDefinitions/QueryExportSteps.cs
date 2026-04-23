using NUnit.Framework;
using TechTalk.SpecFlow;
using WTWTechTest.Context;

namespace WTWTechTest.StepDefinitions;

[Binding]
public class QueryExportSteps
{
    private readonly ScenarioTestContext _testContext;

    public QueryExportSteps(ScenarioTestContext testContext)
    {
        _testContext = testContext;
    }

    [When(@"I export the query ""([^""]*)"" to CSV file ""([^""]*)""")]
    public void WhenIExportTheQueryToCsvFile(string query, string filename)
    {
        Utilities.Utilities.ExecuteQueryToCsv(
            _testContext.DatabaseContext.ConnectionString, 
            query, 
            filename);

        _testContext.CsvFilePath = Utilities.Utilities.GetCsvFilePath(filename);
    }

    [Then(@"the CSV file ""([^""]*)"" should exist")]
    public void ThenTheCsvFileShouldExist(string filename)
    {
        var filePath = Utilities.Utilities.GetCsvFilePath(filename);
        Assert.That(File.Exists(filePath), Is.True, 
            $"Expected CSV file to exist at: {filePath}");
    }
}
