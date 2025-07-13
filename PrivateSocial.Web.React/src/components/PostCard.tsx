import React from 'react';
import { Post } from '../services/postsService';
import { useAuth } from '../contexts/AuthContext';

interface PostCardProps {
  post: Post;
  onEdit?: (post: Post) => void;
  onDelete?: (postId: number) => void;
}

const PostCard: React.FC<PostCardProps> = ({ post, onEdit, onDelete }) => {
  const { user } = useAuth();
  const isOwner = user?.id === post.authorId;

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString();
  };

  return (
    <div className="card mb-3">
      <div className="card-body">
        <div className="d-flex justify-content-between align-items-start mb-2">
          <h6 className="card-subtitle text-muted">
            {post.authorName}
          </h6>
          {isOwner && (
            <div className="dropdown">
              <button
                className="btn btn-sm btn-link text-muted"
                type="button"
                data-bs-toggle="dropdown"
                aria-expanded="false"
              >
                <i className="bi bi-three-dots"></i>
              </button>
              <ul className="dropdown-menu">
                {onEdit && (
                  <li>
                    <button
                      className="dropdown-item"
                      onClick={() => onEdit(post)}
                    >
                      <i className="bi bi-pencil me-2"></i>
                      Edit
                    </button>
                  </li>
                )}
                {onDelete && (
                  <li>
                    <button
                      className="dropdown-item text-danger"
                      onClick={() => onDelete(post.id)}
                    >
                      <i className="bi bi-trash me-2"></i>
                      Delete
                    </button>
                  </li>
                )}
              </ul>
            </div>
          )}
        </div>
        <p className="card-text" style={{ whiteSpace: 'pre-wrap' }}>
          {post.content}
        </p>
        <small className="text-muted">
          {formatDate(post.createdAt)}
          {post.updatedAt && post.updatedAt !== post.createdAt && (
            <span> (edited)</span>
          )}
        </small>
      </div>
    </div>
  );
};

export default PostCard;