import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { renderHook, act, waitFor } from '@testing-library/react'
import axios from 'axios'
import { AuthProvider, useAuth } from './AuthContext'

// Mock axios
vi.mock('axios')

describe('AuthContext', () => {
  const mockAxios = axios as jest.Mocked<typeof axios>

  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    
    // Setup axios defaults and interceptors mocks
    mockAxios.defaults = { baseURL: '/api' } as any
    mockAxios.interceptors = {
      request: {
        use: vi.fn(),
      },
    } as any
  })

  afterEach(() => {
    localStorage.clear()
  })

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <AuthProvider>{children}</AuthProvider>
  )

  it('throws error when useAuth is used outside AuthProvider', () => {
    expect(() => {
      renderHook(() => useAuth())
    }).toThrow('useAuth must be used within an AuthProvider')
  })

  it('initializes with null user and loading state', async () => {
    const { result } = renderHook(() => useAuth(), { wrapper })

    expect(result.current.user).toBeNull()
    expect(result.current.token).toBeNull()
    
    // Wait for loading to complete
    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })
  })

  it('loads user from stored token on mount', async () => {
    const mockToken = 'existing-token'
    const mockUser = { id: 1, username: 'testuser', email: 'test@example.com' }
    
    localStorage.setItem('token', mockToken)
    mockAxios.get.mockResolvedValueOnce({ data: mockUser })

    const { result } = renderHook(() => useAuth(), { wrapper })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
      expect(result.current.user).toEqual(mockUser)
      expect(result.current.token).toBe(mockToken)
      expect(mockAxios.get).toHaveBeenCalledWith('/auth/me')
    })
  })

  it('clears invalid token on failed user fetch', async () => {
    const mockToken = 'invalid-token'
    localStorage.setItem('token', mockToken)
    mockAxios.get.mockRejectedValueOnce(new Error('Unauthorized'))

    const { result } = renderHook(() => useAuth(), { wrapper })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
      expect(result.current.user).toBeNull()
      expect(result.current.token).toBeNull()
      expect(localStorage.getItem('token')).toBeNull()
    })
  })

  it('handles successful login', async () => {
    const mockToken = 'new-token'
    const mockUser = { id: 1, username: 'testuser', email: 'test@example.com' }
    
    mockAxios.post.mockResolvedValueOnce({ data: { token: mockToken } })
    mockAxios.get.mockResolvedValueOnce({ data: mockUser })

    const { result } = renderHook(() => useAuth(), { wrapper })

    await act(async () => {
      await result.current.login('testuser', 'password123')
    })

    expect(mockAxios.post).toHaveBeenCalledWith('/auth/login', {
      username: 'testuser',
      password: 'password123'
    })
    expect(localStorage.getItem('token')).toBe(mockToken)
    expect(result.current.token).toBe(mockToken)
    expect(result.current.user).toEqual(mockUser)
  })

  it('handles successful registration', async () => {
    const mockToken = 'new-token'
    const mockUser = { 
      id: 1, 
      username: 'newuser', 
      email: 'new@example.com',
      firstName: 'John',
      lastName: 'Doe'
    }
    
    mockAxios.post.mockResolvedValueOnce({ data: { token: mockToken } })
    mockAxios.get.mockResolvedValueOnce({ data: mockUser })

    const { result } = renderHook(() => useAuth(), { wrapper })

    await act(async () => {
      await result.current.register(
        'newuser',
        'new@example.com',
        'password123',
        'John',
        'Doe'
      )
    })

    expect(mockAxios.post).toHaveBeenCalledWith('/auth/register', {
      username: 'newuser',
      email: 'new@example.com',
      password: 'password123',
      firstName: 'John',
      lastName: 'Doe'
    })
    expect(localStorage.getItem('token')).toBe(mockToken)
    expect(result.current.token).toBe(mockToken)
    expect(result.current.user).toEqual(mockUser)
  })

  it('handles logout correctly', async () => {
    const mockToken = 'existing-token'
    const mockUser = { id: 1, username: 'testuser', email: 'test@example.com' }
    
    mockAxios.post.mockResolvedValueOnce({ data: { token: mockToken } })
    mockAxios.get.mockResolvedValueOnce({ data: mockUser })

    const { result } = renderHook(() => useAuth(), { wrapper })

    // Login first
    await act(async () => {
      await result.current.login('testuser', 'password')
    })

    expect(result.current.user).toEqual(mockUser)
    expect(result.current.token).toBe(mockToken)

    act(() => {
      result.current.logout()
    })

    expect(localStorage.getItem('token')).toBeNull()
    expect(result.current.token).toBeNull()
    expect(result.current.user).toBeNull()
  })

  it('configures axios interceptor to add auth header', async () => {
    // Clear any previous mock calls
    vi.clearAllMocks()
    
    // Re-import the module to trigger the interceptor setup
    vi.resetModules()
    const axiosMock = vi.mocked(axios)
    axiosMock.defaults = { baseURL: '/api' } as any
    axiosMock.interceptors = {
      request: {
        use: vi.fn(),
      },
    } as any
    
    // Dynamic import to trigger module initialization
    await import('./AuthContext')
    
    const interceptorUse = axiosMock.interceptors.request.use as jest.Mock
    expect(interceptorUse).toHaveBeenCalled()
    
    // Get the request interceptor function
    const requestInterceptor = interceptorUse.mock.calls[0][0]
    
    // Test with token
    localStorage.setItem('token', 'test-token')
    const config = { headers: {} }
    const result = requestInterceptor(config)
    
    expect(result.headers.Authorization).toBe('Bearer test-token')
    
    // Test without token
    localStorage.removeItem('token')
    const configNoToken = { headers: {} }
    const resultNoToken = requestInterceptor(configNoToken)
    
    expect(resultNoToken.headers.Authorization).toBeUndefined()
  })

  it('handles request interceptor error', async () => {
    // Clear any previous mock calls
    vi.clearAllMocks()
    
    // Re-import the module to trigger the interceptor setup
    vi.resetModules()
    const axiosMock = vi.mocked(axios)
    axiosMock.defaults = { baseURL: '/api' } as any
    axiosMock.interceptors = {
      request: {
        use: vi.fn(),
      },
    } as any
    
    await import('./AuthContext')
    
    const interceptorUse = axiosMock.interceptors.request.use as jest.Mock
    const errorHandler = interceptorUse.mock.calls[0][1]
    const error = new Error('Request error')
    
    await expect(errorHandler(error)).rejects.toThrow('Request error')
  })
})