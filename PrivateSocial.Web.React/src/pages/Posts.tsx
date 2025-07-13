import React, { useState, useEffect } from 'react';
import { postsService, Post, PagedResult } from '../services/postsService';
import CreatePostForm from '../components/CreatePostForm';
import PostCard from '../components/PostCard';

const Posts: React.FC = () => {
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    loadPosts();
  }, [page, refreshKey]);

  const loadPosts = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const result: PagedResult<Post> = await postsService.getPosts(page, 20);
      setPosts(result.items);
      setTotalPages(result.totalPages);
    } catch (err) {
      setError('Failed to load posts. Please try again.');
      console.error('Error loading posts:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePostCreated = () => {
    setPage(1);
    setRefreshKey(prev => prev + 1);
  };

  const handleDeletePost = async (postId: number) => {
    if (!window.confirm('Are you sure you want to delete this post?')) {
      return;
    }

    try {
      await postsService.deletePost(postId);
      setRefreshKey(prev => prev + 1);
    } catch (err) {
      alert('Failed to delete post. Please try again.');
      console.error('Error deleting post:', err);
    }
  };

  const handleEditPost = (post: Post) => {
    // TODO: Implement edit functionality
    console.log('Edit post:', post);
  };

  return (
    <div className="container mt-4">
      <h1 className="mb-4">Posts</h1>
      
      <CreatePostForm onPostCreated={handlePostCreated} />
      
      {loading && (
        <div className="text-center my-4">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      )}
      
      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}
      
      {!loading && !error && posts.length === 0 && (
        <div className="alert alert-info" role="alert">
          No posts yet. Be the first to create one!
        </div>
      )}
      
      {!loading && !error && posts.length > 0 && (
        <>
          <div className="posts-list">
            {posts.map(post => (
              <PostCard
                key={post.id}
                post={post}
                onEdit={handleEditPost}
                onDelete={handleDeletePost}
              />
            ))}
          </div>
          
          {totalPages > 1 && (
            <nav aria-label="Posts pagination" className="mt-4">
              <ul className="pagination justify-content-center">
                <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
                  <button
                    className="page-link"
                    onClick={() => setPage(page - 1)}
                    disabled={page === 1}
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
                      <li
                        key={pageNumber}
                        className={`page-item ${pageNumber === page ? 'active' : ''}`}
                      >
                        <button
                          className="page-link"
                          onClick={() => setPage(pageNumber)}
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
                      <li key={pageNumber} className="page-item disabled">
                        <span className="page-link">...</span>
                      </li>
                    );
                  }
                  return null;
                })}
                
                <li className={`page-item ${page === totalPages ? 'disabled' : ''}`}>
                  <button
                    className="page-link"
                    onClick={() => setPage(page + 1)}
                    disabled={page === totalPages}
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