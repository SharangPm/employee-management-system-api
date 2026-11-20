import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { AppLayout } from './layout/AppLayout';
import { AdminDashboardPage } from './pages/AdminDashboardPage';
import { EmployeeDetailsPage } from './pages/EmployeeDetailsPage';
import { EmployeeFormPage } from './pages/EmployeeFormPage';
import { EmployeeListPage } from './pages/EmployeeListPage';
import { NotFoundPage } from './pages/NotFoundPage';

function ApplicationRoutes() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<AdminDashboardPage />} />
        <Route path="/admin" element={<AdminDashboardPage />} />
        <Route path="/employees" element={<EmployeeListPage />} />
        <Route path="/employees/new" element={<EmployeeFormPage mode="create" />} />
        <Route path="/employees/:id" element={<EmployeeDetailsPage />} />
        <Route path="/employees/:id/edit" element={<EmployeeFormPage mode="edit" />} />
      </Route>
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <ApplicationRoutes />
    </BrowserRouter>
  );
}
