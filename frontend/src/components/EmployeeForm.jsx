import { useState } from 'react';
import Button from 'react-bootstrap/Button';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import Row from 'react-bootstrap/Row';
import { FiSave } from 'react-icons/fi';
import { getApiErrorMessage, getFieldErrors } from '../api/errors';

const emptyEmployee = {
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  department: '',
  designation: '',
  salary: '',
  dateOfJoining: '',
  isActive: true,
};

export function EmployeeForm({ initialValue, submitLabel, onSubmit }) {
  const [values, setValues] = useState({
    ...emptyEmployee,
    ...initialValue,
    salary: initialValue?.salary ?? '',
  });
  const [fieldErrors, setFieldErrors] = useState({});
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  function update(event) {
    const { name, value, checked, type } = event.target;
    setValues((current) => ({ ...current, [name]: type === 'checkbox' ? checked : value }));
    setFieldErrors((current) => ({ ...current, [name]: undefined }));
  }

  async function submit(event) {
    event.preventDefault();
    setBusy(true);
    setError('');
    setFieldErrors({});
    try {
      await onSubmit({
        ...values,
        phoneNumber: values.phoneNumber.trim() || null,
        salary: Number(values.salary),
        dateOfJoining: values.dateOfJoining
          ? new Date(`${values.dateOfJoining}T00:00:00.000Z`).toISOString()
          : null,
      });
    } catch (submitError) {
      setFieldErrors(getFieldErrors(submitError));
      setError(getApiErrorMessage(submitError, 'Check the form and try again.'));
    } finally {
      setBusy(false);
    }
  }

  const field = (name) => ({ isInvalid: Boolean(fieldErrors[name]) });
  const feedback = (name) => (
    <Form.Control.Feedback type="invalid">{fieldErrors[name]}</Form.Control.Feedback>
  );

  return (
    <Form onSubmit={submit} noValidate>
      {error && (
        <div className="form-error-summary" role="alert">
          {error}
        </div>
      )}
      <section className="form-section">
        <div className="form-section-heading">
          <span>01</span>
          <div>
            <h2>Personal information</h2>
            <p>Basic details used for employee records.</p>
          </div>
        </div>
        <Row className="g-3">
          <Form.Group as={Col} md={6} controlId="firstName">
            <Form.Label>
              First name <span className="required-mark">*</span>
            </Form.Label>
            <Form.Control
              name="firstName"
              value={values.firstName}
              onChange={update}
              required
              maxLength={100}
              placeholder="e.g. Jordan"
              {...field('firstname')}
            />
            {feedback('firstname')}
          </Form.Group>
          <Form.Group as={Col} md={6} controlId="lastName">
            <Form.Label>
              Last name <span className="required-mark">*</span>
            </Form.Label>
            <Form.Control
              name="lastName"
              value={values.lastName}
              onChange={update}
              required
              maxLength={100}
              placeholder="e.g. Lee"
              {...field('lastname')}
            />
            {feedback('lastname')}
          </Form.Group>
          <Form.Group as={Col} md={6} controlId="email">
            <Form.Label>
              Work email <span className="required-mark">*</span>
            </Form.Label>
            <Form.Control
              type="email"
              name="email"
              value={values.email}
              onChange={update}
              required
              maxLength={254}
              placeholder="jordan@company.com"
              {...field('email')}
            />
            {feedback('email')}
          </Form.Group>
          <Form.Group as={Col} md={6} controlId="phoneNumber">
            <Form.Label>Phone number</Form.Label>
            <Form.Control
              type="tel"
              name="phoneNumber"
              value={values.phoneNumber ?? ''}
              onChange={update}
              maxLength={30}
              placeholder="+1 555 0100"
              {...field('phonenumber')}
            />
            {feedback('phonenumber')}
          </Form.Group>
        </Row>
      </section>
      <section className="form-section">
        <div className="form-section-heading">
          <span>02</span>
          <div>
            <h2>Role &amp; compensation</h2>
            <p>Department, position and employment details.</p>
          </div>
        </div>
        <Row className="g-3">
          <Form.Group as={Col} md={6} controlId="department">
            <Form.Label>
              Department <span className="required-mark">*</span>
            </Form.Label>
            <Form.Control
              name="department"
              value={values.department}
              onChange={update}
              required
              maxLength={100}
              placeholder="e.g. Product"
              {...field('department')}
            />
            {feedback('department')}
          </Form.Group>
          <Form.Group as={Col} md={6} controlId="designation">
            <Form.Label>
              Designation <span className="required-mark">*</span>
            </Form.Label>
            <Form.Control
              name="designation"
              value={values.designation}
              onChange={update}
              required
              maxLength={100}
              placeholder="e.g. Product Designer"
              {...field('designation')}
            />
            {feedback('designation')}
          </Form.Group>
          <Form.Group as={Col} md={6} controlId="salary">
            <Form.Label>
              Annual salary <span className="required-mark">*</span>
            </Form.Label>
            <div className="salary-input">
              <span>$</span>
              <Form.Control
                type="number"
                name="salary"
                value={values.salary}
                onChange={update}
                min="0.01"
                step="0.01"
                required
                placeholder="75000"
                {...field('salary')}
              />
            </div>
            {feedback('salary')}
          </Form.Group>
          <Form.Group as={Col} md={6} controlId="dateOfJoining">
            <Form.Label>
              Date of joining <span className="required-mark">*</span>
            </Form.Label>
            <Form.Control
              type="date"
              name="dateOfJoining"
              value={values.dateOfJoining?.slice(0, 10) ?? ''}
              onChange={update}
              required
              {...field('dateofjoining')}
            />
            {feedback('dateofjoining')}
          </Form.Group>
        </Row>
        <Form.Check
          className="active-check mt-4"
          type="switch"
          name="isActive"
          id="isActive"
          label="Employee is currently active"
          checked={Boolean(values.isActive)}
          onChange={update}
        />
      </section>
      <div className="form-actions">
        <Button type="submit" disabled={busy} className="primary-action">
          <FiSave />
          {busy ? 'Saving...' : submitLabel}
        </Button>
      </div>
    </Form>
  );
}
