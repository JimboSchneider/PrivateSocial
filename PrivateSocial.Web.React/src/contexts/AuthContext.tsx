import { createContext, useState, useContext, useEffect, ReactNode } from 'react'
import axios from 'axios'

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

const AuthContext = createContext<AuthContextType | undefined>(undefined)

// Configure axios defaults
axios.defaults.baseURL = '/api'

// Add token to requests if available
axios.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [token, setToken] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    // Check for existing token on mount
    const storedToken = localStorage.getItem('token')
    if (storedToken) {
      setToken(storedToken)
      // Verify token and get user info
      fetchCurrentUser()
    } else {
      setIsLoading(false)
    }
  }, [])

  const fetchCurrentUser = async () => {
    try {
      const response = await axios.get('/auth/me')
      setUser(response.data)
    } catch (error) {
      // Token is invalid, clear it
      localStorage.removeItem('token')
      setToken(null)
    } finally {
      setIsLoading(false)
    }
  }

  const login = async (username: string, password: string) => {
    const response = await axios.post('/auth/login', { username, password })
    const { token: newToken } = response.data
    
    localStorage.setItem('token', newToken)
    setToken(newToken)
    
    // Fetch full user data
    await fetchCurrentUser()
  }

  const register = async (
    username: string, 
    email: string, 
    password: string, 
    firstName?: string, 
    lastName?: string
  ) => {
    const response = await axios.post('/auth/register', {
      username,
      email,
      password,
      firstName,
      lastName
    })
    const { token: newToken } = response.data
    
    localStorage.setItem('token', newToken)
    setToken(newToken)
    
    // Fetch full user data
    await fetchCurrentUser()
  }

  const logout = () => {
    localStorage.removeItem('token')
    setToken(null)
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, token, login, register, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}