Feature: Currency Conversion Validation
  As a data analyst
  I want to validate currency conversions between tables
  So that I can ensure data integrity across different currency representations

  Background:
    Given the database is initialized with test data

  Scenario: Validate Sterling to Euro conversion with correct data
    When I validate conversion from "TableA_Sterling" to "TableB_Euro" using exchange rate from "GBP" to "EUR"
    Then the validation should have compared rows
    And there should be no conversion errors

  Scenario: Detect conversion errors in Euro table with errors
    When I validate conversion from "TableA_Sterling" to "TableC_EuroWithErrors" using exchange rate from "GBP" to "EUR"
    Then the validation should have compared rows
    And there should be conversion errors detected
