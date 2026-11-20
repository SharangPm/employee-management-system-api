import axios from 'axios';

const TOKEN_KEY = 'employee-management.auth';
const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api';

export const api = axios.create({
  baseURL: configuredBaseUrl.replace(/\/$/, ''),
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

export function readStoredAuth() {
  try {
    const raw = sessionStorage.getItem(TOKEN_KEY);
    if (!raw) return null;
    const auth = JSON.parse(raw);
    if (!auth.accessToken || !auth.expiresAt || Date.parse(auth.expiresAt) <= Date.now()) {
      sessionStorage.removeItem(TOKEN_KEY);
      return null;
    }
    return auth;
  } catch {
    sessionStorage.removeItem(TOKEN_KEY);
    return null;
  }
}

export function storeAuth(auth) {
  sessionStorage.setItem(TOKEN_KEY, JSON.stringify(auth));
}

export function clearStoredAuth() {
  sessionStorage.removeItem(TOKEN_KEY);
}

api.interceptors.request.use((config) => {
  const auth = readStoredAuth();
  if (auth?.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      clearStoredAuth();
      window.dispatchEvent(new CustomEvent('employee-management:unauthorized'));
    }
    return Promise.reject(error);
  },
);
