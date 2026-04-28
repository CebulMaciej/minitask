Feature: Workout Scheduling
  As a trainer
  I want to schedule and configure workout sessions for clients
  So that training plans are structured and trackable

  Background:
    Given I am logged in as a trainer
    And I have a client "Jane Smith" in my roster

  Scenario: [US-006] Schedule a new workout session
    Given I am on Jane's calendar view
    When I click on "2026-05-01" at "10:00"
    Then a new session form opens pre-filled with that date and time
    When I save the session
    Then the session appears on the calendar at the correct slot

  Scenario: [US-007] Add exercises to a session
    Given I am creating a session for Jane
    When I add exercise "Bench Press" with sets=3, reps=10, weight=60kg
    And I add exercise "Pull-ups" with sets=3, reps=8, weight=null (bodyweight)
    And I save the session
    Then the session contains both exercises with their correct values

  Scenario: [US-007] Exercises are saved in order
    Given I add exercises A, B, C to a session
    When I save and reopen the session
    Then exercises appear in the same order: A, B, C

  Scenario: [US-008] Edit a scheduled session
    Given a session exists on "2026-05-01" for Jane
    When I click the session and choose edit
    And I change the time to "11:00" and save
    Then the session now shows "11:00" on the calendar

  Scenario: [US-008] Delete a scheduled session
    Given a PLANNED session exists for Jane
    When I choose delete and confirm
    Then the session is removed from the calendar

  Scenario: [US-008] Cannot silently delete a session in progress
    Given a session is in status IN_PROGRESS
    When I try to delete it
    Then I see a warning "This session is currently in progress. Are you sure?"
