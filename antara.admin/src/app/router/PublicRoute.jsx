import { Navigate, Outlet } from 'react-router-dom'
import { storage } from '../../services/storage/localStorage'

export const PublicRoute = ({ children }) => {
  if (storage.hasSession()) {
    return <Navigate to="/dashboard" replace />
  }

  return children ?? <Outlet />
}

export default PublicRoute

