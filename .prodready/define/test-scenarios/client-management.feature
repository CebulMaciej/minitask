Feature: Client Management
  As a trainer
  I want to manage my client roster
  So that I can create training plans for each client

  Background:
    Given I am logged in as a trainer

  Scenario: [US-004] Add a new client
    Given I am on the client management page
    When I enter client name "John Doe" and email "john@example.com" and submit
    Then a new client profile is created scoped to my trainer account
    And an invitation email is sent to "john@example.com"
    And "John Doe" appears in my client list

  Scenario: [US-004] Cannot add duplicate client email within same trainer
    Given I already have a client with email "john@example.com"
    When I try to add another client with the same email
    Then I see an error "A client with this email already exists"

  Scenario: [US-005] View client list
    Given I have 3 clients added to my account
    When I navigate to the clients page
    Then I see all 3 clients listed with their names

  Scenario: [US-005] Trainer cannot see another trainer's clients
    Given trainer B has a client "secret@example.com"
    When I (trainer A) request the client list
    Then I do not see "secret@example.com" in the results
