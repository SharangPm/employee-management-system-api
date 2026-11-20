import { Link } from 'react-router-dom';
import Button from 'react-bootstrap/Button';
import { FiArrowLeft } from 'react-icons/fi';

export function NotFoundPage() {
  return (
    <div className="not-found-page">
      <span>404</span>
      <h1>We couldn't find that page</h1>
      <p>It may have moved, or the link may be incorrect.</p>
      <Button as={Link} to="/" className="primary-action">
        <FiArrowLeft /> Back to workspace
      </Button>
    </div>
  );
}
