import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Badge from 'react-bootstrap/Badge';
import Button from 'react-bootstrap/Button';
import Card from 'react-bootstrap/Card';
import { FiArrowLeft, FiEdit2, FiMail, FiMapPin, FiPhone, FiTrash2, FiUser } from 'react-icons/fi';
import { deleteEmployee, getEmployee } from '../api/employees';
import { getApiErrorMessage } from '../api/errors';
import { ConfirmModal } from '../components/ConfirmModal';
import { ErrorAlert } from '../components/FeedbackAlerts';
import { LoadingScreen } from '../components/LoadingScreen';
import { formatDate, formatMoney, getInitials } from '../utils/formatters';

export function EmployeeDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [employee, setEmployee] = useState(null);
  const [error, setError] = useState('');
  const [confirm, setConfirm] = useState(false);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    let active = true;
    getEmployee(id)
      .then((result) => {
        if (active) setEmployee(result);
      })
      .catch((requestError) => {
        if (active) setError(getApiErrorMessage(requestError, 'Unable to load employee details.'));
      });
    return () => {
      active = false;
    };
  }, [id]);

  async function removeEmployee() {
    setDeleting(true);
    try {
      await deleteEmployee(id);
      navigate('/employees', { replace: true });
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, 'Unable to delete this employee.'));
      setConfirm(false);
      setDeleting(false);
    }
  }

  if (!employee && !error) return <LoadingScreen label="Loading employee profile..." />;

  return (
    <div className="page-stack">
      <div className="page-heading-row detail-heading">
        <div>
          <Link to="/employees" className="back-link">
            <FiArrowLeft /> Employee directory
          </Link>
          <div className="eyebrow mt-3">EMPLOYEE PROFILE</div>
          <h1>Employee details</h1>
        </div>
        <div className="d-flex gap-2">
          <Button
            as={Link}
            to={`/employees/${id}/edit`}
            variant="light"
            className="secondary-action"
          >
            <FiEdit2 /> Edit profile
          </Button>
          <Button
            variant="outline-danger"
            className="delete-action"
            onClick={() => setConfirm(true)}
          >
            <FiTrash2 />
          </Button>
        </div>
      </div>
      <ErrorAlert error={error} />
      {employee && (
        <>
          <Card className="profile-hero-card border-0">
            <Card.Body>
              <div className="profile-hero-avatar">
                {getInitials(employee.firstName, employee.lastName)}
              </div>
              <div className="profile-hero-main">
                <div className="d-flex flex-wrap align-items-center gap-2">
                  <h2>
                    {employee.firstName} {employee.lastName}
                  </h2>
                  <span
                    className={`status-pill ${employee.isActive ? 'status-active' : 'status-inactive'}`}
                  >
                    {employee.isActive ? 'Active' : 'Inactive'}
                  </span>
                </div>
                <p>
                  {employee.designation} <span>...</span> {employee.department}
                </p>
                <div className="profile-contact-row">
                  <span>
                    <FiMail />
                    {employee.email}
                  </span>
                  {employee.phoneNumber && (
                    <span>
                      <FiPhone />
                      {employee.phoneNumber}
                    </span>
                  )}
                </div>
              </div>
              <div className="employee-id-block">
                <span>EMPLOYEE ID</span>
                <code>{employee.id}</code>
              </div>
            </Card.Body>
          </Card>
          <div className="details-grid">
            <Card className="content-card border-0">
              <Card.Body>
                <div className="section-heading-row">
                  <div>
                    <h2>Employment information</h2>
                    <p>Position and employment timeline.</p>
                  </div>
                  <FiMapPin className="section-heading-icon" />
                </div>
                <div className="detail-fields">
                  <Detail label="Department" value={employee.department} />
                  <Detail label="Designation" value={employee.designation} />
                  <Detail label="Date of joining" value={formatDate(employee.dateOfJoining)} />
                  <Detail label="Record created" value={formatDate(employee.createdAt)} />
                  <Detail label="Last updated" value={formatDate(employee.updatedAt)} />
                </div>
              </Card.Body>
            </Card>
            <Card className="content-card border-0">
              <Card.Body>
                <div className="section-heading-row">
                  <div>
                    <h2>Compensation</h2>
                    <p>Current compensation information.</p>
                  </div>
                  <span className="compensation-icon">
                    <FiUser />
                  </span>
                </div>
                <span className="salary-label">ANNUAL SALARY</span>
                <strong className="salary-display">{formatMoney(employee.salary)}</strong>
                <Badge bg={employee.isActive ? 'success' : 'secondary'} className="status-badge">
                  {employee.isActive ? 'Currently employed' : 'Inactive record'}
                </Badge>
              </Card.Body>
            </Card>
          </div>
        </>
      )}
      <ConfirmModal
        show={confirm}
        title="Delete employee?"
        message={
          employee
            ? `This will permanently remove ${employee.firstName} ${employee.lastName} from the employee directory.`
            : ''
        }
        confirmLabel="Delete employee"
        busy={deleting}
        onCancel={() => !deleting && setConfirm(false)}
        onConfirm={removeEmployee}
      />
    </div>
  );
}

function Detail({ label, value }) {
  return (
    <div className="detail-field">
      <span>{label}</span>
      <strong>{value || '...'}</strong>
    </div>
  );
}
