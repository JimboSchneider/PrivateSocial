import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Login from './Login'

// Mock dependencies
vi.mock('../contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}))

vi.mock('react-router-dom', () => ({
  useNavigate: vi.fn(),
  Link: ({ children, to, ...props }: any) => <a href={to} {...props}>{children}</a>,
}))

import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'

describe('Login Component', () => {
  const mockLogin = vi.fn()
  const mockNavigate = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    ;(useAuth as jest.Mock).mockReturnValue({
      login: mockLogin,
    })
    ;(useNavigate as jest.Mock).mockReturnValue(mockNavigate)
  })

  it('renders login form correctly', () => {
    render(<Login />)

    expect(screen.getByRole('heading', { name: /login/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument()
    expect(screen.getByText(/don't have an account\?/i)).toBeInTheDocument()
  })

  it('handles successful login', async () => {
    const user = userEvent.setup()
    mockLogin.mockResolvedValueOnce(undefined)

    render(<Login />)

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const loginButton = screen.getByRole('button', { name: /login/i })

    await user.type(usernameInput, 'testuser')
    await user.type(passwordInput, 'password123')
    await user.click(loginButton)

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith('testuser', 'password123')
      expect(mockNavigate).toHaveBeenCalledWith('/')
    })
  })

  it('displays error message on failed login', async () => {
    const user = userEvent.setup()
    const errorMessage = 'Invalid username or password'
    mockLogin.mockRejectedValueOnce(new Error(errorMessage))

    render(<Login />)

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const loginButton = screen.getByRole('button', { name: /login/i })

    await user.type(usernameInput, 'testuser')
    await user.type(passwordInput, 'wrongpassword')
    await user.click(loginButton)

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(errorMessage)
    })
  })

  it('disables form during submission', async () => {
    const user = userEvent.setup()
    mockLogin.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)))

    render(<Login />)

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const loginButton = screen.getByRole('button', { name: /login/i })

    await user.type(usernameInput, 'testuser')
    await user.type(passwordInput, 'password123')
    await user.click(loginButton)

    expect(usernameInput).toBeDisabled()
    expect(passwordInput).toBeDisabled()
    expect(loginButton).toBeDisabled()
    expect(loginButton).toHaveTextContent('Logging in...')

    await waitFor(() => {
      expect(usernameInput).not.toBeDisabled()
      expect(passwordInput).not.toBeDisabled()
      expect(loginButton).not.toBeDisabled()
      expect(loginButton).toHaveTextContent('Login')
    })
  })

  it('validates required fields', async () => {
    const user = userEvent.setup()
    render(<Login />)

    const loginButton = screen.getByRole('button', { name: /login/i })
    await user.click(loginButton)

    expect(mockLogin).not.toHaveBeenCalled()
  })

  it('navigates to register page when clicking register link', async () => {
    render(<Login />)

    const registerLink = screen.getByRole('link', { name: /register here/i })
    expect(registerLink).toHaveAttribute('href', '/register')
  })
})