Feature: Authentication
  As a trainer or client
  I want to register and log in
  So that I can securely access my account

  # --- Trainer Email Registration ---

  Scenario: [US-001] Trainer registers with email successfully
    Given I am on the registration page
    When I submit a valid email "trainer@example.com" and password "SecurePass123!"
    Then my account is created with emailConfirmed = false
    And a confirmation email is sent to "trainer@example.com"

  Scenario: [US-001] Trainer cannot log in before confirming email
    Given I have registered with email "trainer@example.com" but not confirmed it
    When I attempt to log in with correct credentials
    Then I receive a 401 error with message "Please confirm your email before logging in"

  Scenario: [US-001] Trainer confirms email and logs in
    Given I have received a confirmation email with a valid token
    When I click the confirmation link
    Then my email is marked as confirmed
    And I am redirected to the login page

  Scenario: [US-001] Registration fails for duplicate email
    Given a trainer account already exists with "trainer@example.com"
    When I try to register with the same email
    Then I see an error "An account with this email already exists"

  # --- Google OAuth ---

  Scenario: [US-002] Trainer signs in with Google (new account)
    Given I have no existing account
    When I complete Google OAuth with email "trainer@gmail.com"
    Then a new trainer account is created with googleId populated
    And I am redirected to the trainer dashboard

  Scenario: [US-002] Trainer signs in with Google (existing email account)
    Given a trainer account exists with email "trainer@gmail.com" (email/password)
    When I complete Google OAuth with the same email
    Then the accounts are linked (googleId is set on existing account)
    And I am logged in successfully

  # --- Client Login ---

  Scenario: [US-003] Client logs in successfully
    Given a client account was created by their trainer with email "client@example.com"
    When I log in with correct credentials
    Then I receive a JWT and see only my own workout history

  Scenario: [US-003] Client cannot access another client's data
    Given I am logged in as client "client-a@example.com"
    When I request workout sessions for client ID belonging to another client
    Then I receive a 403 Forbidden error
