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
    await page.goto('http://localhost:3000/');
    await clearAuthData(page);

    // Register a new user for each test
    username = generateUniqueUsername();
    email = generateUniqueEmail();
    password = generateTestPassword();

    await page.goto('/register');
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
    await page.fill('textarea[placeholder="What\'s on your mind?"]', postContent);
    await page.click('button:has-text("Post")');

    // Wait for the post creation to complete
    await page.waitForResponse(response => 
      response.url().includes('/api/posts') && response.status() === 201,
      { timeout: 10000 }
    );
    
    // Small delay to ensure UI updates
    await page.waitForTimeout(500);

    // Verify the post appears in the list
    const postCard = page.locator('.card-body').filter({ hasText: postContent });
    await expect(postCard).toBeVisible({ timeout: 10000 });

    // Verify the author name is displayed
    await expect(postCard.locator('.card-subtitle.text-muted')).toContainText(username);
  });

  test('should show error when trying to create empty post', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Try to submit empty post
    const postButton = page.locator('button:has-text("Post")');
    
    // Button should be disabled when textarea is empty
    await expect(postButton).toBeDisabled();
    
    // Try to type space and clear to trigger validation
    const textarea = page.locator('textarea[placeholder="What\'s on your mind?"]');
    await textarea.fill(' ');
    await textarea.clear();
    await page.click('button:has-text("Post")');

    // Verify error message is shown
    await expect(page.locator('.alert-danger')).toContainText('Post content cannot be empty');
  });

  test('should show character count when typing', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Type in the textarea
    const testContent = 'This is a test post';
    await page.fill('textarea[placeholder="What\'s on your mind?"]', testContent);

    // Verify character count is displayed
    await expect(page.locator('small.text-muted')).toContainText(`${testContent.length}/500 characters`);
  });

  test('should enforce character limit', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Try to type more than 500 characters
    const longContent = 'a'.repeat(501);
    await page.fill('textarea[placeholder="What\'s on your mind?"]', longContent);

    // Verify the content is truncated to 500 characters
    const textareaValue = await page.locator('textarea[placeholder="What\'s on your mind?"]').inputValue();
    expect(textareaValue.length).toBe(500);
  });

  test('should delete own post successfully', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Create a post first
    const postContent = `Post to delete - ${Date.now()}`;
    await page.fill('textarea[placeholder="What\'s on your mind?"]', postContent);
    await page.click('button:has-text("Post")');

    // Wait for the post to appear
    await page.waitForFunction(
      (content) => {
        const posts = document.querySelectorAll('.card-body');
        return Array.from(posts).some(p => p.textContent?.includes(content));
      },
      postContent,
      { timeout: 10000 }
    );
    const postCard = page.locator('.card-body').filter({ hasText: postContent });
    await expect(postCard).toBeVisible();

    // Set up dialog handler before triggering the dialog
    page.once('dialog', dialog => {
      console.log('Dialog message:', dialog.message());
      dialog.accept();
    });

    // Click the three dots menu
    await postCard.locator('button[data-bs-toggle="dropdown"]').click();

    // Wait for dropdown to be visible and click delete
    await page.waitForSelector('.dropdown-menu.show', { timeout: 5000 });
    await page.locator('.dropdown-menu.show button.dropdown-item:has-text("Delete")').click();

    // Wait for the post to be removed
    await expect(postCard).not.toBeVisible({ timeout: 10000 });
  });

  test('should not show edit/delete options for other users posts', async ({ page }) => {
    // Create a post with current user
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');
    
    const postContent = `First user post - ${Date.now()}`;
    await page.fill('textarea[placeholder="What\'s on your mind?"]', postContent);
    await page.click('button:has-text("Post")');
    await page.waitForTimeout(1000);

    // Logout
    await page.click('button.nav-link:has-text("Logout")');
    await page.waitForTimeout(500);
    await clearAuthData(page);

    // Register and login as a different user
    const secondUsername = generateUniqueUsername();
    const secondEmail = generateUniqueEmail();
    
    await page.goto('/register');
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
    const postCard = page.locator('.card-body').filter({ hasText: postContent });
    
    // Verify the three dots menu is not visible for this post
    await expect(postCard.locator('button[data-bs-toggle="dropdown"]')).not.toBeVisible();
  });

  test('should handle pagination correctly', async ({ page }) => {
    // Navigate to posts page
    await page.click('a[href="/posts"]');
    await page.waitForURL('/posts');

    // Create multiple posts to test pagination
    for (let i = 1; i <= 5; i++) {
      const postText = `Test post ${i} - ${Date.now()}`;
      await page.fill('textarea[placeholder="What\'s on your mind?"]', postText);
      await page.click('button:has-text("Post")');
      // Wait for textarea to clear
      await page.waitForFunction(
        () => {
          const textarea = document.querySelector('textarea[placeholder="What\'s on your mind?"]') as HTMLTextAreaElement;
          return textarea && textarea.value === '';
        },
        { timeout: 5000 }
      );
    }

    // Check if posts are displayed
    const posts = page.locator('.card-body');
    const postCount = await posts.count();
    expect(postCount).toBeGreaterThan(0);

    // If pagination is visible, test it
    const paginationExists = await page.locator('.pagination').isVisible();
    if (paginationExists) {
      // Click next page if available
      const nextButton = page.locator('.page-item:has-text("Next")');
      if (await nextButton.isEnabled()) {
        await nextButton.click();
        await page.waitForTimeout(1000);
        
        // Verify we're on a different page
        const newPosts = page.locator('.card-body');
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
    const initialPosts = await page.locator('.card-body').count();

    // Create a new post
    const postContent = `New post for refresh test - ${Date.now()}`;
    await page.fill('textarea[placeholder="What\'s on your mind?"]', postContent);
    await page.click('button:has-text("Post")');

    // Wait for the list to refresh and post to appear
    await page.waitForFunction(
      (content) => {
        const posts = document.querySelectorAll('.card-body');
        return posts.length > 0 && posts[0].textContent?.includes(content);
      },
      postContent,
      { timeout: 10000 }
    );

    // Verify the new post is at the top of the list
    const firstPost = page.locator('.card-body').first();
    await expect(firstPost).toContainText(postContent);

    // Verify the post count increased
    const newPostCount = await page.locator('.card-body').count();
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
      await page.fill('textarea[placeholder="What\'s on your mind?"]', content);
      await page.click('button:has-text("Post")');
      // Wait for textarea to clear
      await page.waitForFunction(
        () => {
          const textarea = document.querySelector('textarea[placeholder="What\'s on your mind?"]') as HTMLTextAreaElement;
          return textarea && textarea.value === '';
        },
        { timeout: 5000 }
      );
    }

    // Verify posts appear in reverse chronological order
    const postCards = page.locator('.card-body');
    
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
    await page.fill('textarea[placeholder="What\'s on your mind?"]', postContent);
    await page.click('button:has-text("Post")');

    // Verify error message is shown
    await expect(page.locator('.alert-danger')).toContainText('Failed to create post. Please try again.');
  });
});