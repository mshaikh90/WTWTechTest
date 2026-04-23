using NUnit.Framework;
using TechTalk.SpecFlow;
using WTWTechTest.Context;
using WTWTechTest.Utilities;

namespace WTWTechTest.StepDefinitions;

[Binding]
public class CurrencyConversionSteps
{
    private readonly ScenarioTestContext _testContext;

    public CurrencyConversionSteps(ScenarioTestContext testContext)
    {
        _testContext = testContext;
    }

    [When(@"I validate conversion from ""([^""]*)"" to ""([^""]*)"" using exchange rate from ""([^""]*)"" to ""([^""]*)""")]
    public void WhenIValidateConversionBetweenTables(string sourceTable, string targetTable, string sourceCurrency, string targetCurrency)
    {
        _testContext.ValidationResult = ValidationUtilities.ValidateConversionBetweenTables(
            _testContext.DatabaseContext.Connection!,
            sourceTable,
            targetTable,
            sourceCurrency,
            targetCurrency);
    }

    [Then(@"the validation should have compared rows")]
    public void ThenTheValidationShouldHaveComparedRows()
    {
        Assert.That(_testContext.ValidationResult, Is.Not.Null, "Validation result should not be null");
        Assert.That(_testContext.ValidationResult!.ComparedRows, Is.GreaterThan(0), 
            "No joined rows were found to compare.");
    }

    [Then(@"there should be no conversion errors")]
    public void ThenThereShouldBeNoConversionErrors()
    {
        Assert.That(_testContext.ValidationResult, Is.Not.Null, "Validation result should not be null");
        Assert.That(_testContext.ValidationResult!.Errors, Is.Empty, 
            string.Join(Environment.NewLine, _testContext.ValidationResult.Errors));
    }

    [Then(@"there should be conversion errors detected")]
    public void ThenThereShouldBeConversionErrorsDetected()
    {
        Assert.That(_testContext.ValidationResult, Is.Not.Null, "Validation result should not be null");
        Assert.That(_testContext.ValidationResult!.Errors, Is.Not.Empty, 
            "Expected conversion errors to be detected.");
    }

    [Then(@"the validation should have no compared rows")]
    public void ThenTheValidationShouldHaveNoComparedRows()
    {
        Assert.That(_testContext.ValidationResult, Is.Not.Null, "Validation result should not be null");
        Assert.That(_testContext.ValidationResult!.ComparedRows, Is.EqualTo(0), 
            "Expected no rows to be compared.");
    }
}
