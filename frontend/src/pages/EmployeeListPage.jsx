import { useCallback, useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import Button from 'react-bootstrap/Button';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import Pagination from 'react-bootstrap/Pagination';
import Table from 'react-bootstrap/Table';
import { FiEye, FiFilter, FiPlus, FiSearch, FiTrash2, FiUser, FiEdit2 } from 'react-icons/fi';
import { deleteEmployee, getEmployees } from '../api/employees';
import { getApiErrorMessage } from '../api/errors';
import { ConfirmModal } from '../components/ConfirmModal';
import { ErrorAlert } from '../components/FeedbackAlerts';
import { InlineLoading } from '../components/LoadingScreen';
import { formatDate, formatMoney, getInitials } from '../utils/formatters';

const PAGE_SIZE = 10;

export function EmployeeListPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const page = Math.max(1, Number(searchParams.get('page')) || 1);
  const [searchInput, setSearchInput] = useState(searchParams.get('search') || '');
  const [departmentInput, setDepartmentInput] = useState(searchParams.get('department') || '');
  const [activeInput, setActiveInput] = useState(searchParams.get('active') || '');
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedEmployee, setSelectedEmployee] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const loadEmployees = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const params = { page, pageSize: PAGE_SIZE };
      const search = searchParams.get('search');
      const department = searchParams.get('department');
      const active = searchParams.get('active');
      if (search) params.search = search;
      if (department) params.department = department;
      if (active !== null && active !== '') params.active = active;
      setData(await getEmployees(params));
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, 'Unable to load employees.'));
    } finally {
      setLoading(false);
    }
  }, [page, searchParams]);

  // Loading state intentionally follows this request, which is triggered by route query changes.
  useEffect(() => {
    // oxlint-disable-next-line react/set-state-in-effect
    loadEmployees();
  }, [loadEmployees]);

  function applyFilters(event) {
    event.preventDefault();
    const next = {};
    if (searchInput.trim()) next.search = searchInput.trim();
    if (departmentInput.trim()) next.department = departmentInput.trim();
    if (activeInput) next.active = activeInput;
    setSearchParams(next);
  }

  async function confirmDelete() {
    if (!selectedEmployee) return;
    setDeleting(true);
    setError('');
    try {
      await deleteEmployee(selectedEmployee.id);
      setSelectedEmployee(null);
      await loadEmployees();
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, 'Unable to delete this employee.'));
      setSelectedEmployee(null);
    } finally {
      setDeleting(false);
    }
  }

  const totalPages = Math.max(1, Math.ceil((data?.total ?? 0) / PAGE_SIZE));
  const employees = data?.items ?? [];

  return (
    <div className="page-stack">
      <div className="page-heading-row">
        <div>
          <div className="eyebrow">PEOPLE DIRECTORY</div>
          <h1>Employees</h1>
          <p className="muted-copy">Manage your team, roles and employee records.</p>
        </div>
        <Button as={Link} to="/employees/new" className="primary-action">
          <FiPlus /> Add employee
        </Button>
      </div>
      <ErrorAlert error={error} />
      <Card className="filter-card border-0">
        <Card.Body>
          <Form onSubmit={applyFilters} className="filter-form">
            <Form.Group className="search-field" controlId="employeeSearch">
              <Form.Label className="visually-hidden">Search by name</Form.Label>
              <FiSearch />
              <Form.Control
                value={searchInput}
                onChange={(event) => setSearchInput(event.target.value)}
                placeholder="Search by first or last name"
              />
            </Form.Group>
            <Form.Group className="department-field" controlId="departmentFilter">
              <Form.Label className="visually-hidden">Department</Form.Label>
              <Form.Control
                value={departmentInput}
                onChange={(event) => setDepartmentInput(event.target.value)}
                placeholder="Department"
              />
            </Form.Group>
            <Form.Group className="active-field" controlId="activeFilter">
              <Form.Label className="visually-hidden">Status</Form.Label>
              <Form.Select
                value={activeInput}
                onChange={(event) => setActiveInput(event.target.value)}
              >
                <option value="">Any status</option>
                <option value="true">Active</option>
                <option value="false">Inactive</option>
              </Form.Select>
            </Form.Group>
            <Button type="submit" variant="light" className="filter-button">
              <FiFilter /> Filter
            </Button>
          </Form>
        </Card.Body>
      </Card>
      <Card className="content-card border-0">
        <Card.Body className="p-0">
          <div className="table-title-row">
            <div>
              <h2>Team directory</h2>
              <p>{data?.total ?? 0} employee records</p>
            </div>
            <span className="table-page-label">
              Page {page} of {totalPages}
            </span>
          </div>
          {loading ? (
            <div className="table-loading">
              <InlineLoading label="Loading employee records..." />
            </div>
          ) : employees.length ? (
            <>
              <div className="table-responsive">
                <Table hover className="employee-table align-middle mb-0">
                  <thead>
                    <tr>
                      <th>Employee</th>
                      <th>Department</th>
                      <th>Designation</th>
                      <th>Annual salary</th>
                      <th>Joined</th>
                      <th>Status</th>
                      <th className="text-end">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {employees.map((employee) => (
                      <tr key={employee.id}>
                        <td>
                          <div className="person-cell">
                            <div className="person-avatar">
                              {getInitials(employee.firstName, employee.lastName)}
                            </div>
                            <div>
                              <strong>
                                {employee.firstName} {employee.lastName}
                              </strong>
                              <span>{employee.email}</span>
                            </div>
                          </div>
                        </td>
                        <td>{employee.department}</td>
                        <td>{employee.designation}</td>
                        <td>{formatMoney(employee.salary)}</td>
                        <td>{formatDate(employee.dateOfJoining)}</td>
                        <td>
                          <span
                            className={`status-pill ${employee.isActive ? 'status-active' : 'status-inactive'}`}
                          >
                            {employee.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td>
                          <div className="table-actions">
                            <Button
                              as={Link}
                              to={`/employees/${employee.id}`}
                              variant="light"
                              className="action-icon"
                              aria-label={`View ${employee.firstName} ${employee.lastName}`}
                            >
                              <FiEye />
                            </Button>
                            <Button
                              as={Link}
                              to={`/employees/${employee.id}/edit`}
                              variant="light"
                              className="action-icon"
                              aria-label={`Edit ${employee.firstName} ${employee.lastName}`}
                            >
                              <FiEdit2 />
                            </Button>
                            <Button
                              variant="light"
                              className="action-icon danger-action"
                              aria-label={`Delete ${employee.firstName} ${employee.lastName}`}
                              onClick={() => setSelectedEmployee(employee)}
                            >
                              <FiTrash2 />
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </div>
              <div className="table-footer">
                <span>
                  Showing {(page - 1) * PAGE_SIZE + 1}–{Math.min(page * PAGE_SIZE, data.total)} of{' '}
                  {data.total}
                </span>
                <Pagination className="mb-0">
                  {' '}
                  <Pagination.Prev
                    disabled={page <= 1}
                    onClick={() =>
                      setSearchParams((current) => {
                        const next = new URLSearchParams(current);
                        next.set('page', String(page - 1));
                        return next;
                      })
                    }
                  />
                  {page > 1 && (
                    <Pagination.Item
                      onClick={() =>
                        setSearchParams((current) => {
                          const next = new URLSearchParams(current);
                          next.set('page', String(page - 1));
                          return next;
                        })
                      }
                    >
                      {page - 1}
                    </Pagination.Item>
                  )}
                  <Pagination.Item active>{page}</Pagination.Item>
                  {page < totalPages && (
                    <Pagination.Item
                      onClick={() =>
                        setSearchParams((current) => {
                          const next = new URLSearchParams(current);
                          next.set('page', String(page + 1));
                          return next;
                        })
                      }
                    >
                      {page + 1}
                    </Pagination.Item>
                  )}
                  <Pagination.Next
                    disabled={page >= totalPages}
                    onClick={() =>
                      setSearchParams((current) => {
                        const next = new URLSearchParams(current);
                        next.set('page', String(page + 1));
                        return next;
                      })
                    }
                  />
                </Pagination>
              </div>
            </>
          ) : (
            <div className="empty-state">
              <div className="empty-state-icon">
                <FiUser />
              </div>
              <strong>No employees found</strong>
              <span>Try changing the filters or add an employee.</span>
              <Button as={Link} to="/employees/new" variant="link" className="text-link mt-2">
                Add employee <FiPlus />
              </Button>
            </div>
          )}
        </Card.Body>
      </Card>
      <ConfirmModal
        show={Boolean(selectedEmployee)}
        title="Delete employee?"
        message={
          selectedEmployee
            ? `This will permanently remove ${selectedEmployee.firstName} ${selectedEmployee.lastName} from the employee directory.`
            : ''
        }
        confirmLabel="Delete employee"
        busy={deleting}
        onCancel={() => !deleting && setSelectedEmployee(null)}
        onConfirm={confirmDelete}
      />
    </div>
  );
}
