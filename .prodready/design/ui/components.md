# UI Components

## Primitives
- [ ] Button (primary/lime, secondary/ghost, danger, icon-only)
- [ ] Input (text, email, password with show/hide toggle)
- [ ] NumberInput (sets, reps, weight — large tap targets for mobile)
- [ ] Select / Dropdown
- [ ] Badge (status: PLANNED, IN_PROGRESS, COMPLETED; unexpected progress pill)
- [ ] Avatar (client initials fallback)
- [ ] Spinner / LoadingDots

## Composite
- [ ] Card (dark surface, subtle border)
- [ ] Modal (dark elevated background, backdrop blur)
- [ ] Toast / Notification (success, error, warning)
- [ ] EmptyState (icon + message + optional CTA)
- [ ] ConfirmDialog (used for delete + session cancel actions)

## Layout
- [ ] AppShell (sidebar nav + main content area)
- [ ] MobileBottomNav (trainer app bottom tab bar)
- [ ] PageHeader (title + breadcrumb)
- [ ] Container (max-width wrapper with responsive padding)

## Trainer-Specific
- [ ] CalendarGrid (monthly/weekly view, clickable day slots)
- [ ] SessionCard (shows time, exercise count, status badge — used on calendar)
- [ ] ExerciseRow (name, sets × reps @ weight — used in session detail and live mode)
- [ ] ExerciseInputRow (live session — planned values shown, actual value inputs)
- [ ] UnexpectedProgressBadge (lime/accent highlight when actual > planned)
- [ ] LiveSessionHeader (sticky — session timer, client name, Finish button)
- [ ] ClientListItem (avatar, name, last session date)

## Client Portal-Specific
- [ ] SessionHistoryCard (date, exercise count, has unexpected progress indicator)
- [ ] ExerciseResultRow (planned vs actual comparison, progress flag)

## Pages
- [ ] LoginPage (trainer + client, Google OAuth button, email form)
- [ ] RegisterPage (trainer only)
- [ ] TrainerDashboard (client list + recent activity)
- [ ] ClientCalendarPage (calendar view for a specific client)
- [ ] SessionDetailPage (view/edit planned session)
- [ ] LiveSessionPage (full-screen live mode, mobile-optimized)
- [ ] ClientPortalPage (client's session history list)
- [ ] SessionHistoryDetailPage (client view of a completed session)
