using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework.Interfaces;
using NUnit.Framework;

namespace WTWTechTest.Reporting;

public static class ExtentReportManager
{
    private static readonly object Sync = new();
    private static readonly AsyncLocal<ExtentTest?> CurrentTest = new();

    private static ExtentReports? _extent;
    private static string _reportDirectory = string.Empty;

    public static void Initialize()
    {
        lock (Sync)
        {
            if (_extent != null)
            {
                return;
            }

            _reportDirectory = ResolveReportDirectory();
            Directory.CreateDirectory(_reportDirectory);

            var reportFilePath = Path.Combine(_reportDirectory, "index.html");
            var reporter = new ExtentSparkReporter(reportFilePath);
            reporter.Config.DocumentTitle = "WTW Tech Test Report";
            reporter.Config.ReportName = "WTW Tech Test NUnit Results";

            _extent = new ExtentReports();
            _extent.AttachReporter(reporter);
            _extent.AddSystemInfo("Framework", ".NET + NUnit");
            _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
        }
    }

    public static void StartTest(TestContext context)
    {
        EnsureInitialized();

        var test = _extent!.CreateTest(context.Test.Name);
        if (!string.IsNullOrWhiteSpace(context.Test.ClassName))
        {
            test.AssignCategory(context.Test.ClassName);
        }

        CurrentTest.Value = test;
    }

    public static void CompleteTest(TestContext context)
    {
        var test = CurrentTest.Value;
        if (test == null)
        {
            return;
        }

        var result = context.Result;

        switch (result.Outcome.Status)
        {
            case TestStatus.Passed:
                test.Pass("Passed");
                break;
            case TestStatus.Failed:
                test.Fail(string.IsNullOrWhiteSpace(result.Message) ? "Failed" : result.Message);
                if (!string.IsNullOrWhiteSpace(result.StackTrace))
                {
                    test.Fail(result.StackTrace);
                }
                break;
            case TestStatus.Skipped:
                test.Skip(string.IsNullOrWhiteSpace(result.Message) ? "Skipped" : result.Message);
                break;
            default:
                test.Warning(string.IsNullOrWhiteSpace(result.Message) ? "Inconclusive" : result.Message);
                break;
        }

        CurrentTest.Value = null;
    }

    public static void Flush()
    {
        lock (Sync)
        {
            _extent?.Flush();
        }
    }

    public static string GetReportPath()
    {
        return Path.Combine(ResolveReportDirectory(), "index.html");
    }

    private static void EnsureInitialized()
    {
        if (_extent == null)
        {
            Initialize();
        }
    }

    private static string ResolveReportDirectory()
    {
        var configuredPath = Environment.GetEnvironmentVariable("EXTENT_REPORT_DIR");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.GetFullPath(Path.Combine(TestContext.CurrentContext.WorkDirectory, configuredPath));
        }

        return Path.Combine(FindSolutionRoot(), "extent-report");
    }

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (directory.EnumerateFiles("*.sln").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return TestContext.CurrentContext.WorkDirectory;
    }
}


