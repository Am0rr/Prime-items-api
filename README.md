# Prime-items-api

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=.net)
![EF Core](https://img.shields.io/badge/EF_Core-10.0-5C2D91?style=flat&logo=nuget)
![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?style=flat&logo=docker)
![SQL Server](https://img.shields.io/badge/SQL_Server-Supported-CC2927?style=flat&logo=microsoftsqlserver)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

A REST API built with **.NET 10** for catalog and order management, implementing Clean N-Tier architecture, JWT-based role authentication, EF Core, and basic business analytics.

## Tech Stack
* **.NET 10, ASP.NET Core Web API**
* **Entity Framework Core** — code-first migrations, per-entity configuration classes
* **SQL Server** — containerized via Docker
* **JWT Authentication** — access + refresh token flow with role-based authorization
* **FluentValidation** — request validation
* **AutoMapper** — entity ↔ DTO mapping
* **Repository Pattern + Unit of Work** — data access abstraction
* **Dependency Injection** — built-in .NET DI container
* **xUnit + Moq** — unit testing for the BLL layer
* **Docker / Docker Compose** — SQL Server containerization
* **Postman** — API testing collection included in the repo

## Architecture
The project follows a Clean N-Tier architecture, split into three main layers:

* **PI.DAL (Data Access Layer)** — Entities, EF Core configurations, repositories, Unit of Work.
* **PI.BLL (Business Logic Layer)** — Services, DTOs, validators, mapping profiles.
* **PI.PL (Presentation Layer)** — Controllers, middleware, JWT setup, DI composition root.

Domain entities follow a *semi-rich model*: private setters protect entity state from uncontrolled mutation, while validation and business rules live in the service layer rather than the entities themselves.

## Roles & Permissions

| Role | Products / Categories | Orders | Users | Statistics |
| :--- | :--- | :--- | :--- | :--- |
| **Anonymous** | View only | — | — | — |
| **Registered** | View only | Create, view own orders | — | — |
| **Manager** | Create, edit, delete | View all, update status | — | — |
| **Admin** | Create, edit, delete | View all, update status, delete | Full access | Full access |

## Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Docker](https://www.docker.com/) / Docker Compose
* A REST client (Postman collection included)

### 1. Clone the repository
```bash
git clone [https://github.com/Am0rr/Prime-items-api.git](https://github.com/Am0rr/Prime-items-api.git)
```

```bash
cd Prime-items-api
```

### 2. Configure environment variables

Copy `.env.example` to `.env` and fill in the values:

```env
DB_HOST=localhost
DB_PORT=1433
MSSQL_SA_PASSWORD=YourStrongPassword123!
DB_NAME=PIDb

Jwt__SecureKey=your-secret-key-min-32-characters-long
Jwt__Issuer=PI.Backend
Jwt__Audience=PI.Frontend
Jwt__AccessTokenLifetimeInMinutes=15
Jwt__RefreshTokenLifetimeInDays=7
```

> **Note:** The API automatically constructs the Connection String using the `DB_*` variables, so you don't need to provide a full connection string manually. `MSSQL_SA_PASSWORD` must satisfy SQL Server's complexity policy (uppercase, lowercase, digit, special character, 8+ characters).

### 3. Start the database

```bash
docker compose up -d
```

### 4. Run the API

```bash
dotnet run --project PI.PL
```

On first launch, the app automatically applies EF Core migrations and creates the database schema — no manual table creation needed. Each developer runs their own local database; data is not shared between machines.

### 5. Test the API

Import the included Postman collection: `Prime-items-api/postman/collection.json`

To unlock full API access, register a user via `POST /api/auth/register`, then promote them to Admin directly in the database:

```sql
UPDATE Users SET Role = 'Admin' WHERE Email = 'your@email.com';
```

---

## API Endpoints

### Auth (`/api/auth`) — Public

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/register` | Register a new user (default role: Registered) |
| `POST` | `/login` | Authenticate and receive access + refresh tokens |
| `POST` | `/refresh` | Exchange a refresh token for a new token pair |
| `POST` | `/revoke` | Revoke a refresh token |

### Categories (`/api/categories`)

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `GET` | `/` | Anonymous | List all categories |
| `GET` | `/{id}` | Anonymous | Get category by ID |
| `POST` | `/` | Admin, Manager | Create a category |
| `PATCH` | `/{id}` | Admin, Manager | Update a category |
| `DELETE` | `/{id}` | Admin, Manager | Delete a category |

### Products (`/api/products`)

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `GET` | `/` | Anonymous | List all products |
| `GET` | `/{id}` | Anonymous | Get product by ID |
| `GET` | `/paged` | Anonymous | Filtered, paginated, sorted product list |
| `POST` | `/` | Admin, Manager | Create a product |
| `PATCH` | `/{id}` | Admin, Manager | Update a product |
| `DELETE` | `/{id}` | Admin, Manager | Delete a product |

### Orders (`/api/orders`)

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| `POST` | `/` | Registered | Place a new order |
| `GET` | `/{id}` | Admin, Manager, Registered* | Get order details |
| `GET` | `/` | Admin, Manager | List all orders |
| `GET` | `/user/{userId}` | Admin, Manager, Registered* | List a user's orders |
| `PATCH` | `/{id}` | Admin, Manager | Update order status |
| `DELETE` | `/{id}` | Admin | Delete an order |

Registered users may only access their own orders; ownership is enforced in the service layer.

### Users (`/api/users`) — Admin only

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/` | List all users |
| `GET` | `/{id}` | Get user by ID |
| `GET` | `/email?email=` | Get user by email |
| `PATCH` | `/{id}` | Update a user (including role assignment) |
| `DELETE` | `/{id}` | Delete a user |

### Statistics (`/api/statistics`) — Admin only

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/summary` | Total revenue and order count for a date range |
| `GET` | `/top-products` | Best-selling products |
| `GET` | `/revenue-by-category` | Revenue breakdown by category |
| `GET` | `/low-stock` | Products below a stock threshold |
| `GET` | `/top-users` | Top-spending customers |

---

## Testing

Unit tests cover the Business Logic Layer using `xUnit` and `Moq`, with mocked repositories via `MockQueryable.Moq`.

```bash
dotnet test
```

## Team
This project was built by a team of three as a coursework assignment, implementing a **Feature-Driven / Vertical Slice** development approach where each engineer was fully responsible for their respective modules (from Database Entities and Business Logic to API Endpoints and Unit Tests):

* **Am0rr** — *Team Lead / Lead Backend Developer*
  * Designed and implemented system architecture, global exception handling, and core project infrastructure.
  * Developed the **Identity & Authentication** module (JWT access/refresh token flow, secure password hashing, and role-based authorization rules).
  * Responsible for DevOps configuration (Docker, environment orchestration) and final code reviews.

* **ArtemBereznii** — *Backend Developer / Data Analytics Engineer*
  * Developed the **Catalog** module, including full-text search, multi-parameter product filtering, sorting, and cursor/page-based pagination.
  * Built the **Business Analytics & Statistics** module, creating optimized queries for complex reporting (revenue breakdown, top-spending customers, low-stock triggers).

* **Artem0311** — *Backend Developer / Order Management Engineer*
  * Developed the **Orders** module, handling the full lifecycle of a purchase from placement to status transitions.
  * Implemented strict transactional business rules for inventory management (race-condition safe stock reduction and validation).
  * Formulated security layer rules for order ownership, ensuring users can only access their personal data.