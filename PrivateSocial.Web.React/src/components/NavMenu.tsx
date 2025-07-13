import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import '../styles/NavMenu.css'

function NavMenu() {
  const [collapseNavMenu, setCollapseNavMenu] = useState(true)
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const toggleNavMenu = () => {
    setCollapseNavMenu(!collapseNavMenu)
  }

  const handleLogout = () => {
    logout()
    navigate('/')
  }

  return (
    <>
      <div className="top-row ps-3 navbar navbar-dark">
        <div className="container-fluid">
          <a className="navbar-brand" href="">PrivateSocial</a>
          <button 
            title="Navigation menu" 
            className="navbar-toggler" 
            onClick={toggleNavMenu}
          >
            <span className="navbar-toggler-icon"></span>
          </button>
        </div>
      </div>

      <nav className={`flex-column ${collapseNavMenu ? 'collapse' : ''}`}>
        <div className="nav-item px-3">
          <NavLink className="nav-link" to="/">
            <span className="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
          </NavLink>
        </div>
        <div className="nav-item px-3">
          <NavLink className="nav-link" to="/counter">
            <span className="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> Counter
          </NavLink>
        </div>
        <div className="nav-item px-3">
          <NavLink className="nav-link" to="/weather">
            <span className="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Weather
          </NavLink>
        </div>
        <div className="nav-item px-3">
          <NavLink className="nav-link" to="/posts">
            <span className="bi bi-chat-square-text" aria-hidden="true"></span> Posts
          </NavLink>
        </div>
        
        <hr className="my-2" style={{ borderColor: 'rgba(255, 255, 255, 0.2)' }} />
        
        {user ? (
          <>
            <div className="nav-item px-3 text-light">
              <small>Logged in as: {user.username}</small>
            </div>
            <div className="nav-item px-3">
              <button className="nav-link btn btn-link" onClick={handleLogout}>
                <span className="bi bi-box-arrow-right" aria-hidden="true"></span> Logout
              </button>
            </div>
          </>
        ) : (
          <>
            <div className="nav-item px-3">
              <NavLink className="nav-link" to="/login">
                <span className="bi bi-box-arrow-in-right" aria-hidden="true"></span> Login
              </NavLink>
            </div>
            <div className="nav-item px-3">
              <NavLink className="nav-link" to="/register">
                <span className="bi bi-person-plus" aria-hidden="true"></span> Register
              </NavLink>
            </div>
          </>
        )}
      </nav>
    </>
  )
}

export default NavMenu