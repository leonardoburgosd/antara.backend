import { Navigate, Outlet } from 'react-router-dom'
import { storage } from '../../services/storage/localStorage'

export const ProtectedRoute = ({ children }) => {
  if (!storage.hasSession()) {
    return <Navigate to="/login" replace />
  }

  return children ?? <Outlet />
}

export default ProtectedRoute

