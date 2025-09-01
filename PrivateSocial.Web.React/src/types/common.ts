/**
 * Common type definitions used across the application
 */

/**
 * Async operation state
 */
export interface AsyncState<T = unknown> {
  data: T | null;
  loading: boolean;
  error: Error | null;
}

/**
 * Pagination parameters
 */
export interface PaginationParams {
  page: number;
  pageSize: number;
}

/**
 * Paginated response wrapper
 */
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Form field state
 */
export interface FormField<T = string> {
  value: T;
  error?: string;
  touched: boolean;
}

/**
 * API Error response
 */
export interface ApiError {
  message: string;
  statusCode?: number;
  errors?: Record<string, string[]>;
}

/**
 * Loading states for different operations
 */
export type LoadingState = 'idle' | 'loading' | 'success' | 'error';

/**
 * Generic result type for operations that can succeed or fail
 */
export type Result<T, E = Error> = 
  | { success: true; data: T }
  | { success: false; error: E };

/**
 * Nullable type helper
 */
export type Nullable<T> = T | null;

/**
 * Optional type helper
 */
export type Optional<T> = T | undefined;

/**
 * Deep partial type helper for nested objects
 */
export type DeepPartial<T> = {
  [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P];
};