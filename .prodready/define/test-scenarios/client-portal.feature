Feature: Client Portal
  As a client
  I want to view my completed training sessions
  So that I can review my workout history and progress

  Background:
    Given I am logged in as a client

  Scenario: [US-012] View list of completed sessions
    Given I have 5 completed sessions across different dates
    When I open my portal
    Then I see 5 sessions listed sorted by date (newest first)

  Scenario: [US-012] View session detail with planned vs actual
    Given a completed session exists with exercises that had unexpected progress
    When I click on that session
    Then I see each exercise with both planned and actual values
    And exercises with unexpected progress are visually highlighted

  Scenario: [US-012] Empty state for no sessions
    Given I have no completed sessions
    When I open my portal
    Then I see an encouraging empty state message (not a blank page)

  Scenario: [US-012] Client cannot see other clients' sessions
    Given I am client A
    When I request session data for client B's ID
    Then I receive a 403 Forbidden error
