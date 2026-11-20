import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './auth/AuthContext';
import { useAuth } from './auth/useAuth';
import { LoadingScreen } from './components/LoadingScreen';
import { AppLayout } from './layout/AppLayout';
import { AdminDashboardPage } from './pages/AdminDashboardPage';
import { EmployeeDetailsPage } from './pages/EmployeeDetailsPage';
import { EmployeeFormPage } from './pages/EmployeeFormPage';
import { EmployeeListPage } from './pages/EmployeeListPage';
import { EmployeeDashboardPage } from './pages/EmployeeDashboardPage';
import { LoginPage } from './pages/LoginPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { ProtectedRoute, RoleHomeRedirect } from './routing/ProtectedRoute';

function ApplicationRoutes() {
  const { isAuthenticated } = useAuth();
  if (isAuthenticated === undefined) return <LoadingScreen />;

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <RoleHomeRedirect /> : <LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<RoleHomeRedirect />} />
          <Route element={<ProtectedRoute roles={['Admin']} />}>
            <Route path="/admin" element={<AdminDashboardPage />} />
            <Route path="/employees" element={<EmployeeListPage />} />
            <Route path="/employees/new" element={<EmployeeFormPage mode="create" />} />
            <Route path="/employees/:id" element={<EmployeeDetailsPage />} />
            <Route path="/employees/:id/edit" element={<EmployeeFormPage mode="edit" />} />
          </Route>
          <Route element={<ProtectedRoute roles={['Employee']} />}>
            <Route path="/employee" element={<EmployeeDashboardPage />} />
          </Route>
        </Route>
      </Route>
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <ApplicationRoutes />
      </BrowserRouter>
    </AuthProvider>
  );
}
