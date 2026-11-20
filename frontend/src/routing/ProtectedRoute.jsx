import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';

export function ProtectedRoute({ roles }) {
  const { isAuthenticated, role } = useAuth();
  const location = useLocation();
  if (!isAuthenticated) return <Navigate to="/login" replace state={{ from: location }} />;
  if (roles && !roles.includes(role))
    return <Navigate to={role === 'Admin' ? '/admin' : '/employee'} replace />;
  return <Outlet />;
}

export function RoleHomeRedirect() {
  const { role } = useAuth();
  return <Navigate to={role === 'Admin' ? '/admin' : '/employee'} replace />;
}
