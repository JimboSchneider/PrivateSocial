export const TEST_CONFIG = {
  // API Configuration
  apiUrl: process.env.API_URL || 'http://localhost:5475',
  
  // Frontend Configuration
  frontendUrl: process.env.FRONTEND_URL || 'http://localhost:3000',
  
  // Test User Configuration
  testUserPrefix: 'e2e_test_',
  
  // Timeouts
  defaultTimeout: 30000,
  navigationTimeout: 10000,
  apiTimeout: 10000,
  
  // Retry Configuration
  maxRetries: process.env.CI ? 2 : 0,
  
  // Service URLs
  services: {
    apiService: process.env.services__apiservice__http__0 || 'http://localhost:5475',
    apiServiceHttps: process.env.services__apiservice__https__0 || 'https://localhost:7506',
  }
};