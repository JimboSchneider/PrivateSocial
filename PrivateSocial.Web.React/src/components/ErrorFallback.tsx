import React from 'react';
import { FallbackProps } from 'react-error-boundary';

const ErrorFallback: React.FC<FallbackProps> = ({ error, resetErrorBoundary }) => {
  const isDevelopment = import.meta.env.DEV;

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-6">
        <div className="flex items-center mb-4">
          <svg className="w-12 h-12 text-red-500 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Something went wrong</h2>
            <p className="text-sm text-gray-600">An unexpected error has occurred</p>
          </div>
        </div>
        
        {isDevelopment && error && (
          <div className="mb-4 p-3 bg-red-50 rounded-md">
            <h3 className="text-sm font-medium text-red-800 mb-1">Error Details:</h3>
            <pre className="text-xs text-red-700 overflow-x-auto whitespace-pre-wrap">
              {error.message}
              {error.stack && (
                <>
                  {'\n\nStack trace:\n'}
                  {error.stack}
                </>
              )}
            </pre>
          </div>
        )}

        <div className="flex gap-3">
          <button
            onClick={resetErrorBoundary}
            className="flex-1 btn btn-primary"
          >
            Try Again
          </button>
          <button
            onClick={() => window.location.href = '/'}
            className="flex-1 btn btn-secondary"
          >
            Go Home
          </button>
        </div>

        {!isDevelopment && (
          <p className="mt-4 text-xs text-gray-500 text-center">
            If this problem persists, please contact support.
          </p>
        )}
      </div>
    </div>
  );
};

export default ErrorFallback;