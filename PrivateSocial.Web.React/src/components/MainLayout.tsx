import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import NavMenu from './NavMenu'
import { ErrorBoundary } from 'react-error-boundary'
import ErrorFallback from './ErrorFallback'

function MainLayout() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Mobile menu overlay */}
      {mobileMenuOpen && (
        <div 
          className="fixed inset-0 bg-black bg-opacity-50 z-40 lg:hidden"
          onClick={() => setMobileMenuOpen(false)}
        />
      )}

      {/* Sidebar - hidden on mobile, visible on desktop */}
      <div className={`
        fixed lg:static inset-y-0 left-0 z-50
        transform ${mobileMenuOpen ? 'translate-x-0' : '-translate-x-full'}
        lg:translate-x-0 transition-transform duration-300 ease-in-out
        sidebar w-64 md:w-72 lg:w-64 flex-shrink-0
      `}>
        <NavMenu onClose={() => setMobileMenuOpen(false)} />
      </div>

      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Top bar with mobile menu button */}
        <div className="top-row">
          <button
            className="lg:hidden p-2 rounded-md text-gray-600 hover:text-gray-900 hover:bg-gray-100"
            onClick={() => setMobileMenuOpen(true)}
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
          <div className="flex-1 flex justify-end">
            <a href="https://learn.microsoft.com/aspnet/core/" target="_blank" rel="noopener" className="text-sm md:text-base text-gray-600 hover:text-blue-500 font-medium transition-colors">About</a>
          </div>
        </div>

        <article className="content flex-1 overflow-y-auto">
          <ErrorBoundary FallbackComponent={ErrorFallback}>
            <Outlet />
          </ErrorBoundary>
        </article>
      </main>
    </div>
  )
}

export default MainLayout