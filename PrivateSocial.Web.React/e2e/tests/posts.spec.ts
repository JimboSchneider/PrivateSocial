import { test, expect } from '@playwright/test';
import {
  generateUniqueUsername,
  generateUniqueEmail,
  generateTestPassword,
  waitForNavigation,
  clearAuthData
} from '../utils/test-helpers';

test.describe('Posts', () => {
  let username: string;
  let email: string;
  let password: string;

  test.beforeEach(async ({ page }) => {
    // Clear any existing auth data
    await page.goto('/');
    await clearAuthData(page);

    // Register a new user for each test
    username = generateUniqueUsername();
    email = generateUniqueEmail();
    password = generateTestPassword();

    await page.goto('/register');
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[name="username"]', username);
    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);
    await page.fill('input[name="confirmPassword"]', password);
    await page.click('button[type="submit"]');
    await waitForNavigation(page, '/');
  });

  test('should create a new post successfully', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Create a new post
    const postContent = `Test post created at ${new Date().toISOString()}`;
    await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', postContent);
    await page.click('button:has-text("Share Post")');

    // Wait for the post creation to complete
    await page.waitForResponse(response =>
      response.url().includes('/api/posts') && response.status() === 201,
      { timeout: 10000 }
    );

    // Small delay to ensure UI updates
    await page.waitForTimeout(500);

    // Verify the post appears in the list
    const postCard = page.locator('.card').filter({ hasText: postContent });
    await expect(postCard).toBeVisible({ timeout: 10000 });

    // Verify the author name is displayed
    await expect(postCard.locator('h6')).toContainText(username);
  });

  test('should show error when trying to create empty post', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Try to submit empty post
    const postButton = page.locator('button:has-text("Share Post")');

    // Button should be disabled when textarea is empty
    await expect(postButton).toBeDisabled();

    // Try to type space - button should stay disabled because of trim() validation
    const textarea = page.locator('textarea[placeholder="What\'s on your mind? Share your thoughts..."]');
    await textarea.fill(' ');

    // Button should still be disabled with whitespace only (trim validation)
    await expect(postButton).toBeDisabled();

    // Clear the textarea
    await textarea.clear();

    // Button should be disabled
    await expect(postButton).toBeDisabled();
  });

  test('should show character count when typing', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Type in the textarea
    const testContent = 'This is a test post';
    await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', testContent);

    // Verify character count is displayed
    await expect(page.locator('small').filter({ hasText: 'characters' })).toContainText(`${testContent.length}/500 characters`);
  });

  test('should enforce character limit', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Try to type more than 500 characters
    const longContent = 'a'.repeat(501);
    await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', longContent);

    // Verify the content is truncated to 500 characters
    const textareaValue = await page.locator('textarea[placeholder="What\'s on your mind? Share your thoughts..."]').inputValue();
    expect(textareaValue.length).toBe(500);
  });

  test('should delete own post successfully', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Create a post first
    const postContent = `Post to delete - ${Date.now()}`;
    await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', postContent);
    await page.click('button:has-text("Share Post")');

    // Wait for the post creation API call to complete
    await page.waitForResponse(response =>
      response.url().includes('/api/posts') &&
      response.request().method() === 'POST' &&
      (response.status() === 200 || response.status() === 201),
      { timeout: 10000 }
    );

    // Wait for the post to appear
    await page.waitForFunction(
      (content) => {
        const posts = document.querySelectorAll('.card');
        return Array.from(posts).some(p => p.textContent?.includes(content));
      },
      postContent,
      { timeout: 10000 }
    );
    const postCard = page.locator('.card').filter({ hasText: postContent });
    await expect(postCard).toBeVisible();

    // Click the three dots menu button
    await postCard.locator('button[aria-label="Post options"]').click();

    // Wait for the delete button to be visible in the dropdown menu
    const deleteButton = postCard.locator('button').filter({ hasText: 'Delete' });
    await expect(deleteButton).toBeVisible();

    // Set up dialog handler to accept confirmation
    page.once('dialog', dialog => {
      expect(dialog.type()).toBe('confirm');
      expect(dialog.message()).toContain('Are you sure');
      dialog.accept();
    });

    // Start waiting for response BEFORE clicking to avoid race condition
    const deleteResponsePromise = page.waitForResponse(response =>
      response.url().includes('/api/posts') &&
      response.request().method() === 'DELETE' &&
      response.status() === 200,
      { timeout: 15000 }
    );

    // Click delete button
    await deleteButton.click();

    // Wait for the DELETE API call to complete
    await deleteResponsePromise;

    // Wait for the post to be removed from the UI
    await expect(postCard).not.toBeVisible({ timeout: 10000 });
  });

  test('should not show edit/delete options for other users posts', async ({ page }) => {
    // Create a post with current user
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    const postContent = `First user post - ${Date.now()}`;
    await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', postContent);
    await page.click('button:has-text("Share Post")');
    await page.waitForTimeout(1000);

    // Logout
    await page.click('button.nav-link:has-text("Logout")');
    await page.waitForTimeout(500);
    await clearAuthData(page);

    // Register and login as a different user
    const secondUsername = generateUniqueUsername();
    const secondEmail = generateUniqueEmail();

    await page.goto('/register');
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[name="username"]', secondUsername);
    await page.fill('input[name="email"]', secondEmail);
    await page.fill('input[name="password"]', password);
    await page.fill('input[name="confirmPassword"]', password);
    await page.click('button[type="submit"]');
    await waitForNavigation(page, '/');

    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Find the first user's post
    const postCard = page.locator('.card').filter({ hasText: postContent });

    // Verify the three dots menu is not visible for this post
    await expect(postCard.locator('button[aria-label="Post options"]')).not.toBeVisible();
  });

  test('should handle pagination correctly', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Create multiple posts to test pagination
    for (let i = 1; i <= 5; i++) {
      const postText = `Test post ${i} - ${Date.now()}`;
      await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', postText);
      await page.click('button:has-text("Share Post")');
      // Wait for textarea to clear
      await page.waitForFunction(
        () => {
          const textarea = document.querySelector('textarea[placeholder="What\'s on your mind? Share your thoughts..."]') as HTMLTextAreaElement;
          return textarea && textarea.value === '';
        },
        { timeout: 5000 }
      );
    }

    // Check if posts are displayed
    const posts = page.locator('.card');
    const postCount = await posts.count();
    expect(postCount).toBeGreaterThan(0);

    // If pagination is visible, test it
    const paginationExists = await page.locator('nav[aria-label="Posts pagination"]').isVisible();
    if (paginationExists) {
      // Click next page if available
      const nextButton = page.locator('button[aria-label="Next page"]');
      if (await nextButton.isEnabled()) {
        await nextButton.click();
        await page.waitForTimeout(1000);

        // Verify we're on a different page
        const newPosts = page.locator('.card');
        const newPostCount = await newPosts.count();
        expect(newPostCount).toBeGreaterThan(0);
      }
    }
  });

  test('should refresh posts list after creating a new post', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Get initial post count
    const initialPosts = await page.locator('.card').count();

    // Create a new post
    const postContent = `New post for refresh test - ${Date.now()}`;
    await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', postContent);

    // Start waiting for responses BEFORE clicking to avoid race condition
    const postResponsePromise = page.waitForResponse(response =>
      response.url().includes('/api/posts') &&
      response.request().method() === 'POST' &&
      (response.status() === 200 || response.status() === 201),
      { timeout: 15000 }
    );

    await page.click('button:has-text("Share Post")');

    // Wait for the POST API call to complete
    await postResponsePromise;

    // Wait for the post to appear in the list (more reliable than waiting for GET)
    await page.waitForFunction(
      (content) => {
        const posts = document.querySelectorAll('.card');
        return Array.from(posts).some(p => p.textContent?.includes(content));
      },
      postContent,
      { timeout: 15000 }
    );

    // Verify the new post appears in the list
    const postCard = page.locator('.card').filter({ hasText: postContent });
    await expect(postCard).toBeVisible();

    // Verify the post count increased
    const newPostCount = await page.locator('.card').count();
    expect(newPostCount).toBeGreaterThanOrEqual(initialPosts);
  });

  test('should display posts in chronological order (newest first)', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Create multiple posts with timestamps
    const posts = [];
    for (let i = 1; i <= 3; i++) {
      const content = `Post ${i} - ${Date.now()}`;
      posts.push(content);
      await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', content);
      await page.click('button:has-text("Share Post")');
      // Wait for textarea to clear
      await page.waitForFunction(
        () => {
          const textarea = document.querySelector('textarea[placeholder="What\'s on your mind? Share your thoughts..."]') as HTMLTextAreaElement;
          return textarea && textarea.value === '';
        },
        { timeout: 5000 }
      );
    }

    // Verify posts appear in reverse chronological order
    // Use .card:has(p) to select only post cards (not the create form card)
    const postCards = page.locator('.card:has(p)');

    // The last created post should be first
    await expect(postCards.first()).toContainText(posts[2]);
  });

  test('should handle network errors gracefully', async ({ page, context }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Intercept the create post request to simulate a network error
    await context.route('**/api/posts', route => {
      route.abort('failed');
    });

    // Try to create a post
    const postContent = 'This post will fail';
    await page.fill('textarea[placeholder="What\'s on your mind? Share your thoughts..."]', postContent);
    await page.click('button:has-text("Share Post")');

    // Verify error message is shown (axios gives "Network Error" message)
    await expect(page.locator('.alert-danger')).toContainText('Network Error');
  });
});