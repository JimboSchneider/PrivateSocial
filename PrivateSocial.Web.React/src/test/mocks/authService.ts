import { vi } from 'vitest'
import { AuthService } from '../../services/authService'

export const createMockAuthService = (): jest.Mocked<AuthService> => ({
  login: vi.fn().mockResolvedValue({ 
    token: 'mock-token', 
    user: { 
      id: 1, 
      username: 'testuser', 
      email: 'test@example.com' 
    } 
  }),
  register: vi.fn().mockResolvedValue({ 
    success: true, 
    message: 'Registration successful' 
  }),
  getUserInfo: vi.fn().mockResolvedValue({ 
    id: 1, 
    username: 'testuser', 
    email: 'test@example.com' 
  }),
  logout: vi.fn(),
  setAuthToken: vi.fn(),
  getStoredToken: vi.fn().mockReturnValue('mock-token'),
  removeStoredToken: vi.fn(),
})