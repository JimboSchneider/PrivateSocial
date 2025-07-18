import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import PostCard from './PostCard'
import { Post } from '../services/postsService'

// Mock useAuth hook
vi.mock('../contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}))

import { useAuth } from '../contexts/AuthContext'

const mockPost: Post = {
  id: 1,
  content: 'This is a test post',
  authorId: 1,
  authorName: 'testuser',
  createdAt: '2024-01-01T12:00:00Z',
  updatedAt: null,
}

describe('PostCard Component', () => {
  const mockOnEdit = vi.fn()
  const mockOnDelete = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders post content correctly', () => {
    (useAuth as jest.Mock).mockReturnValue({ user: null })
    
    render(<PostCard post={mockPost} />)
    
    expect(screen.getByText('This is a test post')).toBeInTheDocument()
    expect(screen.getByText('testuser')).toBeInTheDocument()
    expect(screen.getByText(/1\/1\/2024/)).toBeInTheDocument() // Date format may vary
  })

  it('shows edit and delete buttons for post owner', () => {
    (useAuth as jest.Mock).mockReturnValue({ 
      user: { id: 1, username: 'testuser', email: 'test@example.com' } 
    })
    
    render(<PostCard post={mockPost} onEdit={mockOnEdit} onDelete={mockOnDelete} />)
    
    // Should see the dropdown button
    const dropdownButton = screen.getByLabelText('Post options')
    expect(dropdownButton).toBeInTheDocument()
  })

  it('hides options for non-owner', () => {
    (useAuth as jest.Mock).mockReturnValue({ 
      user: { id: 2, username: 'otheruser', email: 'other@example.com' } 
    })
    
    render(<PostCard post={mockPost} onEdit={mockOnEdit} onDelete={mockOnDelete} />)
    
    // Should not see the dropdown button
    expect(screen.queryByLabelText('Post options')).not.toBeInTheDocument()
  })

  it('has dropdown with edit and delete actions for owner', () => {
    (useAuth as jest.Mock).mockReturnValue({ 
      user: { id: 1, username: 'testuser', email: 'test@example.com' } 
    })
    
    render(<PostCard post={mockPost} onEdit={mockOnEdit} onDelete={mockOnDelete} />)
    
    // Verify the dropdown button exists
    const dropdownButton = screen.getByLabelText('Post options')
    expect(dropdownButton).toBeInTheDocument()
    
    // Since Bootstrap dropdowns require JS to work, we can at least verify
    // the structure exists in the DOM
    const dropdownMenu = dropdownButton.nextElementSibling
    expect(dropdownMenu).toHaveClass('dropdown-menu')
    
    // Verify buttons exist in the dropdown (even if not visible without Bootstrap JS)
    const editButton = screen.getByText('Edit')
    const deleteButton = screen.getByText('Delete')
    
    expect(editButton).toBeInTheDocument()
    expect(deleteButton).toBeInTheDocument()
  })

  it('shows edited indicator for updated posts', () => {
    (useAuth as jest.Mock).mockReturnValue({ user: null })
    
    const editedPost = {
      ...mockPost,
      updatedAt: '2024-01-02T12:00:00Z',
    }
    
    render(<PostCard post={editedPost} />)
    
    expect(screen.getByText('(edited)')).toBeInTheDocument()
  })

  it('does not show edited indicator when updatedAt equals createdAt', () => {
    (useAuth as jest.Mock).mockReturnValue({ user: null })
    
    const uneditedPost = {
      ...mockPost,
      updatedAt: mockPost.createdAt,
    }
    
    render(<PostCard post={uneditedPost} />)
    
    expect(screen.queryByText('(edited)')).not.toBeInTheDocument()
  })

  it('preserves whitespace in post content', () => {
    (useAuth as jest.Mock).mockReturnValue({ user: null })
    
    const postWithWhitespace = {
      ...mockPost,
      content: 'Line 1\n\nLine 2\n    Indented line',
    }
    
    render(<PostCard post={postWithWhitespace} />)
    
    const contentElement = screen.getByText(/Line 1/)
    expect(contentElement).toHaveStyle({ whiteSpace: 'pre-wrap' })
  })

  it('does not show edit button when onEdit is not provided', () => {
    (useAuth as jest.Mock).mockReturnValue({ 
      user: { id: 1, username: 'testuser', email: 'test@example.com' } 
    })
    
    render(<PostCard post={mockPost} onDelete={mockOnDelete} />)
    
    expect(screen.queryByRole('button', { name: /edit/i })).not.toBeInTheDocument()
  })

  it('does not show delete button when onDelete is not provided', () => {
    (useAuth as jest.Mock).mockReturnValue({ 
      user: { id: 1, username: 'testuser', email: 'test@example.com' } 
    })
    
    render(<PostCard post={mockPost} onEdit={mockOnEdit} />)
    
    expect(screen.queryByRole('button', { name: /delete/i })).not.toBeInTheDocument()
  })
})