import Spinner from 'react-bootstrap/Spinner';

export function LoadingScreen({ label = 'Loading...' }) {
  return (
    <div className="loading-screen" role="status" aria-live="polite">
      <Spinner animation="border" variant="primary" />
      <span className="ms-3">{label}</span>
    </div>
  );
}

export function InlineLoading({ label = 'Loading data...' }) {
  return (
    <div className="inline-loading" role="status">
      <Spinner size="sm" animation="border" variant="primary" />
      <span>{label}</span>
    </div>
  );
}
