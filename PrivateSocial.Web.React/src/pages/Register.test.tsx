import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Register from './Register'

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

describe('Register Component', () => {
  const mockRegister = vi.fn()
  const mockNavigate = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    ;(useAuth as jest.Mock).mockReturnValue({
      register: mockRegister,
    })
    ;(useNavigate as jest.Mock).mockReturnValue(mockNavigate)
  })

  it('renders registration form correctly', () => {
    render(<Register />)

    expect(screen.getByRole('heading', { name: /register/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/first name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/last name/i)).toBeInTheDocument()
    expect(screen.getByLabelText('Username *')).toBeInTheDocument()
    expect(screen.getByLabelText('Email *')).toBeInTheDocument()
    expect(screen.getByLabelText('Password *')).toBeInTheDocument()
    expect(screen.getByLabelText('Confirm Password *')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /register/i })).toBeInTheDocument()
    expect(screen.getByText(/already have an account\?/i)).toBeInTheDocument()
  })

  it('handles successful registration', async () => {
    const user = userEvent.setup()
    mockRegister.mockResolvedValueOnce(undefined)

    render(<Register />)

    await user.type(screen.getByLabelText(/first name/i), 'John')
    await user.type(screen.getByLabelText(/last name/i), 'Doe')
    await user.type(screen.getByLabelText('Username *'), 'johndoe')
    await user.type(screen.getByLabelText('Email *'), 'john@example.com')
    await user.type(screen.getByLabelText('Password *'), 'P@ssword1234!')
    await user.type(screen.getByLabelText('Confirm Password *'), 'P@ssword1234!')
    await user.click(screen.getByRole('button', { name: /register/i }))

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith(
        'johndoe',
        'john@example.com',
        'P@ssword1234!',
        'John',
        'Doe'
      )
      expect(mockNavigate).toHaveBeenCalledWith('/')
    })
  })

  it('displays error when passwords do not match', async () => {
    const user = userEvent.setup()
    render(<Register />)

    await user.type(screen.getByLabelText('Username *'), 'johndoe')
    await user.type(screen.getByLabelText('Email *'), 'john@example.com')
    await user.type(screen.getByLabelText('Password *'), 'P@ssword1234!')
    await user.type(screen.getByLabelText('Confirm Password *'), 'D!fferentPass1')
    await user.click(screen.getByRole('button', { name: /register/i }))

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('Passwords do not match')
      expect(mockRegister).not.toHaveBeenCalled()
    })
  })

  it('displays error when password is too short', async () => {
    const user = userEvent.setup()
    render(<Register />)

    await user.type(screen.getByLabelText('Username *'), 'johndoe')
    await user.type(screen.getByLabelText('Email *'), 'john@example.com')
    await user.type(screen.getByLabelText('Password *'), 'Short1!abc')
    await user.type(screen.getByLabelText('Confirm Password *'), 'Short1!abc')
    await user.click(screen.getByRole('button', { name: /register/i }))

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('Password must be at least 12 characters')
      expect(mockRegister).not.toHaveBeenCalled()
    })
  })

  it('displays error message on failed registration', async () => {
    const user = userEvent.setup()
    const errorMessage = 'Username already exists'
    mockRegister.mockRejectedValueOnce(new Error(errorMessage))

    render(<Register />)

    await user.type(screen.getByLabelText('Username *'), 'existinguser')
    await user.type(screen.getByLabelText('Email *'), 'existing@example.com')
    await user.type(screen.getByLabelText('Password *'), 'P@ssword1234!')
    await user.type(screen.getByLabelText('Confirm Password *'), 'P@ssword1234!')
    await user.click(screen.getByRole('button', { name: /register/i }))

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(errorMessage)
    })
  })

  it('disables form during submission', async () => {
    const user = userEvent.setup()
    mockRegister.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)))

    render(<Register />)

    const submitButton = screen.getByRole('button', { name: /register/i })
    
    await user.type(screen.getByLabelText('Username *'), 'johndoe')
    await user.type(screen.getByLabelText('Email *'), 'john@example.com')
    await user.type(screen.getByLabelText('Password *'), 'P@ssword1234!')
    await user.type(screen.getByLabelText('Confirm Password *'), 'P@ssword1234!')
    await user.click(submitButton)

    expect(screen.getByLabelText('Username *')).toBeDisabled()
    expect(screen.getByLabelText('Email *')).toBeDisabled()
    expect(screen.getByLabelText('Password *')).toBeDisabled()
    expect(submitButton).toBeDisabled()
    expect(submitButton).toHaveTextContent('Creating Account...')

    await waitFor(() => {
      expect(submitButton).not.toBeDisabled()
      expect(submitButton).toHaveTextContent('Register')
    })
  })

  it('allows registration without optional fields', async () => {
    const user = userEvent.setup()
    mockRegister.mockResolvedValueOnce(undefined)

    render(<Register />)

    await user.type(screen.getByLabelText('Username *'), 'johndoe')
    await user.type(screen.getByLabelText('Email *'), 'john@example.com')
    await user.type(screen.getByLabelText('Password *'), 'P@ssword1234!')
    await user.type(screen.getByLabelText('Confirm Password *'), 'P@ssword1234!')
    await user.click(screen.getByRole('button', { name: /register/i }))

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith(
        'johndoe',
        'john@example.com',
        'P@ssword1234!',
        '',
        ''
      )
    })
  })

  it('navigates to login page when clicking login link', async () => {
    render(<Register />)

    const loginLink = screen.getByRole('link', { name: /login here/i })
    expect(loginLink).toHaveAttribute('href', '/login')
  })
})