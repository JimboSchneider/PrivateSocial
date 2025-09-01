import React from 'react';

interface LoadingSpinnerProps {
  size?: 'small' | 'medium' | 'large';
  fullScreen?: boolean;
  message?: string;
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({ 
  size = 'medium', 
  fullScreen = false,
  message 
}) => {
  const sizeClasses = {
    small: 'h-4 w-4 border-2',
    medium: 'h-8 w-8 border-2',
    large: 'h-12 w-12 border-3'
  };

  const spinner = (
    <>
      <div className={`animate-spin rounded-full border-b-transparent border-blue-500 ${sizeClasses[size]}`}></div>
      {message && (
        <p className="mt-3 text-sm text-gray-600">{message}</p>
      )}
    </>
  );

  if (fullScreen) {
    return (
      <div className="fixed inset-0 bg-white bg-opacity-75 z-50 flex flex-col items-center justify-center">
        {spinner}
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center justify-center p-4">
      {spinner}
    </div>
  );
};

export default LoadingSpinner;