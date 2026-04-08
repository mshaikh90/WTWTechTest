using WTWTechTest.Reporting;

namespace WTWTechTest.Tests;

[SetUpFixture]
public sealed class ReportingSetup
{
    [OneTimeSetUp]
    public void InitializeReporting()
    {
        ExtentReportManager.Initialize();
    }

    [OneTimeTearDown]
    public void FlushReporting()
    {
        ExtentReportManager.Flush();
        TestContext.Progress.WriteLine($"Extent report generated: {ExtentReportManager.GetReportPath()}");
    }
}

