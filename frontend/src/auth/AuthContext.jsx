import { useCallback, useEffect, useMemo, useState } from 'react';
import { login as loginRequest } from '../api/auth';
import { clearStoredAuth, readStoredAuth, storeAuth } from '../api/client';
import { AuthContext } from './contextDefinition';

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState(() => readStoredAuth());

  const logout = useCallback(() => {
    clearStoredAuth();
    setAuth(null);
  }, []);

  useEffect(() => {
    const onUnauthorized = () => logout();
    window.addEventListener('employee-management:unauthorized', onUnauthorized);
    return () => window.removeEventListener('employee-management:unauthorized', onUnauthorized);
  }, [logout]);

  const signIn = useCallback(async (email, password) => {
    const result = await loginRequest({ email, password });
    const nextAuth = {
      accessToken: result.accessToken,
      email: result.email,
      role: result.role,
      expiresAt: result.expiresAt,
    };
    storeAuth(nextAuth);
    setAuth(nextAuth);
    return nextAuth;
  }, []);

  const value = useMemo(
    () => ({
      auth,
      isAuthenticated: Boolean(auth?.accessToken),
      role: auth?.role,
      email: auth?.email,
      signIn,
      logout,
    }),
    [auth, signIn, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
