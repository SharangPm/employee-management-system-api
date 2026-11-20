import { useEffect, useState } from 'react';
import Card from 'react-bootstrap/Card';
import { FiBriefcase, FiCalendar, FiMail, FiPhone, FiShield, FiUser } from 'react-icons/fi';
import { getMyProfile } from '../api/employees';
import { getApiErrorMessage } from '../api/errors';
import { ErrorAlert } from '../components/FeedbackAlerts';
import { LoadingScreen } from '../components/LoadingScreen';
import { formatDate, formatMoney, getInitials } from '../utils/formatters';
import { useAuth } from '../auth/useAuth';

export function EmployeeDashboardPage() {
  const { email } = useAuth();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getMyProfile()
      .then(setProfile)
      .catch((requestError) =>
        setError(getApiErrorMessage(requestError, 'Unable to load your employee profile.')),
      )
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <LoadingScreen label="Loading your profile..." />;

  return (
    <div className="page-stack">
      <div className="page-heading-row">
        <div>
          <div className="eyebrow">EMPLOYEE WORKSPACE</div>
          <h1>Welcome back</h1>
          <p className="muted-copy">Your employee information and employment details.</p>
        </div>
      </div>
      <ErrorAlert error={error} />
      {!profile && !error && (
        <div className="empty-state content-card">
          <div className="empty-state-icon">
            <FiUser />
          </div>
          <strong>Your profile is not linked yet</strong>
          <span>Contact an administrator to link your work account to an employee record.</span>
        </div>
      )}
      {profile && (
        <>
          <Card className="employee-welcome-card border-0">
            <Card.Body>
              <div className="profile-hero-avatar">
                {getInitials(profile.firstName, profile.lastName)}
              </div>
              <div>
                <div className="eyebrow light-eyebrow">YOUR PROFILE</div>
                <h2>
                  {profile.firstName} {profile.lastName}
                </h2>
                <p>
                  {profile.designation} <span>...</span> {profile.department}
                </p>
              </div>
              <span
                className={`status-pill ${profile.isActive ? 'status-active' : 'status-inactive'}`}
              >
                {profile.isActive ? 'Active employee' : 'Inactive'}
              </span>
            </Card.Body>
          </Card>
          <div className="profile-overview-grid">
            <Card className="content-card border-0">
              <Card.Body>
                <div className="section-heading-row">
                  <div>
                    <h2>Personal details</h2>
                    <p>Information on your employee record.</p>
                  </div>
                  <FiUser className="section-heading-icon" />
                </div>
                <div className="employee-detail-list">
                  <ProfileDetail
                    icon={<FiMail />}
                    label="Work email"
                    value={profile.email || email}
                  />
                  <ProfileDetail
                    icon={<FiPhone />}
                    label="Phone"
                    value={profile.phoneNumber || 'Not provided'}
                  />
                  <ProfileDetail
                    icon={<FiCalendar />}
                    label="Date joined"
                    value={formatDate(profile.dateOfJoining)}
                  />
                  <ProfileDetail icon={<FiBriefcase />} label="Employee ID" value={profile.id} />
                </div>
              </Card.Body>
            </Card>
            <Card className="content-card border-0">
              <Card.Body>
                <div className="section-heading-row">
                  <div>
                    <h2>Employment</h2>
                    <p>Your current role and compensation.</p>
                  </div>
                  <FiShield className="section-heading-icon" />
                </div>
                <div className="employment-highlight">
                  <span>DEPARTMENT</span>
                  <strong>{profile.department}</strong>
                </div>
                <div className="employment-highlight">
                  <span>DESIGNATION</span>
                  <strong>{profile.designation}</strong>
                </div>
                <div className="employment-highlight">
                  <span>ANNUAL SALARY</span>
                  <strong>{formatMoney(profile.salary)}</strong>
                </div>
                <div className="read-only-note">
                  <FiShield />
                  <span>For profile changes, please contact your administrator.</span>
                </div>
              </Card.Body>
            </Card>
          </div>
        </>
      )}
    </div>
  );
}

function ProfileDetail({ icon, label, value }) {
  return (
    <div className="employee-detail-row">
      <span className="employee-detail-icon">{icon}</span>
      <div>
        <small>{label}</small>
        <strong>{value || '...'}</strong>
      </div>
    </div>
  );
}
