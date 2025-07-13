import { Outlet } from 'react-router-dom'
import NavMenu from './NavMenu'
import { ErrorBoundary } from 'react-error-boundary'
import ErrorFallback from './ErrorFallback'
import '../styles/MainLayout.css'

function MainLayout() {
  return (
    <div className="page">
      <div className="sidebar">
        <NavMenu />
      </div>

      <main>
        <div className="top-row px-4">
          <a href="https://learn.microsoft.com/aspnet/core/" target="_blank" rel="noopener">About</a>
        </div>

        <article className="content px-4">
          <ErrorBoundary FallbackComponent={ErrorFallback}>
            <Outlet />
          </ErrorBoundary>
        </article>
      </main>
    </div>
  )
}

export default MainLayout