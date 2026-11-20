import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import { FiAlertTriangle } from 'react-icons/fi';

export function ConfirmModal({
  show,
  title,
  message,
  confirmLabel = 'Confirm',
  busy = false,
  onCancel,
  onConfirm,
}) {
  return (
    <Modal show={show} onHide={onCancel} centered aria-labelledby="confirm-modal-title">
      <Modal.Header closeButton={!busy}>
        <Modal.Title id="confirm-modal-title" className="d-flex align-items-center gap-2">
          <span className="modal-warning-icon">
            <FiAlertTriangle />
          </span>
          {title}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>{message}</Modal.Body>
      <Modal.Footer>
        <Button variant="light" onClick={onCancel} disabled={busy}>
          Cancel
        </Button>
        <Button variant="danger" onClick={onConfirm} disabled={busy}>
          {busy ? 'Working...' : confirmLabel}
        </Button>
      </Modal.Footer>
    </Modal>
  );
}
