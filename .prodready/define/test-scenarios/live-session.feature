Feature: Live Training Session
  As a trainer
  I want to run a live training session mode
  So that I can capture actual performance in real time

  Background:
    Given I am logged in as a trainer
    And a PLANNED session exists for client "Jane Smith" with exercises configured

  Scenario: [US-009] Start a live session
    Given I am viewing Jane's scheduled session
    When I tap "Start Session"
    Then the session status changes to IN_PROGRESS
    And I see all planned exercises with their target values

  Scenario: [US-009] Live session is mobile-friendly
    Given a session is IN_PROGRESS
    When I view it on a mobile viewport (375px width)
    Then all controls are accessible without horizontal scrolling

  Scenario: [US-009] Session state persists if I navigate away
    Given a session is IN_PROGRESS with some exercises logged
    When I navigate to a different page and return
    Then the session is still IN_PROGRESS with my logged data intact

  Scenario: [US-010] Log unexpected progress
    Given a live session is active with "Bench Press" planned at 60kg
    When I tap the exercise and enter actual weight 70kg
    Then the exercise is marked as unexpectedProgress = true
    And it is visually highlighted differently from normal exercises

  Scenario: [US-010] Log actual values matching plan (no flag)
    Given a live session is active with "Bench Press" planned at 60kg
    When I enter actual weight 60kg
    Then the exercise is NOT flagged as unexpected progress

  Scenario: [US-011] Complete a session
    Given a live session is IN_PROGRESS
    When I tap "Finish Session" and confirm
    Then the session status changes to COMPLETED
    And completedAt is recorded
    And the session appears in Jane's workout history

  Scenario: [US-011] Actual values are stored separately from planned
    Given I complete a session where I overrode weights for 2 exercises
    When I retrieve the session data
    Then planned and actual values are both present for each exercise
