import { useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';

export function AppLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login', { replace: true });
  }

  return (
    <div className="app-shell">
      <Sidebar open={sidebarOpen} onNavigate={() => setSidebarOpen(false)} />
      <div className="app-main">
        <Topbar onMenu={() => setSidebarOpen(true)} onLogout={handleLogout} />
        <main className="page-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
