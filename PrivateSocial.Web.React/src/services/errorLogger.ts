interface ErrorInfo {
  message: string;
  stack?: string;
  componentStack?: string | null;
  timestamp: Date;
  userAgent: string;
  url: string;
}

class ErrorLogger {
  private errors: ErrorInfo[] = [];
  private maxErrors = 50;

  logError(error: Error, errorInfo?: React.ErrorInfo): void {
    const errorData: ErrorInfo = {
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo?.componentStack,
      timestamp: new Date(),
      userAgent: navigator.userAgent,
      url: window.location.href,
    };

    // Store locally
    this.errors.push(errorData);
    if (this.errors.length > this.maxErrors) {
      this.errors.shift();
    }

    // Log to console in development
    if (import.meta.env.DEV) {
      console.error('Error logged:', errorData);
    }

    // In production, you could send to a logging service
    if (import.meta.env.PROD) {
      this.sendToLoggingService(errorData);
    }
  }

  private sendToLoggingService(error: ErrorInfo): void {
    // TODO: Implement sending to actual logging service
    // Example: Sentry, LogRocket, etc.
    console.log('Would send to logging service:', error);
  }

  getRecentErrors(): ErrorInfo[] {
    return [...this.errors];
  }

  clearErrors(): void {
    this.errors = [];
  }
}

export const errorLogger = new ErrorLogger();