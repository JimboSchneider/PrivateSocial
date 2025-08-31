import React, { useState } from 'react';
import { postsService, CreatePostRequest } from '../services/postsService';
import { extractErrorMessage } from '../utils/errorHandler';

interface CreatePostFormProps {
  onPostCreated: () => void;
}

const CreatePostForm: React.FC<CreatePostFormProps> = ({ onPostCreated }) => {
  const [content, setContent] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!content.trim()) {
      setError('Post content cannot be empty');
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const postData: CreatePostRequest = { content: content.trim() };
      await postsService.createPost(postData);
      setContent('');
      onPostCreated();
    } catch (err: any) {
      setError(extractErrorMessage(err));
      console.error('Error creating post:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="card w-full mb-6 md:mb-8 border-2 border-blue-100 bg-gradient-to-br from-blue-50 to-white">
      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <textarea
            className="form-control resize-none text-base md:text-lg border-0 bg-transparent focus:ring-0 placeholder-gray-400"
            rows={4}
            placeholder="What's on your mind? Share your thoughts..."
            value={content}
            onChange={(e) => setContent(e.target.value)}
            maxLength={500}
            required
            disabled={isSubmitting}
          />
        </div>
        {error && (
          <div className="alert alert-danger mb-3" role="alert">
            {error}
          </div>
        )}
        <div className="flex items-center justify-between">
          <small className="text-xs md:text-sm text-gray-500">
            {content.length}/500 characters
          </small>
          <button
            type="submit"
            className="btn btn-primary px-6 md:px-8"
            disabled={isSubmitting || !content.trim()}
          >
            {isSubmitting ? (
              <span className="flex items-center gap-2">
                <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none"/>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/>
                </svg>
                Posting...
              </span>
            ) : 'Share Post'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default CreatePostForm;