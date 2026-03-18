# BancoAnchoas API

API REST para la gestión de inventario del **Banco de Anchoas**. Permite administrar productos, categorías, almacenes, sectores, y movimientos de stock con autenticación basada en JWT.

## Tecnologías

| Capa | Tecnología |
|---|---|
| Framework | .NET 10 / ASP.NET Core |
| Base de datos | SQL Server (LocalDB en desarrollo) |
| ORM | Entity Framework Core 10 |
| Autenticación | ASP.NET Identity + JWT Bearer |
| CQRS / Mediator | MediatR |
| Validación | FluentValidation |
| Mapeo | Mapster |
| Logging | Serilog |
| Documentación | OpenAPI + Swagger UI (solo en Development) |
| Tests | xUnit + FluentAssertions + Moq + Microsoft.AspNetCore.Mvc.Testing |

## Arquitectura

```
BancoAnchoas.API.sln
├── src/
│   ├── BancoAnchoas.Domain/         # Entidades, enums, interfaces de repositorio
│   ├── BancoAnchoas.Application/    # CQRS (Commands/Queries), validadores, DTOs, mappings
│   └── BancoAnchoas.API/            # Controllers, middleware, DI, configuración
└── tests/
    ├── BancoAnchoas.Application.Tests/    # Tests unitarios (handlers + validators)
    └── BancoAnchoas.Integration.Tests/    # Tests de integración (endpoints HTTP)
```

Sigue **Clean Architecture** con separación estricta:
- **Domain** → sin dependencias externas
- **Application** → depende solo de Domain
- **API** → compone todo, expone endpoints REST

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server LocalDB (incluido con Visual Studio) o una instancia SQL Server

## Inicio rápido

```bash
# Clonar y restaurar
git clone <url-del-repo>
cd BancoAnchoas.API
dotnet restore

# Aplicar migraciones y ejecutar
dotnet run --project src/BancoAnchoas.API

# Swagger UI disponible en desarrollo
# http://localhost:5088/swagger
```

La base de datos se crea automáticamente al iniciar (migraciones + seed del usuario admin).

## Configuración

Archivo principal: `src/BancoAnchoas.API/appsettings.json`

| Sección | Descripción |
|---|---|
| `ConnectionStrings:DefaultConnection` | Cadena de conexión a SQL Server |
| `Jwt` | Key, Issuer, Audience, ExpirationInMinutes |
| `AdminSeed` | Email, Name, Password del admin inicial |
| `AllowedOrigins` | Orígenes CORS permitidos |
| `AlertCheck` | Intervalo y días de alerta de expiración |

> **Nota:** En producción, usa variables de entorno o un secreto seguro para `Jwt:Key`.

## Autenticación

Dos roles: **Admin** y **Almacenista**.

```bash
# Login
POST /api/auth/login
{ "email": "admin@bancodeanchoas.com", "password": "Admin123!" }

# Respuesta → { "data": { "token": "eyJ...", "email": "...", "role": "Admin" } }
```

Incluir el token en las peticiones:
```
Authorization: Bearer <token>
```

## Endpoints principales

| Recurso | Método | Ruta | Rol requerido |
|---|---|---|---|
| **Auth** | POST | `/api/auth/login` | Público |
| | POST | `/api/auth/register` | Admin |
| | GET | `/api/auth/profile` | Autenticado |
| **Productos** | GET | `/api/products` | Autenticado |
| | GET | `/api/products/{id}` | Autenticado |
| | GET | `/api/products/by-barcode/{barcode}` | Autenticado |
| | GET | `/api/products/low-stock` | Autenticado |
| | GET | `/api/products/expiring` | Autenticado |
| | POST | `/api/products` | Autenticado |
| | PUT | `/api/products/{id}` | Autenticado |
| | DELETE | `/api/products/{id}` | Admin |
| **Categorías** | GET | `/api/categories` | Autenticado |
| | POST/PUT/DELETE | `/api/categories/{id}` | Admin |
| **Almacenes** | GET | `/api/warehouses` | Autenticado |
| | POST/PUT/DELETE | `/api/warehouses/{id}` | Admin |
| | GET/POST | `/api/warehouses/{id}/sectors` | Auth / Admin |
| **Sectores** | GET/PUT/DELETE | `/api/sectors/{id}` | Auth / Admin |
| **Stock** | POST | `/api/stock/movements` | Autenticado |
| | POST | `/api/stock/write-off` | Autenticado |
| | POST | `/api/stock/relocate` | Autenticado |
| | POST | `/api/stock/adjustment` | Admin |
| | GET | `/api/stock/history` | Admin |
| | GET | `/api/stock/write-offs` | Admin |

Todas las respuestas usan el wrapper `ApiResponse<T>`:
```json
{ "data": <T>, "message": null }
```

## Tests

```bash
# Ejecutar todos los tests
dotnet test

# Solo unitarios
dotnet test tests/BancoAnchoas.Application.Tests

# Solo integración
dotnet test tests/BancoAnchoas.Integration.Tests
```

| Proyecto | Tests | Descripción |
|---|---|---|
| Application.Tests | 95 | Handlers y validators (Moq + InMemory) |
| Integration.Tests | 47 | Endpoints HTTP reales (WebApplicationFactory + InMemory DB) |

## Estructura de los tests

- **Unitarios:** un archivo por handler/validator, agrupados por feature (Products, Stock, Categories, Warehouses)
- **Integración:** un archivo por controller, heredan de `IntegrationTestBase` que provee autenticación automática con el admin seed
