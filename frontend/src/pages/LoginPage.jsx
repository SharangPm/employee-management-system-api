import { useLocation, useNavigate } from 'react-router-dom';
import Button from 'react-bootstrap/Button';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import { FiArrowRight, FiBriefcase, FiLock, FiUsers } from 'react-icons/fi';
import { useAuth } from '../auth/useAuth';
import { getApiErrorMessage } from '../api/errors';
import { useState } from 'react';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const { signIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  async function submit(event) {
    event.preventDefault();
    setBusy(true);
    setError('');
    try {
      const auth = await signIn(email.trim(), password);
      const intendedPath = location.state?.from?.pathname;
      navigate(intendedPath || (auth.role === 'Admin' ? '/admin' : '/employee'), { replace: true });
    } catch (loginError) {
      setError(
        getApiErrorMessage(loginError, 'Unable to sign in. Check your connection and try again.'),
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="login-page">
      <div className="login-visual">
        <div className="login-brand">
          <span className="brand-mark">
            <FiBriefcase />
          </span>
          <strong>PeopleDesk</strong>
        </div>
        <div className="login-story">
          <div className="eyebrow light-eyebrow">
            <span />
            PEOPLE OPERATIONS
          </div>
          <h1>
            Good work
            <br />
            starts with
            <br />
            <em>great people.</em>
          </h1>
          <p>A clear view of your team, all in one place.</p>
          <div className="login-visual-stats">
            <div>
              <FiUsers />
              <span>Team directory</span>
            </div>
            <div>
              <FiLock />
              <span>Secure access</span>
            </div>
          </div>
        </div>
        <div className="login-visual-footer">EMPLOYEE MANAGEMENT</div>
      </div>
      <div className="login-panel">
        <div className="login-form-wrap">
          <div className="login-mobile-brand">
            <span className="brand-mark">
              <FiBriefcase />
            </span>
            <strong>PeopleDesk</strong>
          </div>
          <div className="eyebrow">WELCOME BACK</div>
          <h2>Sign in to your account</h2>
          <p className="muted-copy">Use the work account linked to your employee profile.</p>
          {error && (
            <div className="form-error-summary" role="alert">
              {error}
            </div>
          )}
          <Card className="login-card border-0">
            <Card.Body>
              <Form onSubmit={submit}>
                <Form.Group className="mb-3" controlId="loginEmail">
                  <Form.Label>Work email</Form.Label>
                  <Form.Control
                    type="email"
                    autoComplete="username"
                    autoFocus
                    required
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    placeholder="you@company.com"
                  />
                </Form.Group>
                <Form.Group className="mb-4" controlId="loginPassword">
                  <div className="d-flex justify-content-between">
                    <Form.Label>Password</Form.Label>
                  </div>
                  <Form.Control
                    type="password"
                    autoComplete="current-password"
                    required
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    placeholder="Enter your password"
                  />
                </Form.Group>
                <Button type="submit" className="primary-action w-100" disabled={busy}>
                  {busy ? (
                    'Signing in...'
                  ) : (
                    <>
                      Continue <FiArrowRight />
                    </>
                  )}
                </Button>
              </Form>
            </Card.Body>
          </Card>
          <div className="login-help">Need access? Contact your administrator.</div>
          <div className="login-security">
            <FiLock /> Your connection is protected with secure authentication.
          </div>
        </div>
      </div>
    </div>
  );
}
