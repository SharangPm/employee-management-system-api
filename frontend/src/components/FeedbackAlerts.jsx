import Alert from 'react-bootstrap/Alert';

export function ErrorAlert({ error, onClose }) {
  if (!error) return null;
  return (
    <Alert
      variant="danger"
      dismissible={Boolean(onClose)}
      onClose={onClose}
      className="border-0 shadow-sm"
    >
      <strong>Request failed.</strong> {error}
    </Alert>
  );
}

export function SuccessAlert({ message, onClose }) {
  if (!message) return null;
  return (
    <Alert variant="success" dismissible onClose={onClose} className="border-0 shadow-sm">
      {message}
    </Alert>
  );
}
