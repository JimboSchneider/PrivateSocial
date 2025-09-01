import { vi } from 'vitest'

interface User {
  id: number
  username: string
  email: string
  firstName?: string
  lastName?: string
}

interface AuthContextType {
  user: User | null
  token: string | null
  login: (username: string, password: string) => Promise<void>
  register: (username: string, email: string, password: string, firstName?: string, lastName?: string) => Promise<void>
  logout: () => void
  isLoading: boolean
}

export const createMockAuthContext = (): AuthContextType => ({
  user: {
    id: 1,
    username: 'testuser',
    email: 'test@example.com'
  },
  token: 'mock-token',
  login: vi.fn().mockResolvedValue(undefined),
  register: vi.fn().mockResolvedValue(undefined),
  logout: vi.fn(),
  isLoading: false
})