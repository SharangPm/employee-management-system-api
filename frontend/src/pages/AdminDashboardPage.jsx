import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import Button from 'react-bootstrap/Button';
import Card from 'react-bootstrap/Card';
import {
  FiArrowUpRight,
  FiBriefcase,
  FiClock,
  FiPlus,
  FiTrendingUp,
  FiUsers,
} from 'react-icons/fi';
import { getEmployees } from '../api/employees';
import { getApiErrorMessage } from '../api/errors';
import { ErrorAlert } from '../components/FeedbackAlerts';
import { InlineLoading } from '../components/LoadingScreen';
import { formatDate, getInitials } from '../utils/formatters';

const EMPTY_EMPLOYEES = [];

export function AdminDashboardPage() {
  const [pageData, setPageData] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [currentTime] = useState(() => Date.now());

  useEffect(() => {
    getEmployees({ page: 1, pageSize: 100 })
      .then(setPageData)
      .catch((requestError) =>
        setError(getApiErrorMessage(requestError, 'Unable to load employee data.')),
      )
      .finally(() => setLoading(false));
  }, []);

  const employees = pageData?.items ?? EMPTY_EMPLOYEES;
  const departments = useMemo(
    () => new Set(employees.map((employee) => employee.department).filter(Boolean)).size,
    [employees],
  );
  const newJoiners = useMemo(
    () =>
      [...employees]
        .sort((a, b) => new Date(b.dateOfJoining) - new Date(a.dateOfJoining))
        .slice(0, 5),
    [employees],
  );

  return (
    <div className="page-stack">
      <div className="page-heading-row">
        <div>
          <div className="eyebrow">TEAM OVERVIEW</div>
          <h1>Good morning, Admin</h1>
          <p className="muted-copy">Here's what's happening across your organization.</p>
        </div>
        <Button as={Link} to="/employees/new" className="primary-action">
          <FiPlus /> Add employee
        </Button>
      </div>
      <ErrorAlert error={error} />
      {loading ? (
        <InlineLoading label="Loading team overview..." />
      ) : (
        <>
          <div className="stats-grid">
            <StatCard
              icon={<FiUsers />}
              label="Total employees"
              value={pageData?.total ?? 0}
              note="Across your organization"
              tone="blue"
            />
            <StatCard
              icon={<FiTrendingUp />}
              label="Active on this page"
              value={employees.filter((employee) => employee.isActive).length}
              note={`From ${employees.length} loaded records`}
              tone="green"
            />
            <StatCard
              icon={<FiBriefcase />}
              label="Departments"
              value={departments}
              note="Represented in loaded records"
              tone="violet"
            />
            <StatCard
              icon={<FiClock />}
              label="Recently joined"
              value={
                employees.filter(
                  (employee) =>
                    currentTime - new Date(employee.dateOfJoining).getTime() < 90 * 86400000,
                ).length
              }
              note="Within the last 90 days, loaded records"
              tone="amber"
            />
          </div>
          <Card className="content-card border-0">
            <Card.Body>
              <div className="section-heading-row">
                <div>
                  <h2>Recent team members</h2>
                  <p>Latest joining dates among the loaded records.</p>
                </div>
                <Button as={Link} to="/employees" variant="link" className="text-link">
                  View directory <FiArrowUpRight />
                </Button>
              </div>
              {newJoiners.length ? (
                <div className="recent-list">
                  {newJoiners.map((employee) => (
                    <Link to={`/employees/${employee.id}`} className="recent-row" key={employee.id}>
                      <div className="person-cell">
                        <div className="person-avatar">
                          {getInitials(employee.firstName, employee.lastName)}
                        </div>
                        <div>
                          <strong>
                            {employee.firstName} {employee.lastName}
                          </strong>
                          <span>{employee.designation}</span>
                        </div>
                      </div>
                      <span className="department-chip">{employee.department}</span>
                      <span className="recent-date">
                        Joined {formatDate(employee.dateOfJoining)}
                      </span>
                      <span
                        className={`status-pill ${employee.isActive ? 'status-active' : 'status-inactive'}`}
                      >
                        {employee.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </Link>
                  ))}
                </div>
              ) : (
                <div className="empty-state">
                  <div className="empty-state-icon">
                    <FiUsers />
                  </div>
                  <strong>No employees yet</strong>
                  <span>Add your first team member to get started.</span>
                </div>
              )}
            </Card.Body>
          </Card>
        </>
      )}
    </div>
  );
}

function StatCard({ icon, label, value, note, tone }) {
  return (
    <Card className="stat-card border-0">
      <Card.Body>
        <div className="stat-card-top">
          <span>{label}</span>
          <span className={`stat-icon tone-${tone}`}>{icon}</span>
        </div>
        <strong className="stat-value">{value}</strong>
        <span className="stat-note">{note}</span>
      </Card.Body>
    </Card>
  );
}
