using BoDi;
using TechTalk.SpecFlow;
using WTWTechTest.Context;
using WTWTechTest.Reporting;

namespace WTWTechTest.Hooks;

[Binding]
public class TestHooks
{
    private readonly IObjectContainer _objectContainer;

    public TestHooks(IObjectContainer objectContainer)
    {
        _objectContainer = objectContainer;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        ExtentReportManager.Initialize();
    }

    [BeforeScenario]
    public void BeforeScenario(ScenarioContext scenarioContext)
    {
        var testContext = new ScenarioTestContext();
        _objectContainer.RegisterInstanceAs(testContext);

        var nunitContext = NUnit.Framework.TestContext.CurrentContext;
        ExtentReportManager.StartTest(nunitContext);
    }

    [AfterScenario]
    public void AfterScenario(ScenarioContext scenarioContext)
    {
        var testContext = _objectContainer.Resolve<ScenarioTestContext>();
        testContext.DatabaseContext.Dispose();

        var nunitContext = NUnit.Framework.TestContext.CurrentContext;
        ExtentReportManager.CompleteTest(nunitContext);
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        ExtentReportManager.Flush();
        NUnit.Framework.TestContext.Progress.WriteLine($"Extent report generated: {ExtentReportManager.GetReportPath()}");
    }
}
