# PeopleDesk Employee Management System

PeopleDesk is a public portfolio demo built with a React/Vite frontend and an ASP.NET Core 8 API backed by MongoDB. The dashboard opens at `/` without login or signup. Employee listing, search, filters, pagination, details, create, edit, and delete are available to visitors through the frontend and API.

## Public demo behavior

All `/api/employees` routes are anonymous, including listing, reading an employee, creating, updating, and deleting records. API DTO validation, consistent errors, duplicate email protection, and MongoDB indexes remain enabled. The frontend does not store or send JWTs. Legacy `/api/auth/*` endpoints remain available for API compatibility, but the demo UI does not use them; Admin-only account management endpoints retain their authorization.

This public demo has unrestricted employee write access. Use synthetic portfolio data only; do not store real employee or personal information. Never put MongoDB credentials, JWT secrets, or other private values in the frontend or browser environment variables.

## Features and routes

- `GET /api/employees` - public search, department/active filters, and pagination.
- `GET /api/employees/{id}` - public employee details.
- `POST /api/employees`, `PUT /api/employees/{id}`, `DELETE /api/employees/{id}` - public employee CRUD.
- `POST /api/auth/register` and `POST /api/auth/login` - legacy API endpoints; not used by the frontend.
- `POST /api/auth/admins` and `POST /api/auth/employees/{employeeId}/link-account` - retained Admin-only legacy account management.

## Environment variables

Copy `.env.example` to `.env` for local Docker use. Never commit `.env`, production credentials, or secret values. `.gitignore` excludes `.env` and `.env.*`, while allowing the placeholder-only `.env.example`.

| Variable | Purpose | Replace before deployment |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Set `Production` in hosting. This disables development bootstrap and enables HSTS/HTTPS redirection. |
| `ASPNETCORE_URLS` | Kestrel bind address | Container default is port 8080; match the host's required port. |
| `TRUSTED_PROXY_IPS` | Semicolon-separated exact proxy IPs allowed to supply forwarded headers | Required for Production except on Render. Render uses platform-managed ingress, so leave this unset there; see the Render deployment section. |
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

1. Copy `.env.example` to `.env` and replace the JWT placeholder with a fresh local key. Admin bootstrap credentials are optional and only used by the legacy account APIs.
2. Start API and local MongoDB: `docker compose --profile local up --build`.
3. Open `http://localhost:8080/swagger`. MongoDB's development port is bound only to `127.0.0.1`.

For a local process, start MongoDB separately, set the variables from `.env.example` in your shell, then run `dotnet run --project src/EmployeeManagement.API`. The API pings MongoDB and creates indexes at startup. Invalid configuration or unavailable database stops startup rather than silently using a default database.

The optional Development Admin bootstrap is only for the retained legacy account APIs; it is not required to use the public employee demo. Production rejects either development bootstrap variable.

## MongoDB Atlas setup

1. In Atlas, create a project and cluster in a region near the API host.
2. Create a dedicated database user with a generated password and only read/write access to the application database. Do not use your Atlas account password.
3. In **Network Access**, allow only the API host's stable outbound IP/CIDR. If egress IPs are dynamic, use private networking/peering or a controlled egress gateway; avoid `0.0.0.0/0`.
4. Select **Connect** ? **Drivers** ? **C#/.NET**. Copy the `mongodb+srv://` connection string, replace `<password>` with the database user's password, and URL-encode special characters in credentials.
5. Store the URI in the hosting provider's secret manager as `MONGODB_URI` and set `MONGODB_DATABASE_NAME`. Never paste credentials into source control, tickets, logs, or API responses.

## Docker and cloud deployment

The root Dockerfile uses multi-stage .NET 8 builds and runs as a non-root user. The API can connect to MongoDB Atlas; local MongoDB is available only through the optional Docker Compose local profile. Deploy the API container and frontend independently, configure the frontend API base URL and exact CORS origin, and provide database/JWT configuration through the host's secret manager. Because employee CRUD is public, populate a portfolio deployment with synthetic sample data only.

For a Render deployment, follow the exact settings in the [Render Production deployment](#render-production-deployment) section. For other providers, bind the API to the required HTTP port, set `ASPNETCORE_ENVIRONMENT=Production`, configure HTTPS at the ingress, and set exact trusted ingress IPs in `TRUSTED_PROXY_IPS` when forwarded headers are needed.

## Tests and formatting

```sh
dotnet restore
dotnet build
dotnet test
dotnet format EmployeeManagement.sln --verify-no-changes
```

Tests are independent of a production database. Mongo client/database are singletons; repositories and application services are scoped per request.




## React frontend

The independently deployable frontend in `frontend/` uses React, Vite, React Router, Axios, Bootstrap 5, React Bootstrap, and React Icons. It opens the dashboard at `/` without login, signup, protected routes, token storage, or authorization headers. Visitors can browse, search, filter, paginate, view, add, edit, and delete employee records.

### Frontend local setup

Prerequisites: Node.js 20.19+ or 22.12+ and npm. Start the API and MongoDB using the local development steps above, then:

```sh
cd frontend
cp .env.example .env
npm install
npm run dev
```

Open the Vite URL (normally `http://localhost:5173`). `frontend/.env.example` defines `VITE_API_BASE_URL`; set it to the API origin or its `/api` root. Axios normalizes either form to exactly one `/api` prefix, matching the controller route. The local default is `http://localhost:8080`. This value is public and must not contain credentials or secrets.

Set `CORS_ALLOWED_ORIGINS=http://localhost:5173` in the API's local environment. In production, configure it to the exact HTTPS origin serving the frontend; the API rejects wildcard or non-HTTPS production origins. Never add API secrets to `frontend/.env` or any `VITE_*` variable because Vite embeds these values in public browser assets.

The frontend makes anonymous API requests. It handles validation failures, duplicate email conflicts, loading states, and API errors.

### Frontend production build and deployment

```sh
cd frontend
npm ci
npm run build
```

The static site output is `frontend/dist`. Set `VITE_API_BASE_URL` to the real HTTPS API origin (or its `/api` root) before building and publish `dist` using a static hosting service such as Azure Static Web Apps, Cloudflare Pages, Netlify, or an equivalent. Configure the host's SPA fallback so routes such as `/employees/123` serve `index.html`. Configure backend `CORS_ALLOWED_ORIGINS` to the exact frontend HTTPS origin. Deploy API and frontend independently, configure HTTPS for both, and verify the browser can reach the API without mixed-content or CORS errors. No live hosting URL or credentials are supplied by this repository.

Frontend routes use the existing employee endpoints: list, item, create, update, and delete. Pagination/filter query names and employee DTO fields match the API contract.

## Production diagnostics

The API exposes `GET /health/ready` for hosting health checks. It pings MongoDB with a three-second deadline and returns only `{"status":"ready"}` or HTTP 503 with `{"status":"unavailable"}`. Set the Render Web Service health check path to `/health/ready`. The endpoint never returns the MongoDB URI or provider error details.

MongoDB connections use explicit connection, server-selection, and wait-queue deadlines. The API passes request cancellation through to MongoDB and does not turn a caller disconnect into a server error. Database and timeout failures return sanitized JSON errors with HTTP 503; only the failure category and trace ID are logged, without exception contents or connection strings. If Atlas connection failures recur, check the Render service outbound IP ranges against Atlas Network Access, confirm the Atlas database user/URI, and correlate the request trace ID and duration in Render logs. Do not copy outbound ranges to proxy trust settings.
## Render Production deployment

Render does not document a stable ingress proxy IP list for Web Services. Its load balancer terminates inbound TLS and forwards traffic to the service over HTTP; the service port is not directly reachable from the public internet. Render also sets `RENDER=true` at runtime. The application uses that platform-provided marker to disable forwarded-header processing on Render, so it does not trust `X-Forwarded-For` or `X-Forwarded-Proto` from arbitrary request headers. Keep `TRUSTED_PROXY_IPS` unset on Render. Do not use Render outbound IP ranges as proxy IPs; those ranges describe traffic from the app to external services such as MongoDB.

Create a Render **Web Service** using the repository Dockerfile. In the service's Environment settings, configure these values (replace every placeholder with your real value):

| Key | Render value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `PORT` | `10000` (Render default) |
| `ASPNETCORE_URLS` | `http://0.0.0.0:10000` |
| `MONGODB_URI` | Your secret MongoDB Atlas `mongodb+srv://...` URI |
| `MONGODB_DATABASE_NAME` | Your chosen production database |
| `JWT_SECRET` | A new high-entropy secret, stored as a Render secret |
| `JWT_ISSUER` | Your stable API issuer identifier |
| `JWT_AUDIENCE` | Your stable API audience identifier |
| `JWT_EXPIRY_MINUTES` | Your chosen allowed token lifetime, such as `60` |
| `CORS_ALLOWED_ORIGINS` | The exact frontend origin, e.g. `https://your-real-frontend-domain.example` |

Render sets `RENDER=true` automatically; do not add or override it. Do not set `TRUSTED_PROXY_IPS`, `INITIAL_ADMIN_EMAIL`, or `INITIAL_ADMIN_PASSWORD` on the production service. `ADMIN_PROVISIONING_EMAIL` and `ADMIN_PROVISIONING_PASSWORD` belong only to a separate one-time Admin provisioning run, then must be removed. Keep all credentials in Render's Environment/secret settings, not in source control.

Render redirects public HTTP requests to HTTPS and terminates TLS at its load balancer. The API therefore keeps HSTS enabled in Production but skips its own HTTPS redirection on Render, which would otherwise see the internal HTTP connection and could create a redirect loop. On other Production hosts, the existing exact `TRUSTED_PROXY_IPS` allowlist and ASP.NET forwarded-header/HTTPS-redirection behavior remain required.

To permit Atlas connections, use the Render API service's **Connect menu > Outbound tab** IP ranges in the Atlas Network Access allowlist if your Atlas setup requires IP allowlisting. Those are Render egress addresses for MongoDB connections; they are not ingress proxy addresses and must never be copied into `TRUSTED_PROXY_IPS`. Set the Render web service port to `10000`. For the currently configured API, set the frontend `VITE_API_BASE_URL` to `https://employee-management-system-api-my3l.onrender.com` (the Axios client adds `/api` exactly once); the resulting employee list URL is `https://employee-management-system-api-my3l.onrender.com/api/employees?page=1&pageSize=10`. Set `CORS_ALLOWED_ORIGINS` to the exact deployed frontend origin.
