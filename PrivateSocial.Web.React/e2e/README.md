# End-to-End Tests

This directory contains Playwright end-to-end tests for the PrivateSocial application.

## Prerequisites

1. Node.js and npm installed
2. Playwright browsers installed (run `npx playwright install` if not already done)
3. The backend API must be running

## Running the Tests

### Option 1: Automated (Recommended)

Run the tests with automatic backend startup:

```bash
# From PrivateSocial.Web.React directory
npm run test:e2e:full
```

This script will:
1. Start the backend services automatically
2. Wait for the backend to be ready
3. Run the Playwright tests
4. Clean up when tests complete

### Option 2: Manual Setup

#### Step 1: Start the Backend

In one terminal, start the backend application:

```bash
# From the repository root
dotnet run --project PrivateSocial.AppHost
```

Wait for the application to fully start. You should see the Aspire dashboard URL in the output.

#### Step 2: Run the Tests

In another terminal, navigate to the React project and run the tests:

```bash
# From PrivateSocial.Web.React directory
npm run test:e2e
```

## Available Test Commands

- `npm run test:e2e` - Run all tests in headless mode
- `npm run test:e2e:ui` - Run tests with Playwright UI (interactive mode)
- `npm run test:e2e:debug` - Run tests in debug mode
- `npm run test:e2e:report` - Show the HTML test report after tests have run

## Test Coverage

### Authentication Tests (`auth.spec.ts`)
- User registration with valid data
- Registration error handling (duplicate username, password validation)
- User login with valid credentials
- Login error handling (invalid credentials)
- Logout functionality
- Protected route access control

### Posts Tests (`posts.spec.ts`)
- Creating new posts successfully
- Empty post validation
- Character count display and limit enforcement
- Deleting own posts
- Permission checks (can't edit/delete others' posts)
- Pagination functionality
- Post list refresh after creation
- Chronological ordering (newest first)
- Network error handling

## Test Structure

- `/e2e/tests/` - Test files
  - `auth.spec.ts` - Authentication flow tests
  - `posts.spec.ts` - Post creation and management tests
- `/e2e/utils/` - Utility functions
  - `test-helpers.ts` - Helper functions for test data generation and common operations

## Troubleshooting

### Tests failing with "Connection refused"

Make sure the backend is running on the expected port. The tests expect:
- Frontend: http://localhost:3000
- Backend API: http://localhost:5001

### Timeout errors

If tests are timing out, you may need to:
1. Increase the timeout in specific tests
2. Ensure your system isn't under heavy load
3. Check that both frontend and backend are responding properly

### Running specific tests

To run a specific test file:
```bash
npx playwright test e2e/tests/auth.spec.ts
```

To run tests in a specific browser:
```bash
npx playwright test --project=chromium
```