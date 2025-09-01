import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import { ApiProvider } from './contexts/ApiContext'
import { LoadingProvider } from './contexts/LoadingContext'
import AppErrorBoundary from './components/AppErrorBoundary'
import MainLayout from './components/MainLayout'
import ProtectedRoute from './components/ProtectedRoute'
import Home from './pages/Home'
import Error from './pages/Error'
import Login from './pages/Login'
import Register from './pages/Register'
import Posts from './pages/Posts'

function App() {
  return (
    <AppErrorBoundary>
      <Router>
        <ApiProvider>
          <LoadingProvider>
            <AuthProvider>
              <Routes>
                <Route path="/" element={<MainLayout />}>
                  <Route index element={<Home />} />
                  <Route path="login" element={<Login />} />
                  <Route path="register" element={<Register />} />
                  <Route path="posts" element={
                    <ProtectedRoute>
                      <Posts />
                    </ProtectedRoute>
                  } />
                  <Route path="error" element={<Error />} />
                </Route>
              </Routes>
            </AuthProvider>
          </LoadingProvider>
        </ApiProvider>
      </Router>
    </AppErrorBoundary>
  )
}

export default App