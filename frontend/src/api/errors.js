export function getApiErrorMessage(error, fallback = 'Something went wrong. Please try again.') {
  const data = error?.response?.data;
  if (data?.errors && typeof data.errors === 'object') {
    return Object.entries(data.errors)
      .flatMap(([field, messages]) =>
        Array.isArray(messages) ? messages.map((message) => `${field}: ${message}`) : [],
      )
      .join(' ');
  }
  return data?.message || data?.title || error?.message || fallback;
}

export function getFieldErrors(error) {
  const errors = error?.response?.data?.errors;
  if (!errors || typeof errors !== 'object') return {};
  return Object.fromEntries(
    Object.entries(errors).map(([key, value]) => [
      key.split('.').pop().toLowerCase(),
      Array.isArray(value) ? value[0] : String(value),
    ]),
  );
}
