export function formatDate(value) {
  if (!value) return '...';
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? '...'
    : new Intl.DateTimeFormat(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
      }).format(date);
}

export function formatMoney(value) {
  const amount = Number(value);
  if (!Number.isFinite(amount)) return '...';
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 0,
  }).format(amount);
}

export function getInitials(firstName = '', lastName = '') {
  return `${firstName.slice(0, 1)}${lastName.slice(0, 1)}`.toUpperCase() || 'E';
}
