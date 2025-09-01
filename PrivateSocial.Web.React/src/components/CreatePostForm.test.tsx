import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import CreatePostForm from './CreatePostForm'
import { postsService } from '../services/postsService'

// Mock the posts service
vi.mock('../services/postsService', () => ({
  postsService: {
    createPost: vi.fn(),
  },
}))

describe('CreatePostForm Component', () => {
  const mockOnPostCreated = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders form elements correctly', () => {
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    expect(screen.getByPlaceholderText(/what's on your mind/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /share post/i })).toBeInTheDocument()
    expect(screen.getByText('0/500 characters')).toBeInTheDocument()
  })

  it('updates character count as user types', async () => {
    const user = userEvent.setup()
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const textarea = screen.getByPlaceholderText(/what's on your mind/i)
    await user.type(textarea, 'Hello world!')
    
    expect(screen.getByText('12/500 characters')).toBeInTheDocument()
  })

  it('submits form with valid content', async () => {
    const user = userEvent.setup()
    const mockPost = {
      id: 1,
      content: 'Test post content',
      authorId: 1,
      authorName: 'testuser',
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: null,
    }
    
    postsService.createPost = vi.fn().mockResolvedValue(mockPost)
    
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const textarea = screen.getByPlaceholderText(/what's on your mind/i)
    const submitButton = screen.getByRole('button', { name: /share post/i })
    
    await user.type(textarea, 'Test post content')
    await user.click(submitButton)
    
    expect(postsService.createPost).toHaveBeenCalledWith({
      content: 'Test post content',
    })
    
    await waitFor(() => {
      expect(mockOnPostCreated).toHaveBeenCalled()
      expect(textarea).toHaveValue('') // Form should be cleared
    })
  })

  it('prevents submission with empty content via button disabled state', () => {
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const submitButton = screen.getByRole('button', { name: /share post/i })
    
    // Button should be disabled when content is empty
    expect(submitButton).toBeDisabled()
    
    // Service should not be called
    expect(postsService.createPost).not.toHaveBeenCalled()
  })

  it('shows error when submitting whitespace only', async () => {
    const user = userEvent.setup()
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const textarea = screen.getByPlaceholderText(/what's on your mind/i)
    await user.type(textarea, '   ')
    
    // Button should remain disabled with only whitespace
    const submitButton = screen.getByRole('button', { name: /share post/i })
    expect(submitButton).toBeDisabled()
    
    // Try to submit anyway by clearing and using Enter key
    await user.clear(textarea)
    await user.type(textarea, '{Enter}')
    
    // Should not call the service
    expect(postsService.createPost).not.toHaveBeenCalled()
  })

  it('disables submit button when content is empty', () => {
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const submitButton = screen.getByRole('button', { name: /share post/i })
    expect(submitButton).toBeDisabled()
  })

  it('enables submit button when content is entered', async () => {
    const user = userEvent.setup()
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const textarea = screen.getByPlaceholderText(/what's on your mind/i)
    const submitButton = screen.getByRole('button', { name: /share post/i })
    
    await user.type(textarea, 'Some content')
    
    expect(submitButton).not.toBeDisabled()
  })

  it('shows loading state during submission', async () => {
    const user = userEvent.setup()
    let resolvePromise: (value: any) => void
    const promise = new Promise((resolve) => {
      resolvePromise = resolve
    })
    
    postsService.createPost = vi.fn().mockReturnValue(promise)
    
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const textarea = screen.getByPlaceholderText(/what's on your mind/i)
    await user.type(textarea, 'Test post')
    
    const submitButton = screen.getByRole('button', { name: /share post/i })
    await user.click(submitButton)
    
    // Should show loading state immediately
    expect(submitButton).toHaveTextContent('Posting...')
    expect(submitButton).toBeDisabled()
    expect(textarea).toBeDisabled()
    
    // Resolve the promise
    resolvePromise!({
      id: 1,
      content: 'Test post',
      authorId: 1,
      authorName: 'testuser',
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: null,
    })
    
    // Wait for form to reset
    await waitFor(() => {
      expect(submitButton).toHaveTextContent('Share Post')
      expect(textarea).toHaveValue('')
    })
  })

  it('displays error message when API call fails', async () => {
    const user = userEvent.setup()
    const errorMessage = 'Failed to create post'
    postsService.createPost = vi.fn().mockRejectedValue(new Error(errorMessage))
    
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const textarea = screen.getByPlaceholderText(/what's on your mind/i)
    await user.type(textarea, 'Test post')
    
    const submitButton = screen.getByRole('button', { name: /share post/i })
    await user.click(submitButton)
    
    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(errorMessage)
    })
    
    // Form should not be cleared on error
    expect(textarea).toHaveValue('Test post')
    expect(mockOnPostCreated).not.toHaveBeenCalled()
  })

  it('enforces maximum character limit', async () => {
    const user = userEvent.setup()
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const textarea = screen.getByPlaceholderText(/what's on your mind/i)
    const longText = 'a'.repeat(501)
    
    await user.type(textarea, longText)
    
    // Textarea should enforce maxLength
    expect(textarea).toHaveValue('a'.repeat(500))
    expect(screen.getByText('500/500 characters')).toBeInTheDocument()
  })

  it('trims content before submission', async () => {
    const user = userEvent.setup()
    postsService.createPost = vi.fn().mockResolvedValue({
      id: 1,
      content: 'Trimmed content',
      authorId: 1,
      authorName: 'testuser',
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: null,
    })
    
    render(<CreatePostForm onPostCreated={mockOnPostCreated} />)
    
    const textarea = screen.getByPlaceholderText(/what's on your mind/i)
    await user.type(textarea, '  Trimmed content  ')
    
    const submitButton = screen.getByRole('button', { name: /share post/i })
    await user.click(submitButton)
    
    expect(postsService.createPost).toHaveBeenCalledWith({
      content: 'Trimmed content',
    })
  })
})