import { useState, useEffect, useCallback } from 'react';
import { postsService, Post, PagedResult } from '../services/postsService';

/**
 * Custom hook return type for posts data fetching
 */
export interface UsePostsReturn {
  posts: Post[];
  loading: boolean;
  error: Error | null;
  page: number;
  totalPages: number;
  setPage: (page: number) => void;
  refresh: () => void;
  deletePost: (postId: number) => Promise<boolean>;
}

/**
 * Custom hook for managing posts data fetching and operations
 * Implements proper loading states, error handling, and pagination
 */
export const usePosts = (pageSize: number = 20): UsePostsReturn => {
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [refreshKey, setRefreshKey] = useState(0);

  /**
   * Load posts from the API
   */
  const loadPosts = useCallback(async () => {
    setLoading(true);
    setError(null);
    
    try {
      const result: PagedResult<Post> = await postsService.getPosts(page, pageSize);
      setPosts(result.items);
      setTotalPages(result.totalPages);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to load posts');
      setError(error);
      console.error('Error loading posts:', err);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, refreshKey]);

  /**
   * Delete a post with optimistic UI update
   */
  const deletePost = useCallback(async (postId: number): Promise<boolean> => {
    try {
      // Optimistic update
      setPosts(prevPosts => prevPosts.filter(post => post.id !== postId));
      
      await postsService.deletePost(postId);
      
      // Refresh to get updated list
      setRefreshKey(prev => prev + 1);
      return true;
    } catch (err) {
      // Revert optimistic update on error
      setRefreshKey(prev => prev + 1);
      const error = err instanceof Error ? err : new Error('Failed to delete post');
      setError(error);
      console.error('Error deleting post:', err);
      return false;
    }
  }, []);

  /**
   * Refresh posts list
   */
  const refresh = useCallback(() => {
    setPage(1);
    setRefreshKey(prev => prev + 1);
  }, []);

  // Load posts when dependencies change
  useEffect(() => {
    loadPosts();
  }, [loadPosts]);

  return {
    posts,
    loading,
    error,
    page,
    totalPages,
    setPage,
    refresh,
    deletePost
  };
};