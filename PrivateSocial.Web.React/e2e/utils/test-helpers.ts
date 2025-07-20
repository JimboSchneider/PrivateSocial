import { Page } from '@playwright/test';
import { TEST_CONFIG } from '../test-config';

/**
 * Generate a unique username for testing
 */
export function generateUniqueUsername(): string {
  const timestamp = Date.now();
  const random = Math.floor(Math.random() * 1000);
  return `${TEST_CONFIG.testUserPrefix}${timestamp}_${random}`;
}

/**
 * Generate a unique email for testing
 */
export function generateUniqueEmail(): string {
  const username = generateUniqueUsername();
  return `${username}@test.com`;
}

/**
 * Generate a test password
 */
export function generateTestPassword(): string {
  return 'TestPassword123!';
}

/**
 * Wait for navigation to complete
 */
export async function waitForNavigation(page: Page, url: string) {
  await page.waitForURL(url, { waitUntil: 'networkidle' });
}

/**
 * Check if user is logged in by checking for auth token
 */
export async function isLoggedIn(page: Page): Promise<boolean> {
  const token = await page.evaluate(() => {
    return localStorage.getItem('token');
  });
  return !!token;
}

/**
 * Clear auth data (logout)
 */
export async function clearAuthData(page: Page) {
  await page.evaluate(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  });
}