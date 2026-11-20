import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import Button from 'react-bootstrap/Button';
import { FiArrowLeft } from 'react-icons/fi';
import { createEmployee, getEmployee, updateEmployee } from '../api/employees';
import { getApiErrorMessage } from '../api/errors';
import { EmployeeForm } from '../components/EmployeeForm';
import { ErrorAlert } from '../components/FeedbackAlerts';
import { LoadingScreen } from '../components/LoadingScreen';

export function EmployeeFormPage({ mode }) {
  const isEdit = mode === 'edit';
  const { id } = useParams();
  const navigate = useNavigate();
  const [initialValue, setInitialValue] = useState(null);
  const [loading, setLoading] = useState(isEdit);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!isEdit) return;
    let active = true;
    getEmployee(id)
      .then((employee) => {
        if (active) setInitialValue(employee);
      })
      .catch((requestError) => {
        if (active) setError(getApiErrorMessage(requestError, 'Unable to load this employee.'));
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [id, isEdit]);

  async function save(employee) {
    const saved = isEdit ? await updateEmployee(id, employee) : await createEmployee(employee);
    navigate(`/employees/${saved.id}`, { replace: true });
  }

  if (loading) return <LoadingScreen label="Loading employee record..." />;

  return (
    <div className="page-stack form-page">
      <div className="page-heading-row">
        <div>
          <Link to={isEdit ? `/employees/${id}` : '/employees'} className="back-link">
            <FiArrowLeft /> {isEdit ? 'Employee details' : 'Employee directory'}
          </Link>
          <div className="eyebrow mt-3">{isEdit ? 'UPDATE RECORD' : 'NEW TEAM MEMBER'}</div>
          <h1>{isEdit ? 'Edit employee' : 'Add employee'}</h1>
          <p className="muted-copy">
            {isEdit
              ? 'Update employee information and employment details.'
              : 'Create an employee profile for your organization.'}
          </p>
        </div>
      </div>
      <ErrorAlert error={error} />
      {(!isEdit || initialValue) && (
        <div className="form-card">
          <EmployeeForm
            key={initialValue?.id || 'new-employee'}
            initialValue={initialValue || undefined}
            submitLabel={isEdit ? 'Save changes' : 'Create employee'}
            onSubmit={save}
          />
        </div>
      )}
      {isEdit && error && !initialValue && (
        <Button as={Link} to="/employees" variant="light">
          Back to employees
        </Button>
      )}
    </div>
  );
}
