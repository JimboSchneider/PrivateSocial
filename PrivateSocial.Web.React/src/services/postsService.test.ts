import { describe, it, expect, vi, beforeEach } from 'vitest'
import { postsService } from './postsService'
import api from './api'

// Mock the API module
vi.mock('./api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

describe('postsService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('getPosts', () => {
    it('fetches posts with default parameters', async () => {
      const mockResponse = {
        data: {
          items: [],
          totalCount: 0,
          page: 1,
          pageSize: 20,
          totalPages: 0,
        },
      }
      
      api.get = vi.fn().mockResolvedValue(mockResponse)
      
      const result = await postsService.getPosts()
      
      expect(api.get).toHaveBeenCalledWith('/posts', {
        params: { page: 1, pageSize: 20 },
      })
      expect(result).toEqual(mockResponse.data)
    })

    it('fetches posts with custom parameters', async () => {
      const mockResponse = {
        data: {
          items: [],
          totalCount: 50,
          page: 2,
          pageSize: 10,
          totalPages: 5,
        },
      }
      
      api.get = vi.fn().mockResolvedValue(mockResponse)
      
      const result = await postsService.getPosts(2, 10)
      
      expect(api.get).toHaveBeenCalledWith('/posts', {
        params: { page: 2, pageSize: 10 },
      })
      expect(result).toEqual(mockResponse.data)
    })
  })

  describe('getPost', () => {
    it('fetches a single post by id', async () => {
      const mockPost = {
        id: 1,
        content: 'Test post',
        authorId: 1,
        authorName: 'testuser',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: null,
      }
      
      api.get = vi.fn().mockResolvedValue({ data: mockPost })
      
      const result = await postsService.getPost(1)
      
      expect(api.get).toHaveBeenCalledWith('/posts/1')
      expect(result).toEqual(mockPost)
    })
  })

  describe('createPost', () => {
    it('creates a new post', async () => {
      const createRequest = { content: 'New post content' }
      const mockCreatedPost = {
        id: 1,
        content: 'New post content',
        authorId: 1,
        authorName: 'testuser',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: null,
      }
      
      api.post = vi.fn().mockResolvedValue({ data: mockCreatedPost })
      
      const result = await postsService.createPost(createRequest)
      
      expect(api.post).toHaveBeenCalledWith('/posts', createRequest)
      expect(result).toEqual(mockCreatedPost)
    })
  })

  describe('updatePost', () => {
    it('updates an existing post', async () => {
      const updateRequest = { content: 'Updated content' }
      const mockUpdatedPost = {
        id: 1,
        content: 'Updated content',
        authorId: 1,
        authorName: 'testuser',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      }
      
      api.put = vi.fn().mockResolvedValue({ data: mockUpdatedPost })
      
      const result = await postsService.updatePost(1, updateRequest)
      
      expect(api.put).toHaveBeenCalledWith('/posts/1', updateRequest)
      expect(result).toEqual(mockUpdatedPost)
    })
  })

  describe('deletePost', () => {
    it('deletes a post by id', async () => {
      api.delete = vi.fn().mockResolvedValue({ data: null })
      
      await postsService.deletePost(1)
      
      expect(api.delete).toHaveBeenCalledWith('/posts/1')
    })

    it('returns void even when API returns data', async () => {
      api.delete = vi.fn().mockResolvedValue({ data: { message: 'Deleted' } })
      
      const result = await postsService.deletePost(1)
      
      expect(result).toBeUndefined()
    })
  })
})