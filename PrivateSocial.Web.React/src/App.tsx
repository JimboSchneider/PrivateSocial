import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import MainLayout from './components/MainLayout'
import ProtectedRoute from './components/ProtectedRoute'
import NavigationSetup from './components/NavigationSetup'
import Home from './pages/Home'
import Error from './pages/Error'
import Login from './pages/Login'
import Register from './pages/Register'
import Posts from './pages/Posts'

function App() {
  return (
    <Router>
      <NavigationSetup>
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
      </NavigationSetup>
    </Router>
  )
}

export default App