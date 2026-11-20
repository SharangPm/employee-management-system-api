import { api } from './client';

export async function getEmployees(params) {
  const response = await api.get('/employees', { params });
  return response.data;
}

export async function getEmployee(id) {
  const response = await api.get(`/employees/${encodeURIComponent(id)}`);
  return response.data;
}

export async function createEmployee(employee) {
  const response = await api.post('/employees', employee);
  return response.data;
}

export async function updateEmployee(id, employee) {
  const response = await api.put(`/employees/${encodeURIComponent(id)}`, employee);
  return response.data;
}

export async function deleteEmployee(id) {
  await api.delete(`/employees/${encodeURIComponent(id)}`);
}

export async function getMyProfile() {
  const response = await api.get('/employees/me');
  return response.data;
}
