import { useState, useCallback } from 'react';
import { AxiosError } from 'axios';

/**
 * API call state with proper TypeScript typing
 */
export interface ApiState<T> {
  data: T | null;
  loading: boolean;
  error: Error | null;
}

/**
 * Custom hook return type for API calls
 */
export interface UseApiReturn<T> {
  data: T | null;
  loading: boolean;
  error: Error | null;
  execute: (...args: unknown[]) => Promise<T | null>;
  reset: () => void;
}

/**
 * Error response structure from API
 */
interface ApiErrorResponse {
  message?: string;
  errors?: Record<string, string[]>;
}

/**
 * Parse error from Axios error or generic error
 */
const parseError = (error: unknown): Error => {
  if (error instanceof AxiosError) {
    const response = error.response?.data as ApiErrorResponse;
    const message = response?.message || error.message || 'An unexpected error occurred';
    return new Error(message);
  }
  
  if (error instanceof Error) {
    return error;
  }
  
  return new Error('An unexpected error occurred');
};

/**
 * Generic custom hook for API calls with proper error handling and loading states
 * @template T - The expected response type
 * @param apiFunction - The API function to execute
 * @returns Object containing data, loading, error states and execute function
 */
export const useApi = <T = unknown>(
  apiFunction: (...args: unknown[]) => Promise<T>
): UseApiReturn<T> => {
  const [state, setState] = useState<ApiState<T>>({
    data: null,
    loading: false,
    error: null
  });

  /**
   * Execute the API call with proper error handling
   */
  const execute = useCallback(async (...args: unknown[]): Promise<T | null> => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      const result = await apiFunction(...args);
      setState({ data: result, loading: false, error: null });
      return result;
    } catch (err) {
      const error = parseError(err);
      setState(prev => ({ ...prev, loading: false, error }));
      return null;
    }
  }, [apiFunction]);

  /**
   * Reset the state
   */
  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null });
  }, []);

  return {
    data: state.data,
    loading: state.loading,
    error: state.error,
    execute,
    reset
  };
};