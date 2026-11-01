import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthCheckPage, LoginPage, CreateFirstAdminPage } from '../../features/authentication'
import { DashboardPage } from '../../features/dashboard'
import { ProtectedRoute } from './ProtectedRoute'

export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<AuthCheckPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/create-admin" element={<CreateFirstAdminPage />} />
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <DashboardPage />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default AppRouter
