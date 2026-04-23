using TechTalk.SpecFlow;
using WTWTechTest.Context;

namespace WTWTechTest.StepDefinitions;

[Binding]
public class DatabaseSteps
{
    private readonly ScenarioTestContext _testContext;

    public DatabaseSteps(ScenarioTestContext testContext)
    {
        _testContext = testContext;
    }

    [Given(@"the database is initialized with test data")]
    public void GivenTheDatabaseIsInitializedWithTestData()
    {
        _testContext.DatabaseContext.InitializeDatabase();
    }
}
