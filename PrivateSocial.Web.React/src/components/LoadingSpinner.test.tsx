import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import LoadingSpinner from './LoadingSpinner'

describe('LoadingSpinner Component', () => {
  it('renders with default props', () => {
    const { container } = render(<LoadingSpinner />)
    
    // Should have a spinner element
    const spinner = container.querySelector('.animate-spin')
    expect(spinner).toBeInTheDocument()
    expect(spinner).toHaveClass('h-8', 'w-8') // medium size by default
  })

  it('renders with small size', () => {
    const { container } = render(<LoadingSpinner size="small" />)
    
    const spinner = container.querySelector('.animate-spin')
    expect(spinner).toHaveClass('h-4', 'w-4')
  })

  it('renders with large size', () => {
    const { container } = render(<LoadingSpinner size="large" />)
    
    const spinner = container.querySelector('.animate-spin')
    expect(spinner).toHaveClass('h-12', 'w-12')
  })

  it('displays custom message', () => {
    render(<LoadingSpinner message="Loading data..." />)
    
    expect(screen.getByText('Loading data...')).toBeInTheDocument()
    expect(screen.getByText('Loading data...')).toHaveClass('text-sm', 'text-gray-600')
  })

  it('does not display message when not provided', () => {
    render(<LoadingSpinner />)
    
    expect(screen.queryByText(/loading/i)).not.toBeInTheDocument()
  })

  it('renders fullscreen overlay when fullScreen is true', () => {
    const { container } = render(<LoadingSpinner fullScreen />)
    
    const overlay = container.querySelector('.fixed.inset-0')
    expect(overlay).toBeInTheDocument()
    expect(overlay).toHaveClass('bg-white', 'bg-opacity-75', 'z-50')
  })

  it('renders inline when fullScreen is false', () => {
    const { container } = render(<LoadingSpinner fullScreen={false} />)
    
    const wrapper = container.querySelector('.flex.flex-col')
    expect(wrapper).toBeInTheDocument()
    expect(wrapper).toHaveClass('p-4')
    
    // Should not have fullscreen overlay
    expect(container.querySelector('.fixed.inset-0')).not.toBeInTheDocument()
  })

  it('combines fullScreen with message', () => {
    render(<LoadingSpinner fullScreen message="Please wait..." />)
    
    expect(screen.getByText('Please wait...')).toBeInTheDocument()
    const overlay = document.querySelector('.fixed.inset-0')
    expect(overlay).toBeInTheDocument()
  })
})