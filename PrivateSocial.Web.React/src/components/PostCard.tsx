import React, { useState } from 'react';
import { Post } from '../services/postsService';
import { useAuth } from '../contexts/AuthContext';

interface PostCardProps {
  post: Post;
  onEdit?: (post: Post) => void;
  onDelete?: (postId: number) => void;
}

const PostCard: React.FC<PostCardProps> = ({ post, onEdit, onDelete }) => {
  const { user } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const isOwner = user?.id === post.authorId;

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString();
  };

  return (
    <div className="card w-full">
      <div className="flex justify-between items-start mb-4">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <div className="w-8 h-8 md:w-10 md:h-10 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white font-semibold text-sm md:text-base">
              {post.authorName.charAt(0).toUpperCase()}
            </div>
            <div>
              <h6 className="text-sm md:text-base font-semibold text-gray-900">
                {post.authorName}
              </h6>
              <small className="text-xs md:text-sm text-gray-500">
                {formatDate(post.createdAt)}
                {post.updatedAt && post.updatedAt !== post.createdAt && (
                  <span className="italic"> (edited)</span>
                )}
              </small>
            </div>
          </div>
        </div>
        {isOwner && (
          <div className="relative">
            <button
              className="p-1 md:p-2 text-gray-400 hover:text-gray-600 rounded-md hover:bg-gray-100 transition-colors"
              aria-label="Post options"
              onClick={() => setMenuOpen(!menuOpen)}
            >
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path d="M10 6a2 2 0 110-4 2 2 0 010 4zM10 12a2 2 0 110-4 2 2 0 010 4zM10 18a2 2 0 110-4 2 2 0 010 4z" />
              </svg>
            </button>
            {menuOpen && (
              <>
                <div 
                  className="fixed inset-0 z-10" 
                  onClick={() => setMenuOpen(false)}
                />
                <div className="absolute right-0 mt-1 z-20 w-32 md:w-40 bg-white rounded-md shadow-lg border border-gray-200">
                  {onEdit && (
                    <button
                      className="w-full text-left px-3 py-2 text-sm md:text-base hover:bg-gray-100 flex items-center gap-2"
                      onClick={() => {
                        onEdit(post);
                        setMenuOpen(false);
                      }}
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                      </svg>
                      Edit
                    </button>
                  )}
                  {onDelete && (
                    <button
                      className="w-full text-left px-3 py-2 text-sm md:text-base hover:bg-red-50 text-red-600 flex items-center gap-2"
                      onClick={() => {
                        onDelete(post.id);
                        setMenuOpen(false);
                      }}
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                      Delete
                    </button>
                  )}
                </div>
              </>
            )}
          </div>
        )}
      </div>
      <div className="pl-0 md:pl-12">
        <p className="text-sm md:text-base lg:text-lg text-gray-800 whitespace-pre-wrap leading-relaxed">
          {post.content}
        </p>
      </div>
    </div>
  );
};

export default PostCard;