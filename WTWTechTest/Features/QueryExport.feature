Feature: Database Query Export
  As a data analyst
  I want to export query results to CSV
  So that I can analyze data in external tools

  Background:
    Given the database is initialized with test data

  Scenario: Export table query to CSV file
    When I export the query "SELECT * FROM TableA_Sterling" to CSV file "sterlingTable"
    Then the CSV file "sterlingTable" should exist
