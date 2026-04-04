namespace WTWTechTest.Tests;

[TestFixture]
public class SqlDbTests : BaseTest
{

    [Test]
    public void TestQueryToCsv()
    {
        // Set variable for filename
        var filename = "sterlingTable";

        // Example: Export a query result to CSV
        Utilities.Utilities.ExecuteQueryToCsv(ConnectionString, $"SELECT * FROM {TableASterling}", filename);
        var filePath = Utilities.Utilities.GetCsvFilePath(filename);
        Assert.That(File.Exists(filePath), Is.True);
    }

    [Test]
    public void SterlingValues_ShouldMatchEuroValues_UsingExchangeRate()
    {
        var validation = Utilities.Utilities.ValidateConversionBetweenTables(
            Connection!,
            TableASterling,
            TableBEuro,
            "GBP",
            "EUR");

        Assert.That(validation.ComparedRows, Is.GreaterThan(0), "No joined rows were found to compare.");
        Assert.That(validation.Errors, Is.Empty, string.Join(Environment.NewLine, validation.Errors));
    }

    [Test]
    public void SterlingValues_ShouldDetectErrorsInEuroWithErrorsTable()
    {
        var validation = Utilities.Utilities.ValidateConversionBetweenTables(
            Connection!,
            TableASterling,
            TableCEuroWithErrors,
            "GBP",
            "EUR");

        Assert.That(validation.ComparedRows, Is.GreaterThan(0), "No joined rows were found to compare.");
        Assert.That(validation.Errors, Is.Not.Empty, "Expected conversion errors to be detected for TableC_EuroWithErrors.");
    }
    
    [Test]
    public void ThisTestWillFailSterlingValues_ShouldDetectErrorsInEuroWithErrorsTable()
    {
        var validation = Utilities.Utilities.ValidateConversionBetweenTables(
            Connection!,
            TableASterling,
            TableCEuroWithErrors,
            "GBP",
            "EUR");

        Assert.That(validation.ComparedRows, Is.GreaterThan(0), "No joined rows were found to compare.");
        Assert.That(validation.Errors, Is.Empty, "Expected failure to show what failure messages look like.");
    }
    
    [Test]
    public void ThisTestWillIntentionallyFail()
    {
        var validation = Utilities.Utilities.ValidateConversionBetweenTables(
            Connection!,
            "iDontExist",
            "iDontExistEither",
            "GBP",
            "EUR");

        Assert.That(validation.ComparedRows, Is.GreaterThan(0), "No joined rows were found to compare.");
        Assert.That(validation.Errors, Is.Empty, string.Join(Environment.NewLine, validation.Errors));
    }
}
