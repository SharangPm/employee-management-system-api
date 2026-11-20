import { NavLink } from 'react-router-dom';
import { FiBriefcase, FiGrid, FiUsers } from 'react-icons/fi';

export function Sidebar({ open, onNavigate }) {
  const links = [
    { to: '/', label: 'Overview', icon: <FiGrid />, end: true },
    { to: '/employees', label: 'Employees', icon: <FiUsers /> },
  ];

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
          <span className="status-dot" /> Public demo
        </div>
      </aside>
    </>
  );
}
