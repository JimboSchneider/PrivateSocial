import { useState, FormEvent } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { extractErrorMessage } from '../utils/errorHandler'

function Login() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  
  const { login } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      await login(username, password)
      navigate('/')
    } catch (err: any) {
      setError(extractErrorMessage(err))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="max-w-md mx-auto mt-4 md:mt-8 px-4">
      <div className="card">
        <h1 className="text-xl md:text-2xl font-bold text-gray-900 mb-4 md:mb-6">Login</h1>
        
        {error && (
          <div className="alert alert-danger" role="alert">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label htmlFor="username" className="form-label">Username</label>
            <input
              type="text"
              className="form-control"
              id="username"
              name="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              disabled={isLoading}
            />
          </div>

          <div className="mb-6">
            <label htmlFor="password" className="form-label">Password</label>
            <input
              type="password"
              className="form-control"
              id="password"
              name="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={isLoading}
            />
          </div>

          <button 
            type="submit" 
            className="btn btn-primary w-full"
            disabled={isLoading}
          >
            {isLoading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        <p className="mt-4 md:mt-6 text-center text-sm md:text-base text-gray-600">
          <span className="block md:inline">Don't have an account?</span>{' '}
          <Link to="/register" className="text-blue-500 hover:text-blue-600 font-medium block md:inline mt-1 md:mt-0">Register here</Link>
        </p>
      </div>
    </div>
  )
}

export default Login