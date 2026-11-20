import axios from 'axios';

const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';
const normalizedBaseUrl = configuredBaseUrl.replace(/\/+$/, '');
const apiBaseUrl = normalizedBaseUrl.endsWith('/api')
  ? normalizedBaseUrl
  : `${normalizedBaseUrl}/api`;

export const api = axios.create({
  baseURL: apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});
