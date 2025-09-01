import { useState, useEffect } from 'react';

/**
 * Custom hook to debounce a value
 * @template T - The type of the value being debounced
 * @param value - The value to debounce
 * @param delay - The delay in milliseconds
 * @returns The debounced value
 * 
 * @example
 * const debouncedSearchTerm = useDebounce(searchTerm, 500);
 */
export const useDebounce = <T>(value: T, delay: number): T => {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    
    // Cleanup function to cancel the timeout if value changes
    return () => clearTimeout(handler);
  }, [value, delay]);
  
  return debouncedValue;
};