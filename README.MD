# Welcome to my attempt at the WTW Tech Test!
I hope you like what you see :)

## Quick start - 
1. Clone the repo
2. Open solution in Visual Studio or your favourite IDE
3. Build the solution
4. Run the tests either by Test Explorer or running `dotnet test` in the terminal window

## Quick note -
Two of the 5 tests written are expected to fail, this is because I wanted to demonstrate the information surfaced when a test fails.
The first test in SqlDbTests is testing the functionality of the DB to CSV writer, as this was the approach I was going to take first (pull data from DB into CSV, run assertions on DB, until I found that I can store values from DB in memory and run tests on stored lists, I thought to keep the test and related functionality in anyway just to show my initial thought process.)
I did use AI to help me build this project, however I fully understand all of the code, what it is doing and why. I am aware there is some slight overlap in the environment grabbing stuff at the moment, and this is something I would make more solid if I had more time to focus on it. 

One more quick side note - this framework spins up an SQLite DB for use in testing, I know that in reality we would be connecting to an external DB via a connection string. To adapt this framework to do that won't take much work, without having delved into it too deeply, I think it's just a namespace switch from Microsoft.Data.Sqlite to the equivalent Microsoft SQL namespace, doing some cleanup around the tables creation logic, and then we can just store data from tables in memory and the rest of the framework will remain largely unchanged. 

## Simple overview
- BaseTest - where the Nunit OnetimeSetup and OnetimeTeardown methods are, these include setting up DB with tables, grabbing enviornment config and deleting DB and temp files after run
- SqlDbTests - this is where the tests are, this class and other test classes if they were made inheirit from BaseTest so they can use the setup and teardown methods
- Utilities - this is where all of the helper methods are, creating DB connections, writing to CSV, table validation and a stub that acts as a service call for getting the latest exchange rate. In reality I would split this file out into more focussed files.
- TestConfiguration - where the heavy lifting for environment grabbing is.
- appsettings.json - where the DB connection string is stored, this is read by the TestConfiguration class and used in the tests. In a real world scenario I would have different appsettings for different environments and use something like `dotnet user-secrets` to store sensitive information like connection strings, but for the sake of this test I have kept it simple.

I think that's it! Any questions, I'd be more than happy to answer!

