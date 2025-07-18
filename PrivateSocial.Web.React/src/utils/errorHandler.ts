/**
 * Extracts error message from various API error response formats
 */
export function extractErrorMessage(error: any): string {
  // Check if error has response data
  if (error.response?.data) {
    const data = error.response.data;
    
    // Check for simple message format: { message: "..." }
    if (data.message) {
      return data.message;
    }
    
    // Check for validation errors format: { errors: { fieldName: ["error1", "error2"] } }
    if (data.errors) {
      // Get the first error from the first field
      const firstField = Object.keys(data.errors)[0];
      if (firstField && Array.isArray(data.errors[firstField]) && data.errors[firstField].length > 0) {
        return data.errors[firstField][0];
      }
    }
    
    // Check for title in problem details format
    if (data.title) {
      return data.title;
    }
    
    // Check if data is a string
    if (typeof data === 'string') {
      return data;
    }
  }
  
  // Check if error has a message property
  if (error.message) {
    return error.message;
  }
  
  // Default error message
  return 'An unexpected error occurred. Please try again.';
}

/**
 * Extracts all validation errors from API response
 */
export function extractValidationErrors(error: any): Record<string, string[]> {
  if (error.response?.data?.errors) {
    return error.response.data.errors;
  }
  return {};
}