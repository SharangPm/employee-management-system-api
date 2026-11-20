namespace EmployeeManagement.API.Configuration;

public sealed record JwtSettings(string Secret, string Issuer, string Audience, int ExpiryMinutes);
