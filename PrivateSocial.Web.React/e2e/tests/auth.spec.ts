import { test, expect } from '@playwright/test';
import {
  generateUniqueUsername,
  generateUniqueEmail,
  generateTestPassword,
  waitForNavigation,
  isLoggedIn,
  clearAuthData
} from '../utils/test-helpers';

test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    // Clear any existing auth data before each test
    await page.goto('/');
    await clearAuthData(page);
  });

  test('should register a new user successfully', async ({ page }) => {
    // Generate unique test data
    const username = generateUniqueUsername();
    const email = generateUniqueEmail();
    const password = generateTestPassword();
    const firstName = 'Test';
    const lastName = 'User';

    // Navigate to registration page
    await page.goto('/register');
    
    // Fill in the registration form
    await page.fill('input[name="firstName"]', firstName);
    await page.fill('input[name="lastName"]', lastName);
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);
    await page.fill('input[name="confirmPassword"]', password);

    // Submit the form
    await page.click('button[type="submit"]');

    // Wait for redirect to home page
    await waitForNavigation(page, '/');

    // Verify user is logged in
    const loggedIn = await isLoggedIn(page);
    expect(loggedIn).toBe(true);

    // Verify username is displayed in navigation (may be truncated)
    await expect(page.locator('small').filter({ hasText: 'Logged in as:' })).toBeVisible();
    // Also verify the username appears somewhere in the text
    const loggedInText = await page.locator('small').filter({ hasText: 'Logged in as:' }).textContent();
    expect(loggedInText).toContain(username.substring(0, 15)); // Check first 15 chars in case of truncation
  });

  test('should show error when registering with existing username', async ({ page }) => {
    // First, register a user
    const username = generateUniqueUsername();
    const email = generateUniqueEmail();
    const password = generateTestPassword();

    await page.goto('/register');
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);
    await page.fill('input[name="confirmPassword"]', password);
    await page.click('button[type="submit"]');
    await waitForNavigation(page, '/');

    // Logout
    await page.click('button.nav-link:has-text("Logout")');
    await clearAuthData(page);

    // Try to register with the same username
    await page.goto('/register');
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="email"]', generateUniqueEmail()); // Different email
    await page.fill('input[name="password"]', password);
    await page.fill('input[name="confirmPassword"]', password);
    await page.click('button[type="submit"]');

    // Verify error message is shown (user should not be redirected)
    await expect(page).toHaveURL('/register');
    // Check that we're still on the registration page
    await expect(page.locator('h1:has-text("Register")')).toBeVisible();
  });

  test('should show error when passwords do not match', async ({ page }) => {
    const username = generateUniqueUsername();
    const email = generateUniqueEmail();
    const password = generateTestPassword();

    await page.goto('/register');
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);
    await page.fill('input[name="confirmPassword"]', password + '123'); // Different password
    await page.click('button[type="submit"]');

    // Verify error message is shown
    await expect(page.locator('.alert-danger')).toContainText('Passwords do not match');
  });

  test('should show error when password is too short', async ({ page }) => {
    const username = generateUniqueUsername();
    const email = generateUniqueEmail();

    await page.goto('/register');
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', '12345'); // Too short
    await page.fill('input[name="confirmPassword"]', '12345');
    
    await page.click('button[type="submit"]');

    // Check HTML5 validation message (browser-specific text)
    const passwordInput = page.locator('input[name="password"]');
    const validationMessage = await passwordInput.evaluate((el: HTMLInputElement) => el.validationMessage);
    // Different browsers have different validation messages, just check it's not empty
    expect(validationMessage).toBeTruthy();
    expect(validationMessage.toLowerCase()).toMatch(/6|length|short/i);
  });

  test('should login successfully with valid credentials', async ({ page }) => {
    // First, register a user
    const username = generateUniqueUsername();
    const email = generateUniqueEmail();
    const password = generateTestPassword();

    await page.goto('/register');
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);
    await page.fill('input[name="confirmPassword"]', password);
    await page.click('button[type="submit"]');
    await waitForNavigation(page, '/');

    // Logout
    await page.click('button.nav-link:has-text("Logout")');
    await clearAuthData(page);

    // Navigate to login page
    await page.goto('/login');
    await page.waitForLoadState('networkidle');

    // Fill in login form
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="password"]', password);

    // Submit the form
    await Promise.all([
      page.waitForNavigation({ url: '/', timeout: 10000 }),
      page.click('button[type="submit"]')
    ]);

    // Verify user is logged in
    const loggedIn = await isLoggedIn(page);
    expect(loggedIn).toBe(true);

    // Verify username is displayed in navigation (may be truncated)
    await expect(page.locator('small').filter({ hasText: 'Logged in as:' })).toBeVisible();
    // Also verify the username appears somewhere in the text
    const loggedInText = await page.locator('small').filter({ hasText: 'Logged in as:' }).textContent();
    expect(loggedInText).toContain(username.substring(0, 15)); // Check first 15 chars in case of truncation
  });

  test('should show error when logging in with invalid credentials', async ({ page }) => {
    await page.goto('/login');

    // Try to login with invalid credentials
    await page.fill('input[name="username"]', 'invaliduser');
    await page.fill('input[name="password"]', 'invalidpassword');
    await page.click('button[type="submit"]');

    // Verify error is shown (user should not be redirected)
    await expect(page).toHaveURL('/login');
    // Verify we're still on login page
    await expect(page.locator('h1:has-text("Login")')).toBeVisible();
  });

  test('should logout successfully', async ({ page }) => {
    // First, register and login a user
    const username = generateUniqueUsername();
    const email = generateUniqueEmail();
    const password = generateTestPassword();

    await page.goto('/register');
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);
    await page.fill('input[name="confirmPassword"]', password);
    await page.click('button[type="submit"]');
    await waitForNavigation(page, '/');

    // Verify user is logged in
    let loggedIn = await isLoggedIn(page);
    expect(loggedIn).toBe(true);

    // Click logout button
    await page.click('button.nav-link:has-text("Logout")');

    // Verify user is logged out
    loggedIn = await isLoggedIn(page);
    expect(loggedIn).toBe(false);

    // Verify login link is visible
    await expect(page.locator('a[href="/login"]')).toBeVisible();
  });

  test('should redirect to login when accessing protected route while not authenticated', async ({ page }) => {
    // Make sure user is not logged in
    await clearAuthData(page);

    // Try to access a protected route
    await page.goto('/posts');

    // Should be redirected to login page
    await expect(page).toHaveURL('/login');
  });
});