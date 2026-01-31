import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Posts from './Posts'
import { postsService } from '../services/postsService'

// Mock dependencies
vi.mock('../services/postsService', () => ({
  postsService: {
    getPosts: vi.fn(),
    createPost: vi.fn(),
    deletePost: vi.fn(),
  },
}))

vi.mock('../contexts/AuthContext', () => ({
  useAuth: () => ({
    user: { id: 1, username: 'testuser', email: 'test@example.com' },
  }),
}))

vi.mock('../components/CreatePostForm', () => ({
  default: ({ onPostCreated }: any) => (
    <div data-testid="create-post-form">
      <button onClick={onPostCreated}>Mock Create Post</button>
    </div>
  ),
}))

vi.mock('../components/PostCard', () => ({
  default: ({ post, onEdit, onDelete }: any) => (
    <div data-testid={`post-${post.id}`}>
      <div>{post.content}</div>
      <div>{post.authorName}</div>
      <button onClick={() => onEdit(post)}>Edit</button>
      <button onClick={() => onDelete(post.id)}>Delete</button>
    </div>
  ),
}))

const mockPosts = {
  items: [
    {
      id: 1,
      content: 'First post',
      authorId: 1,
      authorName: 'testuser',
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: null,
    },
    {
      id: 2,
      content: 'Second post',
      authorId: 2,
      authorName: 'otheruser',
      createdAt: '2024-01-02T00:00:00Z',
      updatedAt: null,
    },
  ],
  totalCount: 2,
  page: 1,
  pageSize: 20,
  totalPages: 1,
}

describe('Posts Component', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Mock window.confirm
    global.confirm = vi.fn(() => true)
    // Mock window.alert
    global.alert = vi.fn()
  })

  it('renders posts page with loading state initially', () => {
    postsService.getPosts = vi.fn().mockImplementation(() => new Promise(() => {}))
    
    render(<Posts />)
    
    expect(screen.getByRole('heading', { name: /posts/i })).toBeInTheDocument()
    expect(screen.getByText(/loading posts/i)).toBeInTheDocument()
  })

  it('displays posts after loading', async () => {
    postsService.getPosts = vi.fn().mockResolvedValue(mockPosts)
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByTestId('post-1')).toBeInTheDocument()
      expect(screen.getByTestId('post-2')).toBeInTheDocument()
    })
    
    expect(screen.getByText('First post')).toBeInTheDocument()
    expect(screen.getByText('Second post')).toBeInTheDocument()
    expect(screen.getByText('testuser')).toBeInTheDocument()
    expect(screen.getByText('otheruser')).toBeInTheDocument()
  })

  it('shows empty state when no posts exist', async () => {
    postsService.getPosts = vi.fn().mockResolvedValue({
      ...mockPosts,
      items: [],
      totalCount: 0,
    })
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByText(/no posts yet/i)).toBeInTheDocument()
    })
  })

  it('displays error message when loading fails', async () => {
    postsService.getPosts = vi.fn().mockRejectedValue(new Error('Failed to load posts'))

    render(<Posts />)

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(/failed to load posts/i)
    })
  })

  it('refreshes posts when new post is created', async () => {
    const user = userEvent.setup()
    postsService.getPosts = vi.fn().mockResolvedValue(mockPosts)
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByTestId('create-post-form')).toBeInTheDocument()
    })
    
    // Click the mock create post button
    await user.click(screen.getByText('Mock Create Post'))
    
    // Verify getPosts was called twice (initial load + after creation)
    expect(postsService.getPosts).toHaveBeenCalledTimes(2)
    expect(postsService.getPosts).toHaveBeenLastCalledWith(1, 20)
  })

  it('handles post deletion with confirmation', async () => {
    const user = userEvent.setup()
    postsService.getPosts = vi.fn().mockResolvedValue(mockPosts)
    postsService.deletePost = vi.fn().mockResolvedValue(undefined)
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByTestId('post-1')).toBeInTheDocument()
    })
    
    // Click delete on first post
    const deleteButtons = screen.getAllByText('Delete')
    await user.click(deleteButtons[0])
    
    expect(global.confirm).toHaveBeenCalledWith('Are you sure you want to delete this post?')
    expect(postsService.deletePost).toHaveBeenCalledWith(1)
    
    // Verify posts are refreshed after deletion
    expect(postsService.getPosts).toHaveBeenCalledTimes(2)
  })

  it('cancels deletion when user declines confirmation', async () => {
    const user = userEvent.setup()
    global.confirm = vi.fn(() => false)
    postsService.getPosts = vi.fn().mockResolvedValue(mockPosts)
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByTestId('post-1')).toBeInTheDocument()
    })
    
    const deleteButtons = screen.getAllByText('Delete')
    await user.click(deleteButtons[0])
    
    expect(postsService.deletePost).not.toHaveBeenCalled()
  })

  it('shows alert when deletion fails', async () => {
    const user = userEvent.setup()
    postsService.getPosts = vi.fn().mockResolvedValue(mockPosts)
    postsService.deletePost = vi.fn().mockRejectedValue(new Error('Delete failed'))
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByTestId('post-1')).toBeInTheDocument()
    })
    
    const deleteButtons = screen.getAllByText('Delete')
    await user.click(deleteButtons[0])
    
    await waitFor(() => {
      expect(global.alert).toHaveBeenCalledWith('Failed to delete post. Please try again.')
    })
  })

  it('renders pagination when there are multiple pages', async () => {
    const multiPagePosts = {
      ...mockPosts,
      totalCount: 50,
      totalPages: 3,
    }
    postsService.getPosts = vi.fn().mockResolvedValue(multiPagePosts)
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByLabelText(/posts pagination/i)).toBeInTheDocument()
    })
    
    expect(screen.getByText('Previous')).toBeInTheDocument()
    expect(screen.getByText('Next')).toBeInTheDocument()
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
  })

  it('navigates between pages', async () => {
    const user = userEvent.setup()
    const multiPagePosts = {
      ...mockPosts,
      totalCount: 50,
      totalPages: 3,
    }
    postsService.getPosts = vi.fn().mockResolvedValue(multiPagePosts)
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByLabelText(/posts pagination/i)).toBeInTheDocument()
    })
    
    // Click page 2
    await user.click(screen.getByText('2'))
    
    expect(postsService.getPosts).toHaveBeenLastCalledWith(2, 20)
    
    // Click Next
    await user.click(screen.getByText('Next'))
    
    expect(postsService.getPosts).toHaveBeenLastCalledWith(3, 20)
  })

  it('disables pagination buttons appropriately', async () => {
    const multiPagePosts = {
      ...mockPosts,
      totalCount: 50,
      totalPages: 3,
    }
    postsService.getPosts = vi.fn().mockResolvedValue(multiPagePosts)
    
    render(<Posts />)
    
    await waitFor(() => {
      expect(screen.getByLabelText(/posts pagination/i)).toBeInTheDocument()
    })
    
    // On first page, Previous should be disabled
    const previousButton = screen.getByText('Previous')
    expect(previousButton).toBeDisabled()
  })
})