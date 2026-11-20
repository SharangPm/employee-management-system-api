# Employee Management API

ASP.NET Core 8 REST API with MongoDB Atlas support, JWT bearer authentication, role based authorization, Swagger, and xUnit tests.

## Features and routes

- `POST /api/auth/register` — public account registration; the server always assigns the Employee role.
- `POST /api/auth/login` — validates credentials and returns a signed JWT.
- `POST /api/auth/admins` — Admin only; creates another Admin.
- `POST /api/auth/employees/{employeeId}/link-account` — Admin only; links an Employee login to a profile after the Admin verifies the employee's identity.
- `GET /api/employees/me` — Employee only; resolves the employee record through the immutable authenticated user ID link.
- `GET /api/employees/{id}` — Admin can retrieve any record; Employee can retrieve only their linked record.
- `GET /api/employees` — Admin only; search, department/active filters, pagination.
- `POST /api/employees`, `PUT /api/employees/{id}`, `DELETE /api/employees/{id}` — Admin only.

Passwords use ASP.NET Core's PBKDF2 `PasswordHasher`. Registration cannot choose a role or profile. An Admin must explicitly link a registered Employee account to an employee record after identity verification; a user cannot claim a profile by supplying its email. MongoDB unique indexes protect normalized employee email, user email, and linked employee account IDs. API errors have a consistent JSON shape; validation errors include field-level details. CORS is restricted to configured origins.

## Environment variables

Copy `.env.example` to `.env` for local Docker use. Never commit `.env`, production credentials, or secret values. `.gitignore` excludes `.env` and `.env.*`, while allowing the placeholder-only `.env.example`.

| Variable | Purpose | Replace before deployment |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Set `Production` in hosting. This disables development bootstrap and enables HSTS/HTTPS redirection. |
| `ASPNETCORE_URLS` | Kestrel bind address | Container default is port 8080; match the host's required port. |
| `TRUSTED_PROXY_IPS` | Semicolon-separated IP addresses allowed to supply forwarded headers | Required in Production. Set exact ingress/proxy IPs for TLS-terminating hosts; forwarded headers from other hosts are ignored. |
| `MONGODB_URI` | MongoDB connection URI | Replace with the Atlas SRV URI and database user credentials. Keep private. Production requires TLS. |
| `MONGODB_DATABASE_NAME` | Database name | Choose the production database name. |
| `JWT_SECRET` | HMAC JWT signing key (minimum 32 bytes) | Generate high-entropy material, e.g. `openssl rand -base64 48`; store as a hosting secret. Placeholder values are rejected. |
| `JWT_ISSUER` | Expected token issuer | Set a stable API identifier. |
| `JWT_AUDIENCE` | Expected token audience | Set the clients allowed to use these tokens. |
| `JWT_EXPIRY_MINUTES` | Token lifetime (5–1440 minutes) | Choose a short lifetime compatible with your client session design. |
| `INITIAL_ADMIN_EMAIL` | Optional Development-only initial Admin | Set a local-only email; omit in Production. |
| `INITIAL_ADMIN_PASSWORD` | Optional Development-only initial Admin | Set a unique strong password; omit in Production. |
| `ADMIN_PROVISIONING_EMAIL` | One-time production CLI Admin account | Supply only to the restricted provisioning job; never set on the running API service. |
| `ADMIN_PROVISIONING_PASSWORD` | One-time production CLI Admin password | Supply as a temporary secret to the provisioning job and remove it immediately afterward. |
| `CORS_ALLOWED_ORIGINS` | Semicolon-separated browser origins | Replace with exact frontend `https://` origins; no wildcard or path. Required in Production. |

## Local development

Prerequisites: Docker Desktop and .NET 8 SDK.

1. Copy `.env.example` to `.env` and replace every `CHANGE_ME` value. Generate a fresh local JWT key and a unique local Admin password.
2. Start API and local MongoDB: `docker compose --profile local up --build`.
3. Open `http://localhost:8080/swagger`. MongoDB's development port is bound only to `127.0.0.1`.

For a local process, start MongoDB separately, set the variables from `.env.example` in your shell, then run `dotnet run --project src/EmployeeManagement.API`. The API pings MongoDB and creates indexes at startup. Invalid configuration or unavailable database stops startup rather than silently using a default database.

Development Admin bootstrap runs only when `ASPNETCORE_ENVIRONMENT=Development` and both `INITIAL_ADMIN_*` values are supplied. It creates the account only if that email is not already registered. Production rejects either development bootstrap variable.

## MongoDB Atlas setup

1. In Atlas, create a project and cluster in a region near the API host.
2. Create a dedicated database user with a generated password and only read/write access to the application database. Do not use your Atlas account password.
3. In **Network Access**, allow only the API host's stable outbound IP/CIDR. If egress IPs are dynamic, use private networking/peering or a controlled egress gateway; avoid `0.0.0.0/0`.
4. Select **Connect** ? **Drivers** ? **C#/.NET**. Copy the `mongodb+srv://` connection string, replace `<password>` with the database user's password, and URL-encode special characters in credentials.
5. Store the URI in the hosting provider's secret manager as `MONGODB_URI` and set `MONGODB_DATABASE_NAME`. Never paste credentials into source control, tickets, logs, or API responses.

## Docker and cloud deployment

The root Dockerfile uses a .NET 8 SDK build stage and a smaller .NET 8 ASP.NET runtime stage; the final container runs as a non-root user. The default API container does not depend on a local MongoDB service. `docker-compose.yml` provides an optional `local` MongoDB profile for development, with the database port bound to loopback only.

1. Build: `docker build -t employee-management-api:latest .`.
2. Push the image to a private registry or configure your provider to build it from this repository.
3. Create a Linux container/web service on a .NET 8 capable host such as Azure Container Apps, Azure App Service for Containers, AWS App Runner, or Google Cloud Run.
4. Configure port 8080 and set the production variables through the provider's environment/secret settings. Set `ASPNETCORE_ENVIRONMENT=Production`. Do not configure `INITIAL_ADMIN_EMAIL` or `INITIAL_ADMIN_PASSWORD` on the web service.
5. Configure the provider's TLS certificate and HTTPS ingress; redirect HTTP at the ingress. If TLS terminates at a proxy, enable forwarded headers only for that trusted hosting ingress. Set `CORS_ALLOWED_ORIGINS` to the exact frontend origin.
6. Use a separate, one-time provisioning job to create the first Admin. Provide the normal required production settings plus temporary `ADMIN_PROVISIONING_EMAIL` and `ADMIN_PROVISIONING_PASSWORD` to that job only. For a built image and Atlas reachability, run: `docker run --rm --env-file production-provisioning.env --entrypoint dotnet employee-management-api:latest EmployeeManagement.API.dll --provision-admin`. The temporary env file must be outside source control, readable only by the operator, and deleted after the job. The command runs only in Production, requires a strong password, stores only its hash, refuses if an Admin already exists, and uses a MongoDB lock to prevent competing initial provisioning attempts. Do not attach provisioning credentials to the public API service.
7. Deploy the API image. Startup checks Atlas reachability and required indexes. Check platform health/logs if startup fails; logs do not include configured credentials.
8. Open `https://<provider-assigned-host>/swagger` using the real URL assigned by your provider. No deployment URL is created by this repository change. Disable or restrict public Swagger if your production policy requires it.

## Verify authentication and roles

1. Call `POST /api/auth/login` with the provisioned Admin and authorize Swagger with its bearer token.
2. As Admin, create an employee record, then create an Employee login using `POST /api/auth/register` with the same user's chosen email and a strong password. Registration always creates the Employee role.
3. Verify the employee's identity through your organization process, then use Admin token on `POST /api/auth/employees/{employeeId}/link-account` with `{ "email": "person@example.com" }`.
4. Sign in as Employee. Confirm `/api/employees/me` and its linked ID work; another employee ID and Admin-only list/mutation endpoints return 403. Confirm Admin can manage all records and create another Admin.
5. Check missing/invalid credentials return 401, invalid request data returns 400 with field errors, and duplicate email returns 409.

Swagger is served at `/swagger` on the configured host. Live URL, cloud account setup, Atlas credentials, TLS certificate, and actual deployment status must be supplied by the operator/provider.

## Tests and formatting

```sh
dotnet restore
dotnet build
dotnet test
dotnet format EmployeeManagement.sln --verify-no-changes
```

Tests are independent of a production database. Mongo client/database are singletons; repositories and application services are scoped per request.



