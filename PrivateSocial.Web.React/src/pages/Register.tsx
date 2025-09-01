import { useState, FormEvent } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { extractErrorMessage } from '../utils/errorHandler'

function Register() {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: ''
  })
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  
  const { register } = useAuth()
  const navigate = useNavigate()

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    })
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')

    // Validate passwords match
    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match')
      return
    }

    // Validate password length
    if (formData.password.length < 6) {
      setError('Password must be at least 6 characters long')
      return
    }

    setIsLoading(true)

    try {
      await register(
        formData.username, 
        formData.email, 
        formData.password,
        formData.firstName,
        formData.lastName
      )
      navigate('/')
    } catch (err: any) {
      setError(extractErrorMessage(err))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="max-w-lg mx-auto mt-4 md:mt-8 px-4">
      <div className="card">
        <h1 className="text-xl md:text-2xl font-bold text-gray-900 mb-4 md:mb-6">Register</h1>
        
        {error && (
          <div className="alert alert-danger" role="alert">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <div>
              <label htmlFor="firstName" className="form-label">First Name (optional)</label>
              <input
                type="text"
                className="form-control"
                id="firstName"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                disabled={isLoading}
              />
            </div>

            <div>
              <label htmlFor="lastName" className="form-label">Last Name (optional)</label>
              <input
                type="text"
                className="form-control"
                id="lastName"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                disabled={isLoading}
              />
            </div>
          </div>

          <div className="mb-3 md:mb-4">
            <label htmlFor="username" className="form-label">Username *</label>
            <input
              type="text"
              className="form-control"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleChange}
              required
              disabled={isLoading}
            />
          </div>

          <div className="mb-3 md:mb-4">
            <label htmlFor="email" className="form-label">Email *</label>
            <input
              type="email"
              className="form-control"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
              disabled={isLoading}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4 md:mb-6">
            <div>
              <label htmlFor="password" className="form-label">Password *</label>
              <input
                type="password"
                className="form-control"
                id="password"
                name="password"
                value={formData.password}
                onChange={handleChange}
                required
                minLength={6}
                disabled={isLoading}
              />
            </div>

            <div>
              <label htmlFor="confirmPassword" className="form-label">Confirm Password *</label>
              <input
                type="password"
                className="form-control"
                id="confirmPassword"
                name="confirmPassword"
                value={formData.confirmPassword}
                onChange={handleChange}
                required
                minLength={6}
                disabled={isLoading}
              />
            </div>
          </div>

          <button 
            type="submit" 
            className="btn btn-primary w-full"
            disabled={isLoading}
          >
            {isLoading ? 'Creating Account...' : 'Register'}
          </button>
        </form>

        <p className="mt-4 md:mt-6 text-center text-sm md:text-base text-gray-600">
          <span className="block md:inline">Already have an account?</span>{' '}
          <Link to="/login" className="text-blue-500 hover:text-blue-600 font-medium block md:inline mt-1 md:mt-0">Login here</Link>
        </p>
      </div>
    </div>
  )
}

export default Register