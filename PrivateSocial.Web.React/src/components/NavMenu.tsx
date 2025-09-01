import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

interface NavMenuProps {
  onClose?: () => void
}

function NavMenu({ onClose }: NavMenuProps) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleNavClick = () => {
    if (onClose) onClose()
  }

  const handleLogout = () => {
    logout()
    navigate('/')
    if (onClose) onClose()
  }

  return (
    <>
      <div className="h-16 bg-gray-800 px-4 md:px-6 flex items-center justify-between">
        <a className="text-lg md:text-xl font-semibold text-white tracking-tight" href="">PrivateSocial</a>
        {/* Close button for mobile */}
        <button 
          title="Close menu" 
          className="lg:hidden p-2 text-white/80 hover:text-white transition-colors" 
          onClick={onClose}
        >
          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <nav className="flex flex-col h-[calc(100vh-4rem)] overflow-y-auto py-2 px-2">
        <div className="nav-item">
          <NavLink 
            className={({ isActive }) => 
              `nav-link ${isActive ? 'active' : ''}`
            } 
            to="/"
            onClick={handleNavClick}
          >
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z" />
            </svg>
            <span>Home</span>
          </NavLink>
        </div>
        <div className="nav-item">
          <NavLink 
            className={({ isActive }) => 
              `nav-link ${isActive ? 'active' : ''}`
            } 
            to="/posts"
            onClick={handleNavClick}
          >
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M2 5a2 2 0 012-2h8a2 2 0 012 2v10a2 2 0 002 2H4a2 2 0 01-2-2V5zm3 1h6v4H5V6zm6 6H5v2h6v-2z" clipRule="evenodd" />
              <path d="M15 7h1a2 2 0 012 2v5.5a1.5 1.5 0 01-3 0V7z" />
            </svg>
            <span>Posts</span>
          </NavLink>
        </div>
        
        <hr className="my-2 border-white/20" />
        
        {user ? (
          <>
            <div className="px-4 py-2 text-gray-400 text-xs md:text-sm">
              <span className="block md:inline">Logged in as:</span>
              <span className="block md:inline md:ml-1 font-medium">{user.username}</span>
            </div>
            <div className="nav-item">
              <button className="nav-link w-full text-left" onClick={handleLogout}>
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                <span>Logout</span>
              </button>
            </div>
          </>
        ) : (
          <>
            <div className="nav-item">
              <NavLink 
                className={({ isActive }) => 
                  `nav-link ${isActive ? 'active' : ''}`
                } 
                to="/login"
                onClick={handleNavClick}
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                </svg>
                <span>Login</span>
              </NavLink>
            </div>
            <div className="nav-item">
              <NavLink 
                className={({ isActive }) => 
                  `nav-link ${isActive ? 'active' : ''}`
                } 
                to="/register"
                onClick={handleNavClick}
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                </svg>
                <span>Register</span>
              </NavLink>
            </div>
          </>
        )}
      </nav>
    </>
  )
}

export default NavMenu