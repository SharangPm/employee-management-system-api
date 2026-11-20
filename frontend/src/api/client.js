import axios from 'axios';

const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api';

export const api = axios.create({
  baseURL: configuredBaseUrl.replace(/\/$/, ''),
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});
