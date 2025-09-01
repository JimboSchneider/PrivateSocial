import React, { useCallback } from 'react';
import { Post } from '../services/postsService';
import { usePosts } from '../hooks/usePosts';
import CreatePostForm from '../components/CreatePostForm';
import PostCard from '../components/PostCard';
import LoadingSpinner from '../components/LoadingSpinner';

/**
 * Posts page component using custom hooks and optimized rendering
 * Implements proper error handling, loading states, and pagination
 */
const Posts: React.FC = () => {
  const {
    posts,
    loading,
    error,
    page,
    totalPages,
    setPage,
    refresh,
    deletePost
  } = usePosts(20);

  /**
   * Handle post creation - refresh the list
   */
  const handlePostCreated = useCallback(() => {
    refresh();
  }, [refresh]);

  /**
   * Handle post deletion with confirmation
   */
  const handleDeletePost = useCallback(async (postId: number) => {
    if (!window.confirm('Are you sure you want to delete this post?')) {
      return;
    }

    const success = await deletePost(postId);
    if (!success) {
      alert('Failed to delete post. Please try again.');
    }
  }, [deletePost]);

  /**
   * Handle post editing (placeholder for future implementation)
   */
  const handleEditPost = useCallback((post: Post) => {
    // TODO: Implement edit functionality
    console.log('Edit post:', post);
  }, []);

  return (
    <div className="w-full">
      <h1 className="text-2xl md:text-3xl font-bold text-gray-900 mb-4 md:mb-6">Posts</h1>
      
      <CreatePostForm onPostCreated={handlePostCreated} />
      
      {loading && (
        <LoadingSpinner size="large" message="Loading posts..." />
      )}
      
      {error && (
        <div className="alert alert-danger" role="alert">
          {error.message}
        </div>
      )}
      
      {!loading && !error && posts.length === 0 && (
        <div className="alert alert-info" role="alert">
          No posts yet. Be the first to create one!
        </div>
      )}
      
      {!loading && !error && posts.length > 0 && (
        <>
          <div className="space-y-4 md:space-y-6">
            {posts.map(post => (
              <PostCard
                key={post.id}
                post={post}
                onEdit={handleEditPost}
                onDelete={handleDeletePost}
              />
            ))}
          </div>
          
          {/* Pagination */}
          {totalPages > 1 && (
            <nav aria-label="Posts pagination" className="mt-6 md:mt-8">
              <ul className="flex justify-center items-center gap-1 md:gap-2">
                <li>
                  <button
                    className={`px-2 md:px-3 py-1.5 md:py-2 rounded-md text-xs md:text-sm font-medium transition-colors ${
                      page === 1 
                        ? 'bg-gray-100 text-gray-400 cursor-not-allowed' 
                        : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-300'
                    }`}
                    onClick={() => setPage(page - 1)}
                    disabled={page === 1}
                    aria-label="Previous page"
                  >
                    Previous
                  </button>
                </li>
                
                {[...Array(totalPages)].map((_, index) => {
                  const pageNumber = index + 1;
                  if (
                    pageNumber === 1 ||
                    pageNumber === totalPages ||
                    (pageNumber >= page - 2 && pageNumber <= page + 2)
                  ) {
                    return (
                      <li key={pageNumber}>
                        <button
                          className={`px-2 md:px-3 py-1.5 md:py-2 rounded-md text-xs md:text-sm font-medium transition-colors ${
                            pageNumber === page
                              ? 'bg-blue-500 text-white cursor-default'
                              : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-300'
                          }`}
                          onClick={() => setPage(pageNumber)}
                          disabled={pageNumber === page}
                          aria-label={`Go to page ${pageNumber}`}
                          aria-current={pageNumber === page ? 'page' : undefined}
                        >
                          {pageNumber}
                        </button>
                      </li>
                    );
                  } else if (
                    pageNumber === page - 3 ||
                    pageNumber === page + 3
                  ) {
                    return (
                      <li key={pageNumber}>
                        <span className="px-2 md:px-3 py-1.5 md:py-2 text-xs md:text-sm text-gray-400">...</span>
                      </li>
                    );
                  }
                  return null;
                })}
                
                <li>
                  <button
                    className={`px-2 md:px-3 py-1.5 md:py-2 rounded-md text-xs md:text-sm font-medium transition-colors ${
                      page === totalPages
                        ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                        : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-300'
                    }`}
                    onClick={() => setPage(page + 1)}
                    disabled={page === totalPages}
                    aria-label="Next page"
                  >
                    Next
                  </button>
                </li>
              </ul>
            </nav>
          )}
        </>
      )}
    </div>
  );
};

export default Posts;