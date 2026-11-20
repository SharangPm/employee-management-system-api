import Button from 'react-bootstrap/Button';
import { FiMenu, FiLogOut } from 'react-icons/fi';
import { useAuth } from '../auth/useAuth';

export function Topbar({ onMenu, onLogout }) {
  const { email, role } = useAuth();
  const initials = email?.split('@')[0]?.slice(0, 2).toUpperCase() || 'U';

  return (
    <header className="topbar">
      <div className="d-flex align-items-center gap-2">
        <Button
          variant="light"
          className="icon-button d-lg-none"
          aria-label="Open navigation"
          onClick={onMenu}
        >
          <FiMenu />
        </Button>
        <div className="topbar-context">
          <span>Employee Management</span>
          <span className="context-divider">/</span>
          <strong>{role === 'Admin' ? 'Admin workspace' : 'Employee workspace'}</strong>
        </div>
      </div>
      <div className="d-flex align-items-center gap-3">
        <div className="topbar-user d-none d-sm-flex">
          <div className="user-avatar">{initials}</div>
          <div className="user-copy">
            <strong>{email?.split('@')[0]}</strong>
            <span>{role}</span>
          </div>
        </div>
        <Button variant="outline-secondary" size="sm" className="logout-button" onClick={onLogout}>
          <FiLogOut />
          <span className="ms-2 d-none d-md-inline">Sign out</span>
        </Button>
      </div>
    </header>
  );
}
