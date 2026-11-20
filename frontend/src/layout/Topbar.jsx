import Button from 'react-bootstrap/Button';
import { FiMenu } from 'react-icons/fi';

export function Topbar({ onMenu }) {
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
          <strong>Public demo</strong>
        </div>
      </div>
      <span className="demo-badge">Read and write access</span>
    </header>
  );
}
