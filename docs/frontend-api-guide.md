# BancoAnchoas API — Guía Completa para Frontend

> Documento generado a partir del código fuente del backend.  
> Base URL: `https://<host>/api`  
> Autenticación: **JWT Bearer** — enviar header `Authorization: Bearer <token>`

---

## Tabla de Contenidos

1. [Autenticación y Autorización](#1-autenticación-y-autorización)
2. [Estructura de Respuestas](#2-estructura-de-respuestas)
3. [Tipos y Enums](#3-tipos-y-enums)
4. [Módulo Auth](#4-módulo-auth)
5. [Módulo Categorías](#5-módulo-categorías)
6. [Módulo Productos](#6-módulo-productos)
7. [Módulo Stock (Movimientos)](#7-módulo-stock-movimientos)
8. [Módulo Almacenes (Warehouses)](#8-módulo-almacenes-warehouses)
9. [Módulo Sectores](#9-módulo-sectores)
10. [Módulo Usuarios](#10-módulo-usuarios)
11. [Módulo Notificaciones](#11-módulo-notificaciones)
12. [Reglas de Negocio](#12-reglas-de-negocio)
13. [Interfaces TypeScript Completas](#13-interfaces-typescript-completas)

---

## 1. Autenticación y Autorización

### Roles del sistema

| Rol | Descripción |
|-----|-------------|
| `Admin` | Acceso total: CRUD usuarios, categorías, almacenes, ajustes de stock, historial |
| `Almacenista` | Gestión de productos, movimientos de stock (Entry/Exit/WriteOff/Relocation) |

### Flujo de autenticación

1. `POST /api/auth/login` con email + password → obtener `token`
2. Incluir el token en todas las peticiones: `Authorization: Bearer <token>`
3. El token contiene: `userId`, `email`, `name`, `role`

### Permisos por endpoint

| Endpoint | Método | Mín. Rol Requerido |
|----------|--------|---------------------|
| `POST /api/auth/login` | POST | **Público** (sin token) |
| `GET /api/auth/profile` | GET | Cualquier usuario autenticado |
| **Categorías** | GET (listar/detalle) | Cualquier usuario autenticado |
| **Categorías** | POST/PUT/DELETE | `Admin` |
| **Productos** | GET/POST/PUT | Cualquier usuario autenticado |
| **Productos** | DELETE | `Admin` |
| **Stock** movimientos | POST | Cualquier usuario autenticado |
| **Stock** ajustes | POST | `Admin` |
| **Stock** historial | GET | `Admin` |
| **Stock** write-offs | GET | `Admin` |
| **Almacenes** | GET (listar/detalle/sectores) | Cualquier usuario autenticado |
| **Almacenes** | POST/PUT/DELETE | `Admin` |
| **Sectores** | GET | Cualquier usuario autenticado |
| **Sectores** | PUT/DELETE | `Admin` |
| **Usuarios** | TODOS | `Admin` |
| **Notificaciones** | TODOS | Cualquier usuario autenticado |

---

## 2. Estructura de Respuestas

### Respuesta exitosa — `ApiResponse<T>`

```json
{
  "data": T,
  "message": "string | null"
}
```

- `data`: contiene el payload (objeto, array, número, etc.)
- `message`: mensaje opcional (generalmente `null` en éxito)

### Respuesta exitosa paginada — `ApiResponse<PaginatedList<T>>`

```json
{
  "data": {
    "items": [ T ],
    "pageNumber": 1,
    "totalPages": 5,
    "totalCount": 98,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": null
}
```

### Respuesta de creación (HTTP 201)

Las creaciones devuelven **solo el ID** del recurso creado, nunca el objeto completo:

```json
{
  "data": 42,
  "message": null
}
```

> Para usuarios, el `data` es un `string` (GUID de Identity).

### Respuesta sin contenido (HTTP 204)

Updates (`PUT`) y deletes (`DELETE`) devuelven `204 No Content` — sin body.

### Respuestas de error

#### Error de validación — HTTP 400

```json
{
  "succeeded": false,
  "message": "Error de validación.",
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Quantity": ["Quantity exceeds available stock."]
  }
}
```

- `errors` es un diccionario `{ [campo: string]: string[] }` donde cada clave es el nombre del campo y el valor es un array de mensajes.

#### No encontrado — HTTP 404

```json
{
  "succeeded": false,
  "message": "Entity \"Product\" (42) was not found.",
  "errors": null
}
```

#### Prohibido — HTTP 403

```json
{
  "succeeded": false,
  "message": "Invalid credentials.",
  "errors": null
}
```

#### Error interno — HTTP 500

```json
{
  "succeeded": false,
  "message": "Ha ocurrido un error interno.",
  "errors": null
}
```

---

## 3. Tipos y Enums

### `MovementType` (int)

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| `0` | `Entry` | Entrada de stock |
| `1` | `Exit` | Salida de stock |
| `2` | `WriteOff` | Baja / merma |
| `3` | `Relocation` | Reubicación entre sectores |
| `4` | `Adjustment` | Ajuste manual (solo Admin) |

### `AdjustmentType` (int)

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| `0` | `Increase` | Incremento de stock |
| `1` | `Decrease` | Decremento de stock |

### `MovementReason` (int)

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| `0` | `Expiration` | Expiración del producto |
| `1` | `Damage` | Daño del producto |
| `2` | `Loss` | Pérdida |
| `3` | `Other` | Otro motivo |

### `NotificationType` (int)

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| `0` | `LowStock` | Stock bajo mínimo |
| `1` | `Expiring` | Próximo a vencer |
| `2` | `Expired` | Vencido |

### `OrderStatus` (int)

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| `0` | `Pending` | Pendiente |
| `1` | `Processing` | En proceso |
| `2` | `Completed` | Completado |
| `3` | `Cancelled` | Cancelado |

### `UserRole`

| Valor | Nombre |
|-------|--------|
| `0` | `Admin` |
| `1` | `Almacenista` |

> **Nota**: En la API, los roles se envían/reciben como `string` (`"Admin"` o `"Almacenista"`), no como números.

### Unidades válidas para productos

```
"kg" | "g" | "un" | "lt" | "ml"
```

---

## 4. Módulo Auth

### `POST /api/auth/login`

**Público** — no requiere token.

**Request body:**

```json
{
  "email": "admin@bancodeanchoas.com",
  "password": "Admin123!"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `email` | Requerido, formato email válido |
| `password` | Requerido, mínimo 8 caracteres |

**Response (200):**

```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
      "id": "guid-string",
      "email": "admin@bancodeanchoas.com",
      "name": "Administrador",
      "role": "Admin"
    }
  },
  "message": null
}
```

**Errores posibles:**
- `403` — Credenciales inválidas

---

### `GET /api/auth/profile`

**Requiere autenticación.**

**Response (200):**

```json
{
  "data": {
    "id": "guid-string",
    "email": "admin@bancodeanchoas.com",
    "name": "Administrador",
    "role": "Admin"
  },
  "message": null
}
```

---

## 5. Módulo Categorías

### `GET /api/categories`

Obtiene todas las categorías activas, ordenadas por nombre.

**Response (200):**

```json
{
  "data": [
    {
      "id": 1,
      "name": "Conservas",
      "description": "Productos en conserva",
      "createdAt": "2026-01-15T10:30:00Z"
    }
  ],
  "message": null
}
```

---

### `GET /api/categories/{id}`

**Parámetros de ruta:** `id` (int)

**Response (200):**

```json
{
  "data": {
    "id": 1,
    "name": "Conservas",
    "description": "Productos en conserva",
    "createdAt": "2026-01-15T10:30:00Z"
  },
  "message": null
}
```

**Errores:** `404` si no existe.

---

### `POST /api/categories` — Solo Admin

**Request body:**

```json
{
  "name": "Conservas",
  "description": "Productos en conserva"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `name` | Requerido, máx 100 caracteres |
| `description` | Opcional |

**Response (201):**

```json
{
  "data": 1,
  "message": null
}
```

> Devuelve solo el `id` (int) del recurso creado.

---

### `PUT /api/categories/{id}` — Solo Admin

**Parámetros de ruta:** `id` (int)

**Request body:**

```json
{
  "id": 1,
  "name": "Conservas Premium",
  "description": "Productos en conserva de alta calidad"
}
```

> **Importante**: el `id` del body debe coincidir con el `id` de la URL. Si no coinciden → `400 Bad Request`.

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `id` | > 0 |
| `name` | Requerido, máx 100 caracteres |
| `description` | Opcional |

**Response:** `204 No Content`

---

### `DELETE /api/categories/{id}` — Solo Admin

Soft delete: desactiva la categoría (no la elimina físicamente).

**Parámetros de ruta:** `id` (int)

**Response:** `204 No Content`

**Errores:** `404` si no existe.

---

## 6. Módulo Productos

### `GET /api/products`

Lista paginada de productos. Permite búsqueda y filtro por categoría.

**Query params:**
| Param | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `search` | string? | — | Busca por nombre o SKU |
| `categoryId` | int? | — | Filtra por categoría |
| `pageNumber` | int | 1 | Página actual |
| `pageSize` | int | 20 | Items por página |

**Response (200):**

```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Anchoas del Cantábrico",
        "sku": "PROD-00001",
        "barcode": "8437000000001",
        "unit": "un",
        "stock": 150,
        "minimumStock": 20,
        "categoryName": "Conservas"
      }
    ],
    "pageNumber": 1,
    "totalPages": 3,
    "totalCount": 45,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": null
}
```

---

### `GET /api/products/{id}`

**Parámetros de ruta:** `id` (int)

**Response (200):**

```json
{
  "data": {
    "id": 1,
    "name": "Anchoas del Cantábrico",
    "description": "Filetes de anchoa en aceite de oliva",
    "sku": "PROD-00001",
    "barcode": "8437000000001",
    "price": 12.50,
    "unit": "un",
    "stock": 150,
    "minimumStock": 20,
    "expirationDate": "2026-12-31T00:00:00Z",
    "supplier": "Conservera del Norte",
    "categoryId": 1,
    "categoryName": "Conservas",
    "defaultSectorId": 3,
    "createdAt": "2026-01-15T10:30:00Z",
    "updatedAt": "2026-03-20T14:22:00Z"
  },
  "message": null
}
```

---

### `GET /api/products/by-barcode/{barcode}`

Busca un producto por su código de barras.

**Parámetros de ruta:** `barcode` (string)

**Response (200):** Mismo formato que `GET /api/products/{id}`

**Errores:** `404` si no existe un producto con ese barcode.

---

### `GET /api/products/low-stock`

Devuelve todos los productos cuyo `stock <= minimumStock` (y `minimumStock > 0`), ordenados de menor a mayor stock.

**Response (200):**

```json
{
  "data": [
    {
      "id": 5,
      "name": "Atún en aceite",
      "sku": "PROD-00005",
      "barcode": "8437000000005",
      "unit": "un",
      "stock": 3,
      "minimumStock": 10,
      "categoryName": "Conservas"
    }
  ],
  "message": null
}
```

---

### `GET /api/products/expiring`

Devuelve productos con fecha de expiración dentro de los próximos **7 días**, ordenados por fecha de expiración ascendente.

**Response (200):** Mismo formato que `GET /api/products/low-stock`

---

### `POST /api/products`

**Request body:**

```json
{
  "name": "Anchoas del Cantábrico",
  "description": "Filetes de anchoa en aceite de oliva",
  "barcode": "8437000000001",
  "price": 12.50,
  "unit": "un",
  "stock": 150,
  "minimumStock": 20,
  "expirationDate": "2026-12-31T00:00:00Z",
  "supplier": "Conservera del Norte",
  "categoryId": 1,
  "defaultSectorId": 3
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `name` | Requerido, máx 200 caracteres |
| `description` | Opcional |
| `barcode` | Opcional, máx 100 caracteres |
| `price` | Opcional, >= 0 |
| `unit` | Requerido, debe ser: `kg`, `g`, `un`, `lt`, `ml` |
| `stock` | Requerido, >= 0 |
| `minimumStock` | Requerido, >= 0 |
| `expirationDate` | Opcional (ISO 8601) |
| `supplier` | Opcional |
| `categoryId` | Requerido, > 0 |
| `defaultSectorId` | Opcional |

> El **SKU** se genera automáticamente con formato `PROD-XXXXX` (ej: `PROD-00042`). NO se envía en la creación.

**Response (201):**

```json
{
  "data": 42,
  "message": null
}
```

---

### `PUT /api/products/{id}`

**Request body:**

```json
{
  "id": 42,
  "name": "Anchoas del Cantábrico Premium",
  "description": "Filetes premium de anchoa",
  "barcode": "8437000000001",
  "price": 15.00,
  "unit": "un",
  "minimumStock": 25,
  "expirationDate": "2027-06-30T00:00:00Z",
  "supplier": "Conservera del Norte",
  "categoryId": 1,
  "defaultSectorId": 3
}
```

> **Nota**: El update NO incluye `stock`. El stock solo se modifica a través de movimientos de stock.

**Validaciones:** Mismas que la creación (sin `stock`).

**Response:** `204 No Content`

---

### `DELETE /api/products/{id}` — Solo Admin

Soft delete: desactiva el producto.

**Response:** `204 No Content`

---

## 7. Módulo Stock (Movimientos)

### `POST /api/stock/movements`

Registra una **entrada** o **salida** de stock.

**Request body:**

```json
{
  "productId": 1,
  "sectorId": 3,
  "quantity": 50,
  "type": 0,
  "notes": "Recepción de pedido #1234"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `productId` | Requerido, > 0 |
| `sectorId` | Requerido, > 0 |
| `quantity` | Requerido, > 0 (siempre positivo) |
| `type` | Solo `0` (Entry) o `1` (Exit). Para WriteOff/Relocation/Adjustment usar endpoints dedicados |
| `notes` | Opcional |

**Lógica de negocio:**
- `Entry` (0): suma `quantity` al stock del producto
- `Exit` (1): resta `quantity` del stock. Falla si `quantity > stock actual`

**Response (200):**

```json
{
  "data": 101,
  "message": null
}
```

> Devuelve el `id` del movimiento creado.

---

### `POST /api/stock/write-off`

Registra una **baja/merma** de stock. Siempre requiere `reason`.

**Request body:**

```json
{
  "productId": 1,
  "sectorId": 3,
  "quantity": 5,
  "reason": 1,
  "notes": "Latas dañadas en transporte"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `productId` | Requerido, > 0 |
| `sectorId` | Requerido, > 0 |
| `quantity` | Requerido, > 0 |
| `reason` | Requerido, enum `MovementReason` (0-3) |
| `notes` | Opcional |

**Lógica:** Resta `quantity` del stock. Falla si excede stock disponible.

**Response (200):** `{ "data": <movementId>, "message": null }`

---

### `POST /api/stock/relocate`

**Reubica** stock de un sector a otro. **No modifica el stock total** del producto.

**Request body:**

```json
{
  "productId": 1,
  "fromSectorId": 3,
  "sectorId": 5,
  "quantity": 20,
  "notes": "Reorganización de almacén"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `productId` | Requerido, > 0 |
| `fromSectorId` | Requerido, > 0 |
| `sectorId` | Requerido, > 0, **diferente** de `fromSectorId` |
| `quantity` | Requerido, > 0 |
| `notes` | Opcional |

**Response (200):** `{ "data": <movementId>, "message": null }`

---

### `POST /api/stock/adjustment` — Solo Admin

Ajuste manual de stock (incremento o decremento).

**Request body:**

```json
{
  "productId": 1,
  "sectorId": 3,
  "quantity": 10,
  "adjustmentType": 0,
  "reason": 3,
  "notes": "Corrección de inventario físico"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `productId` | Requerido, > 0 |
| `sectorId` | Requerido, > 0 |
| `quantity` | Requerido, > 0 |
| `adjustmentType` | Requerido, enum `AdjustmentType` (0=Increase, 1=Decrease) |
| `reason` | Opcional, enum `MovementReason` (0-3) |
| `notes` | Opcional |

**Lógica:**
- `Increase` (0): suma `quantity` al stock
- `Decrease` (1): resta `quantity` del stock. Falla si excede stock disponible.

**Response (200):** `{ "data": <movementId>, "message": null }`

---

### `GET /api/stock/history` — Solo Admin

Historial paginado de todos los movimientos de stock con filtros.

**Query params:**
| Param | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `productId` | int? | — | Filtrar por producto |
| `sectorId` | int? | — | Filtrar por sector |
| `type` | int? (MovementType) | — | Filtrar por tipo de movimiento (0-4) |
| `from` | DateTime? | — | Fecha inicio (ISO 8601) |
| `to` | DateTime? | — | Fecha fin (ISO 8601) |
| `pageNumber` | int | 1 | Página actual |
| `pageSize` | int | 20 | Items por página |

**Response (200):**

```json
{
  "data": {
    "items": [
      {
        "id": 101,
        "quantity": 50,
        "type": 0,
        "adjustmentType": null,
        "reason": null,
        "notes": "Recepción pedido #1234",
        "productId": 1,
        "productName": "Anchoas del Cantábrico",
        "sectorId": 3,
        "sectorName": "Sector A",
        "fromSectorId": null,
        "fromSectorName": null,
        "userId": "guid-string",
        "createdAt": "2026-03-20T14:22:00Z"
      }
    ],
    "pageNumber": 1,
    "totalPages": 10,
    "totalCount": 195,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": null
}
```

---

### `GET /api/stock/write-offs` — Solo Admin

Historial de bajas/mermas. Filtrable por razón.

**Query params:**
| Param | Tipo | Descripción |
|-------|------|-------------|
| `reason` | int? (MovementReason) | Filtrar por razón (0-3) |

**Response (200):**

```json
{
  "data": [
    {
      "id": 55,
      "quantity": 5,
      "type": 2,
      "adjustmentType": null,
      "reason": 1,
      "notes": "Latas dañadas",
      "productId": 1,
      "productName": "Anchoas del Cantábrico",
      "sectorId": 3,
      "sectorName": "Sector A",
      "fromSectorId": null,
      "fromSectorName": null,
      "userId": "guid-string",
      "createdAt": "2026-03-18T09:15:00Z"
    }
  ],
  "message": null
}
```

> **Nota**: Este endpoint NO está paginado. Devuelve un array directo.

---

## 8. Módulo Almacenes (Warehouses)

### `GET /api/warehouses`

Lista todos los almacenes activos, ordenados por nombre.

**Response (200):**

```json
{
  "data": [
    {
      "id": 1,
      "name": "Almacén Principal",
      "location": "Calle del Puerto 15, Santoña",
      "createdAt": "2026-01-10T08:00:00Z"
    }
  ],
  "message": null
}
```

---

### `GET /api/warehouses/{id}`

**Response (200):**

```json
{
  "data": {
    "id": 1,
    "name": "Almacén Principal",
    "location": "Calle del Puerto 15, Santoña",
    "createdAt": "2026-01-10T08:00:00Z"
  },
  "message": null
}
```

---

### `POST /api/warehouses` — Solo Admin

**Request body:**

```json
{
  "name": "Almacén Norte",
  "location": "Calle Industrial 42"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `name` | Requerido, máx 100 caracteres |
| `location` | Opcional, máx 200 caracteres |

**Response (201):** `{ "data": 1, "message": null }`

---

### `PUT /api/warehouses/{id}` — Solo Admin

**Request body:**

```json
{
  "id": 1,
  "name": "Almacén Principal Renovado",
  "location": "Calle del Puerto 15, Santoña"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `id` | > 0, debe coincidir con URL |
| `name` | Requerido, máx 100 caracteres |
| `location` | Opcional, máx 200 caracteres |

**Response:** `204 No Content`

---

### `DELETE /api/warehouses/{id}` — Solo Admin

Soft delete. **No se puede desactivar un almacén que tenga sectores activos.**

**Response:** `204 No Content`

**Errores:**
- `404` — No existe
- `400` — `"Cannot deactivate a warehouse with active sectors."`

---

### `GET /api/warehouses/{id}/sectors`

Obtiene los sectores de un almacén específico con sus categorías asociadas.

**Response (200):**

```json
{
  "data": [
    {
      "id": 3,
      "name": "Sector A",
      "warehouseId": 1,
      "warehouseName": "Almacén Principal",
      "categories": [
        { "id": 1, "name": "Conservas" },
        { "id": 2, "name": "Refrigerados" }
      ],
      "createdAt": "2026-01-12T09:00:00Z"
    }
  ],
  "message": null
}
```

---

### `POST /api/warehouses/{id}/sectors` — Solo Admin

Crea un sector dentro de un almacén.

**Request body:**

```json
{
  "name": "Sector C",
  "warehouseId": 1
}
```

> **Importante**: `warehouseId` en el body debe coincidir con el `{id}` de la URL.

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `name` | Requerido, máx 100 caracteres |
| `warehouseId` | Requerido, > 0, coincide con URL |

**Response (201):** `{ "data": 5, "message": null }`

---

## 9. Módulo Sectores

Endpoints independientes para gestionar sectores sin pasar por el almacén padre.

### `GET /api/sectors/{id}`

**Response (200):**

```json
{
  "data": {
    "id": 3,
    "name": "Sector A",
    "warehouseId": 1,
    "warehouseName": "Almacén Principal",
    "categories": [
      { "id": 1, "name": "Conservas" }
    ],
    "createdAt": "2026-01-12T09:00:00Z"
  },
  "message": null
}
```

---

### `PUT /api/sectors/{id}` — Solo Admin

**Request body:**

```json
{
  "id": 3,
  "name": "Sector A - Renovado"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `id` | > 0, coincide con URL |
| `name` | Requerido, máx 100 caracteres |

**Response:** `204 No Content`

---

### `DELETE /api/sectors/{id}` — Solo Admin

Soft delete del sector.

**Response:** `204 No Content`

---

## 10. Módulo Usuarios

> **Todos los endpoints requieren rol `Admin`.**

### `GET /api/users`

Lista todos los usuarios del sistema.

**Response (200):**

```json
{
  "data": [
    {
      "id": "guid-string",
      "email": "admin@bancodeanchoas.com",
      "name": "Administrador",
      "role": "Admin",
      "isActive": true
    },
    {
      "id": "guid-string-2",
      "email": "almacenista@bancodeanchoas.com",
      "name": "Juan Pérez",
      "role": "Almacenista",
      "isActive": true
    }
  ],
  "message": null
}
```

---

### `POST /api/users`

**Request body:**

```json
{
  "email": "nuevo@bancodeanchoas.com",
  "name": "María García",
  "password": "Password123!",
  "role": "Almacenista"
}
```

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `email` | Requerido, formato email válido |
| `name` | Requerido, máx 200 caracteres |
| `password` | Requerido, mínimo 8 caracteres |
| `role` | Requerido, debe ser `"Admin"` o `"Almacenista"` |

**Response (201):**

```json
{
  "data": "guid-string-nuevo",
  "message": null
}
```

> El ID del usuario es un `string` (GUID), no un `int`.

---

### `PUT /api/users/{id}`

**Parámetros de ruta:** `id` (string — GUID)

**Request body:**

```json
{
  "id": "guid-string",
  "name": "María García López",
  "email": "maria@bancodeanchoas.com",
  "role": "Admin"
}
```

> Todos los campos (excepto `id`) son opcionales. Solo se actualizan los campos enviados con valor no nulo.

**Validaciones:**
| Campo | Regla |
|-------|-------|
| `id` | Requerido, no vacío, coincide con URL |
| `email` | Formato email válido (si se envía) |
| `name` | Opcional |
| `role` | `"Admin"` o `"Almacenista"` (si se envía) |

**Response:** `204 No Content`

---

### `DELETE /api/users/{id}`

Soft delete del usuario.

**Parámetros de ruta:** `id` (string — GUID)

**Response:** `204 No Content`

---

## 11. Módulo Notificaciones

### `GET /api/notifications`

Lista paginada de notificaciones. Se puede filtrar por leídas/no leídas.

**Query params:**
| Param | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `isRead` | bool? | — | `true` = solo leídas, `false` = solo no leídas |
| `pageNumber` | int | 1 | Página actual |
| `pageSize` | int | 20 | Items por página |

**Response (200):**

```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Stock bajo",
        "message": "El producto 'Anchoas del Cantábrico' está por debajo del mínimo",
        "type": 0,
        "productId": 1,
        "isRead": false,
        "readAt": null,
        "createdAt": "2026-03-20T08:00:00Z"
      }
    ],
    "pageNumber": 1,
    "totalPages": 2,
    "totalCount": 25,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": null
}
```

---

### `GET /api/notifications/unread-count`

Devuelve la cantidad de notificaciones no leídas.

**Response (200):**

```json
{
  "data": 7,
  "message": null
}
```

---

### `PUT /api/notifications/{id}/read`

Marca una notificación como leída.

**Response:** `204 No Content`

---

### `PUT /api/notifications/read-all`

Marca **todas** las notificaciones no leídas como leídas.

**Response:** `204 No Content`

---

## 12. Reglas de Negocio

### Stock

- El stock de un producto **nunca puede ser negativo**
- Todos los movimientos requieren `Quantity > 0`
- `Exit` y `WriteOff` fallan si `quantity > stock actual`
- **Relocation** no modifica `Product.Stock` — solo cambia de sector
- `Adjustment` con `Decrease` falla si `quantity > stock actual`

### Soft Delete

- Todas las eliminaciones son **lógicas**: `IsActive = false`, `DeactivatedAt = DateTime.UtcNow`
- Los listados del backend solo devuelven entidades activas (`IsActive = true`)

### Almacenes y Sectores

- Un almacén **no se puede desactivar** si tiene sectores activos
- Los sectores están asociados a categorías (relación many-to-many via `SectorCategory`)

### Productos

- El **SKU se auto-genera** al crear el producto: `PROD-{id:D5}` (ej: `PROD-00042`)
- El **stock no se puede editar** directamente en el UPDATE — solo a través de movimientos
- Unidades válidas: `kg`, `g`, `un`, `lt`, `ml`
- Los productos pueden tener un `DefaultSector` (sector asignado por defecto)

### WriteOff vs Adjustment

| Aspecto | WriteOff | Adjustment |
|---------|----------|------------|
| Endpoint | `POST /api/stock/write-off` | `POST /api/stock/adjustment` |
| Restricción | Cualquier usuario | Solo Admin |
| Dirección | Siempre resta stock | Increase o Decrease |
| Reason | **Obligatorio** | Opcional |
| Uso | Mermas, daños, pérdidas | Correcciones de inventario |

### Notificaciones

- Tipos auto-generados por el sistema: stock bajo, próximo a vencer, vencido
- Los productos se consideran **próximos a vencer** si expiran dentro de 7 días
- Las notificaciones tienen referencia opcional al producto (`productId`)

---

## 13. Interfaces TypeScript Completas

A continuación se presentan todas las interfaces necesarias para implementar el frontend.

```typescript
// ============================================================
// RESPUESTAS GENÉRICAS
// ============================================================

interface ApiResponse<T> {
  data: T;
  message: string | null;
}

interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

interface ErrorResponse {
  succeeded: false;
  message: string;
  errors: Record<string, string[]> | null;
}

// ============================================================
// ENUMS
// ============================================================

enum MovementType {
  Entry = 0,
  Exit = 1,
  WriteOff = 2,
  Relocation = 3,
  Adjustment = 4,
}

enum AdjustmentType {
  Increase = 0,
  Decrease = 1,
}

enum MovementReason {
  Expiration = 0,
  Damage = 1,
  Loss = 2,
  Other = 3,
}

enum NotificationType {
  LowStock = 0,
  Expiring = 1,
  Expired = 2,
}

enum OrderStatus {
  Pending = 0,
  Processing = 1,
  Completed = 2,
  Cancelled = 3,
}

type UserRole = "Admin" | "Almacenista";

type ProductUnit = "kg" | "g" | "un" | "lt" | "ml";

// ============================================================
// AUTH
// ============================================================

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  token: string;
  user: UserInfo;
}

interface UserInfo {
  id: string;
  email: string;
  name: string;
  role: string; // "Admin" | "Almacenista"
}

interface UserProfile {
  id: string;
  email: string;
  name: string;
  role: string;
}

// ============================================================
// CATEGORÍAS
// ============================================================

interface CategoryDto {
  id: number;
  name: string;
  description: string | null;
  createdAt: string; // ISO 8601
}

interface CreateCategoryRequest {
  name: string;
  description?: string | null;
}

interface UpdateCategoryRequest {
  id: number;
  name: string;
  description?: string | null;
}

// ============================================================
// PRODUCTOS
// ============================================================

interface ProductDto {
  id: number;
  name: string;
  description: string | null;
  sku: string;
  barcode: string | null;
  price: number | null;
  unit: ProductUnit;
  stock: number;
  minimumStock: number;
  expirationDate: string | null; // ISO 8601
  supplier: string | null;
  categoryId: number;
  categoryName: string;
  defaultSectorId: number | null;
  createdAt: string;
  updatedAt: string | null;
}

interface ProductListDto {
  id: number;
  name: string;
  sku: string;
  barcode: string | null;
  unit: ProductUnit;
  stock: number;
  minimumStock: number;
  categoryName: string;
}

interface CreateProductRequest {
  name: string;
  description?: string | null;
  barcode?: string | null;
  price?: number | null;
  unit: ProductUnit;
  stock: number;
  minimumStock: number;
  expirationDate?: string | null; // ISO 8601
  supplier?: string | null;
  categoryId: number;
  defaultSectorId?: number | null;
}

interface UpdateProductRequest {
  id: number;
  name: string;
  description?: string | null;
  barcode?: string | null;
  price?: number | null;
  unit: ProductUnit;
  minimumStock: number;
  expirationDate?: string | null;
  supplier?: string | null;
  categoryId: number;
  defaultSectorId?: number | null;
}

/** Query params para GET /api/products */
interface GetProductsParams {
  search?: string;
  categoryId?: number;
  pageNumber?: number; // default: 1
  pageSize?: number;   // default: 20
}

// ============================================================
// STOCK / MOVIMIENTOS
// ============================================================

interface StockMovementDto {
  id: number;
  quantity: number;
  type: MovementType;
  adjustmentType: AdjustmentType | null;
  reason: MovementReason | null;
  notes: string | null;
  productId: number;
  productName: string;
  sectorId: number;
  sectorName: string;
  fromSectorId: number | null;
  fromSectorName: string | null;
  userId: string;
  createdAt: string;
}

interface RegisterMovementRequest {
  productId: number;
  sectorId: number;
  quantity: number;
  type: MovementType.Entry | MovementType.Exit; // Solo 0 o 1
  notes?: string | null;
}

interface RegisterWriteOffRequest {
  productId: number;
  sectorId: number;
  quantity: number;
  reason: MovementReason; // Obligatorio
  notes?: string | null;
}

interface RegisterRelocationRequest {
  productId: number;
  fromSectorId: number;
  sectorId: number; // Destino — debe ser diferente de fromSectorId
  quantity: number;
  notes?: string | null;
}

interface RegisterAdjustmentRequest {
  productId: number;
  sectorId: number;
  quantity: number;
  adjustmentType: AdjustmentType;
  reason?: MovementReason | null;
  notes?: string | null;
}

/** Query params para GET /api/stock/history */
interface GetMovementHistoryParams {
  productId?: number;
  sectorId?: number;
  type?: MovementType;
  from?: string; // ISO 8601
  to?: string;   // ISO 8601
  pageNumber?: number;
  pageSize?: number;
}

/** Query params para GET /api/stock/write-offs */
interface GetWriteOffsParams {
  reason?: MovementReason;
}

// ============================================================
// ALMACENES (WAREHOUSES)
// ============================================================

interface WarehouseDto {
  id: number;
  name: string;
  location: string | null;
  createdAt: string;
}

interface CreateWarehouseRequest {
  name: string;
  location?: string | null;
}

interface UpdateWarehouseRequest {
  id: number;
  name: string;
  location?: string | null;
}

// ============================================================
// SECTORES
// ============================================================

interface SectorDto {
  id: number;
  name: string;
  warehouseId: number;
  warehouseName: string;
  categories: SectorCategoryDto[];
  createdAt: string;
}

interface SectorCategoryDto {
  id: number;
  name: string;
}

interface CreateSectorRequest {
  name: string;
  warehouseId: number;
}

interface UpdateSectorRequest {
  id: number;
  name: string;
}

// ============================================================
// USUARIOS
// ============================================================

interface UserDto {
  id: string; // GUID
  email: string;
  name: string;
  role: string; // "Admin" | "Almacenista"
  isActive: boolean;
}

interface CreateUserRequest {
  email: string;
  name: string;
  password: string;
  role: UserRole;
}

interface UpdateUserRequest {
  id: string;
  name?: string | null;
  email?: string | null;
  role?: UserRole | null;
}

// ============================================================
// NOTIFICACIONES
// ============================================================

interface NotificationDto {
  id: number;
  title: string;
  message: string;
  type: NotificationType;
  productId: number | null;
  isRead: boolean;
  readAt: string | null; // ISO 8601
  createdAt: string;
}

/** Query params para GET /api/notifications */
interface GetNotificationsParams {
  isRead?: boolean;
  pageNumber?: number;
  pageSize?: number;
}
```

---

## Resumen de Endpoints

| Método | Ruta | Rol Mínimo | Paginado | Retorno |
|--------|------|------------|----------|---------|
| `POST` | `/api/auth/login` | Público | No | `LoginResponse` |
| `GET` | `/api/auth/profile` | Auth | No | `UserProfile` |
| `GET` | `/api/categories` | Auth | No | `CategoryDto[]` |
| `GET` | `/api/categories/:id` | Auth | No | `CategoryDto` |
| `POST` | `/api/categories` | Admin | No | `int` (id) |
| `PUT` | `/api/categories/:id` | Admin | No | `204` |
| `DELETE` | `/api/categories/:id` | Admin | No | `204` |
| `GET` | `/api/products` | Auth | **Sí** | `PaginatedList<ProductListDto>` |
| `GET` | `/api/products/:id` | Auth | No | `ProductDto` |
| `GET` | `/api/products/by-barcode/:barcode` | Auth | No | `ProductDto` |
| `GET` | `/api/products/low-stock` | Auth | No | `ProductListDto[]` |
| `GET` | `/api/products/expiring` | Auth | No | `ProductListDto[]` |
| `POST` | `/api/products` | Auth | No | `int` (id) |
| `PUT` | `/api/products/:id` | Auth | No | `204` |
| `DELETE` | `/api/products/:id` | Admin | No | `204` |
| `POST` | `/api/stock/movements` | Auth | No | `int` (id) |
| `POST` | `/api/stock/write-off` | Auth | No | `int` (id) |
| `POST` | `/api/stock/relocate` | Auth | No | `int` (id) |
| `POST` | `/api/stock/adjustment` | Admin | No | `int` (id) |
| `GET` | `/api/stock/history` | Admin | **Sí** | `PaginatedList<StockMovementDto>` |
| `GET` | `/api/stock/write-offs` | Admin | No | `StockMovementDto[]` |
| `GET` | `/api/warehouses` | Auth | No | `WarehouseDto[]` |
| `GET` | `/api/warehouses/:id` | Auth | No | `WarehouseDto` |
| `POST` | `/api/warehouses` | Admin | No | `int` (id) |
| `PUT` | `/api/warehouses/:id` | Admin | No | `204` |
| `DELETE` | `/api/warehouses/:id` | Admin | No | `204` |
| `GET` | `/api/warehouses/:id/sectors` | Auth | No | `SectorDto[]` |
| `POST` | `/api/warehouses/:id/sectors` | Admin | No | `int` (id) |
| `GET` | `/api/sectors/:id` | Auth | No | `SectorDto` |
| `PUT` | `/api/sectors/:id` | Admin | No | `204` |
| `DELETE` | `/api/sectors/:id` | Admin | No | `204` |
| `GET` | `/api/users` | Admin | No | `UserDto[]` |
| `POST` | `/api/users` | Admin | No | `string` (id) |
| `PUT` | `/api/users/:id` | Admin | No | `204` |
| `DELETE` | `/api/users/:id` | Admin | No | `204` |
| `GET` | `/api/notifications` | Auth | **Sí** | `PaginatedList<NotificationDto>` |
| `GET` | `/api/notifications/unread-count` | Auth | No | `int` |
| `PUT` | `/api/notifications/:id/read` | Auth | No | `204` |
| `PUT` | `/api/notifications/read-all` | Auth | No | `204` |
