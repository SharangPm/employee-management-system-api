import { NavLink } from 'react-router-dom';
import { FiBriefcase, FiGrid, FiUsers } from 'react-icons/fi';
import { useAuth } from '../auth/useAuth';

export function Sidebar({ open, onNavigate }) {
  const { role } = useAuth();
  const links =
    role === 'Admin'
      ? [
          { to: '/admin', label: 'Overview', icon: <FiGrid />, end: true },
          { to: '/employees', label: 'Employees', icon: <FiUsers /> },
        ]
      : [{ to: '/employee', label: 'My profile', icon: <FiBriefcase />, end: true }];

  return (
    <>
      {open && (
        <button
          className="sidebar-backdrop d-lg-none"
          aria-label="Close navigation"
          onClick={onNavigate}
        />
      )}
      <aside className={`app-sidebar ${open ? 'is-open' : ''}`}>
        <div className="brand-lockup">
          <div className="brand-mark">
            <FiBriefcase />
          </div>
          <div>
            <strong>PeopleDesk</strong>
            <span>WORKSPACE</span>
          </div>
        </div>
        <div className="nav-caption">WORKSPACE</div>
        <nav className="sidebar-nav" aria-label="Main navigation">
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end={link.end}
              className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
              onClick={onNavigate}
            >
              {link.icon}
              <span>{link.label}</span>
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">
          <span className="status-dot" /> API workspace
        </div>
      </aside>
    </>
  );
}
