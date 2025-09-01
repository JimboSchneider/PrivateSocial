import React, { ReactNode } from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import ErrorFallback from './ErrorFallback';
import { errorLogger } from '../services/errorLogger';

interface AppErrorBoundaryProps {
  children: ReactNode;
  fallback?: React.ComponentType<any>;
  onReset?: () => void;
}

const AppErrorBoundary: React.FC<AppErrorBoundaryProps> = ({ 
  children, 
  fallback = ErrorFallback,
  onReset
}) => {
  return (
    <ErrorBoundary
      FallbackComponent={fallback}
      onError={(error, errorInfo) => {
        errorLogger.logError(error, errorInfo);
      }}
      onReset={() => {
        // Clear any error state
        if (onReset) {
          onReset();
        }
        // Optionally navigate to home
        window.location.href = '/';
      }}
    >
      {children}
    </ErrorBoundary>
  );
};

export default AppErrorBoundary;