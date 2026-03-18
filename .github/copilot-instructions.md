---
applyTo: "**"
---

# BancoAnchoas API - Instrucciones del proyecto

## Stack técnico
- .NET 10 / ASP.NET Core / C# 13
- Entity Framework Core 10 con SQL Server (LocalDB en desarrollo)
- ASP.NET Identity + JWT Bearer (roles: Admin, Almacenista)
- MediatR (CQRS), FluentValidation, Mapster, Serilog
- xUnit + FluentAssertions + Moq para tests

## Arquitectura
- **Clean Architecture** con 3 capas: Domain → Application → API
- **CQRS** con MediatR: Commands y Queries separados bajo `Application/Features/{Feature}/`
- Cada comando tiene: Command (record), Validator, Handler
- Controllers sólo delegan a `Mediator.Send()` — cero lógica de negocio en controllers
- Respuestas envueltas en `ApiResponse<T>` con propiedades `Data` y `Message`
- Paginación mediante `PaginatedList<T>` con `Items`, `PageNumber`, `TotalPages`, `TotalCount`

## Convenciones de código
- Los endpoints de creación devuelven solo el ID (int o string), nunca el objeto completo
- Creaciones retornan HTTP 201 con `CreatedAtAction` — no usar `Ok()` para creaciones
- Eliminaciones son lógicas (soft delete): `IsActive = false`, `DeactivatedAt = DateTime.UtcNow`
- El stock del producto nunca puede ser negativo
- Los movimientos de stock siempre requieren `Quantity > 0`
- Relocaciones no modifican `Product.Stock` — solo cambian de sector
- WriteOff siempre requiere `Reason` (enum `MovementReason`)
- Adjustment usa `AdjustmentType` (Increase/Decrease) para la dirección
- Un almacén no se puede desactivar si tiene sectores activos
- Unidades válidas para productos: kg, g, un, lt, ml

## Estructura de archivos
```
src/BancoAnchoas.API/           # Controllers, Middleware, DI, Program.cs
src/BancoAnchoas.Application/   # Features (CQRS), Common (Behaviors, Exceptions, Models, Interfaces)
src/BancoAnchoas.Domain/        # Entities, Enums, Interfaces (IRepository, IUnitOfWork)
tests/BancoAnchoas.Application.Tests/   # Tests unitarios por feature
tests/BancoAnchoas.Integration.Tests/   # Tests de integración por controller
```

## Testing
- **Tests unitarios**: Mock con Moq, validadores con FluentValidation.TestHelper
- **Tests integración**: `CustomWebApplicationFactory` con EF Core InMemory, `IntegrationTestBase` con auth automática
- Para mockear `IQueryable` async de EF Core, usar `TestAsyncEnumerable<T>` (helper propio)
- Admin seed para tests: `admin@bancodeanchoas.com` / `Admin123!`
- No usar `PaginatedList<T>` directamente para deserializar en tests — usar `JsonElement` por incompatibilidad del constructor

## Idioma
- Código, nombres de clases, métodos, y variables en **inglés**
- Mensajes de error al usuario en **español** cuando sea relevante
- Documentación (README, comentarios relevantes) en **español**
