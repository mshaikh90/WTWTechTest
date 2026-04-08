# Welcome to my attempt at the WTW Tech Test!
I hope you like what you see :)

## Quick start - 
1. Clone the repo
2. Open solution in Visual Studio or your favourite IDE
3. Build the solution
4. Run the tests either by Test Explorer or running `dotnet test` in the terminal window

## Quick note -
Two of the 5 tests written are expected to fail, this is because I wanted to demonstrate the information surfaced when a test fails.
The first test in SqlDbTests is testing the functionality of the DB to CSV writer, as this was the approach I was going to take first (pull data from DB into CSV, run assertions on CSV files, until I found that I can store values from DB in memory and run tests on stored lists, I thought to keep the test and related functionality in anyway just to show my initial thought process.)
I did use AI to help me build this project, however I fully understand all of the code, what it is doing and why. I am aware there is some slight overlap in the environment grabbing stuff at the moment, and this is something I would make more solid if I had more time to focus on it. 

One more quick side note - this framework spins up an SQLite DB for use in testing, I know that in reality we would be connecting to an external DB via a connection string. To adapt this framework to do that won't take much work, without having delved into it too deeply, I think it's just a namespace switch from Microsoft.Data.Sqlite to the equivalent Microsoft SQL namespace, doing some cleanup around the tables creation logic, and then we can just store data from tables in memory and the rest of the framework will remain largely unchanged. 

Another side, side note - I developed this solution on MacOS using JetBrains Rider IDE, I did before sending my email with link to fork pull the repo onto an old Windows machine I had lying around and test that it works on Windows 11, Visual Studio. All good on my side and I am confident you won't have any issues either!

Yet another side note - 

I’ve kept the environment/config side of this project pretty lightweight. At the moment the tests read from `appsettings.json`, and can also pick up an environment-specific file if `DOTNET_ENVIRONMENT` is set. If it isn’t set, the project just falls back to the default config.

For this technical test that felt like a good middle ground between keeping things simple and still showing the general shape of how I’d manage config in a .NET project.

In a real-world setup, I’d be a bit stricter with environment separation and I wouldn’t keep sensitive values in source-controlled config files. For local development I’d likely use something like `dotnet user-secrets`, and for deployed environments I’d expect configuration and secrets to come from environment variables or a secure secrets store.


## Simple overview
- BaseTest - where the Nunit OnetimeSetup and OnetimeTeardown methods are, these include setting up DB with tables, grabbing enviornment config and deleting DB and temp files after run
- SqlDbTests - this is where the tests are, this class and other test classes if they were made inheirit from BaseTest so they can use the setup and teardown methods
- Utilities - this is where all of the helper methods are, creating DB connections, writing to CSV, table validation and a stub that acts as a service call for getting the latest exchange rate. In reality I would split this file out into more focussed files.
- TestConfiguration - where the heavy lifting for environment grabbing is.
- appsettings.json - where the DB connection string is stored, this is read by the TestConfiguration class and used in the tests. In a real world scenario I would have different appsettings for different environments and use something like `dotnet user-secrets` to store sensitive information like connection strings, but for the sake of this test I have kept it simple.

I think that's it! Any questions, I'd be more than happy to answer!

## Reporting

I have added ExtentReports so test runs output a detailed HTML report.

- Default output folder: `extent-report`
- Main report file: `extent-report/index.html`
- You can override the output folder with `EXTENT_REPORT_DIR`

Run tests as normal:

```bash
dotnet test
```

