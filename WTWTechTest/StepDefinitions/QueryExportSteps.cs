using NUnit.Framework;
using TechTalk.SpecFlow;
using WTWTechTest.Context;
using WTWTechTest.Utilities;

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
        CsvUtilities.ExecuteQueryToCsv(
            _testContext.DatabaseContext.ConnectionString,
            query,
            filename);

        _testContext.CsvFilePath = FileUtilities.GetCsvFilePath(filename);
    }

    [Then(@"the CSV file ""([^""]*)"" should exist")]
    public void ThenTheCsvFileShouldExist(string filename)
    {
        Assert.That(File.Exists(_testContext.CsvFilePath), Is.True,
            $"Expected CSV file to exist at: {_testContext.CsvFilePath}");
    }
}
