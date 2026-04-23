# SpecFlow Test Implementation

## Overview
This project has been refactored to use **SpecFlow** with a clean, maintainable structure following the **Page Object Model** and **DRY principles**.

## Architecture

### 📁 Project Structure
```
WTWTechTest/
├── Context/                    # Context objects (Page Object Model for DB)
│   ├── DatabaseContext.cs     # Database setup and teardown
│   └── ScenarioTestContext.cs # Test state management
├── Features/                   # Gherkin feature files
│   ├── CurrencyConversion.feature
│   └── QueryExport.feature
├── Hooks/                      # SpecFlow lifecycle hooks
│   └── TestHooks.cs           # Before/After scenario and test run hooks
├── StepDefinitions/           # Reusable step implementations
│   ├── DatabaseSteps.cs       # Database initialization steps
│   ├── CurrencyConversionSteps.cs  # Currency validation steps
│   └── QueryExportSteps.cs    # CSV export steps
├── Utilities/                 # Helper utilities
│   └── Utilities.cs           # Database and validation utilities
├── Configuration/             # Configuration management
│   ├── TestConfigurationFactory.cs
│   └── TestConfigurationExtensions.cs
└── Reporting/                 # ExtentReports integration
    └── ExtentReportManager.cs

```

## Key Features

### ✅ Clean Gherkin Syntax
Feature files focus on **business-readable language** with no implementation details:

```gherkin
Scenario: Validate Sterling to Euro conversion with correct data
  When I validate conversion from "TableA_Sterling" to "TableB_Euro" using exchange rate from "GBP" to "EUR"
  Then the validation should have compared rows
  And there should be no conversion errors
```

### ✅ Reusable Step Definitions
Step definitions are **parameterized** and **reusable** across multiple scenarios:

```csharp
[When(@"I validate conversion from ""([^""]*)"" to ""([^""]*)"" using exchange rate from ""([^""]*)"" to ""([^""]*)""")]
public void WhenIValidateConversionBetweenTables(string sourceTable, string targetTable, 
    string sourceCurrency, string targetCurrency)
```

### ✅ Page Object Model (Context Pattern)
- **DatabaseContext**: Manages database lifecycle (setup, connections, teardown)
- **ScenarioTestContext**: Stores test state between steps (DI via SpecFlow)

### ✅ DRY Principles
- Database setup logic centralized in `DatabaseContext`
- Step definitions reuse existing utilities
- No code duplication across scenarios
- Shared hooks for reporting and lifecycle management

### ✅ Integration with Existing Infrastructure
- **ExtentReports**: Automatic HTML report generation
- **Configuration**: Uses existing `appsettings.json` configuration
- **SQLite**: Maintains existing database testing approach
- **Utilities**: Reuses all existing validation and helper methods

## Running Tests

### Command Line
```bash
dotnet test
```

### Visual Studio
1. Open Test Explorer
2. Run all tests or individual scenarios

### Test Reports
- HTML reports are generated in: `extent-report/index.html`
- Reports include scenario names, step execution times, and failure details

## SpecFlow Configuration

The project uses `specflow.json` for configuration:
- Language: English (en)
- Culture: en-US
- Trace successful steps and timings

## Removed Files

The following legacy NUnit test files have been removed:
- ❌ `Tests/SqlDbTests.cs` (replaced by CurrencyConversion.feature)
- ❌ `Tests/BaseTest.cs` (functionality moved to DatabaseContext)
- ❌ `Tests/ReportingSetup.cs` (functionality moved to TestHooks)

## Benefits of This Implementation

1. **Readability**: Feature files are readable by non-technical stakeholders
2. **Maintainability**: Step definitions are centralized and reusable
3. **Separation of Concerns**: Clear separation between test logic, data, and assertions
4. **Scalability**: Easy to add new scenarios by reusing existing steps
5. **Clean**: Follows SpecFlow best practices and DRY principles
6. **Integrated**: Works seamlessly with existing reporting and configuration

## Scenarios Covered

### Currency Conversion Validation
- ✅ Validate correct Sterling to Euro conversion
- ✅ Detect conversion errors in tables with data issues

### Database Query Export
- ✅ Export query results to CSV files

## Dependencies

- **SpecFlow 3.9.74**: BDD framework
- **SpecFlow.NUnit 3.9.74**: NUnit integration
- **NUnit 4.3.2**: Test runner
- **ExtentReports 5.0.4**: HTML reporting
- **Microsoft.Data.Sqlite 9.0.0**: Database testing
- **CsvHelper 33.0.1**: CSV export functionality
