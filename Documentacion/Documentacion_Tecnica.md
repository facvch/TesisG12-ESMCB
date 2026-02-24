# Documentación Técnica - Sistema de Gestión Veterinaria

**Versión:** 1.0  
**Fecha:** Febrero 2026  
**Destinado a:** Programadores y Analistas de Sistemas

---

## 1. Visión General de la Arquitectura

El sistema utiliza una **arquitectura DDD Hybrid (Domain-Driven Design)** organizada en capas desacopladas, implementada sobre **.NET 8** con **Entity Framework Core** y **SQLite**.

### 1.1 Diagrama de Capas

```
┌──────────────────────────────────────────┐
│            Template-API                  │  ← Capa de Presentación
│         (Controllers REST)               │     Endpoints HTTP
├──────────────────────────────────────────┤
│            Application                   │  ← Capa de Aplicación
│    (UseCases, DTOs, Repositories I/F)    │     Lógica de orquestación
├──────────────────────────────────────────┤
│              Domain                      │  ← Capa de Dominio
│     (Entities, Validators, Enums)        │     Reglas de negocio
├──────────────────────────────────────────┤
│           Infrastructure                 │  ← Capa de Infraestructura
│  (SQL Repositories, DbContext, Factory)  │     Persistencia de datos
├──────────────────────────────────────────┤
│            Core.*                        │  ← Librerías Core
│  (Base Entities, Base Repos, Bus, etc.)  │     Abstracciones reutilizables
└──────────────────────────────────────────┘
```

### 1.2 Estructura de Proyectos

```
Base/Backend/
├── Template-API/                    # API REST (.NET Web API)
│   ├── Controllers/                 # Controladores HTTP
│   ├── appsettings.json            # Configuración (DB, servicios)
│   └── Program.cs                  # Entry point
│
├── Application/                     # Capa de Aplicación
│   ├── DataTransferObjects/        # DTOs para respuestas API
│   ├── Repositories/               # Interfaces de repositorios
│   ├── UseCases/                   # Comandos y Queries (CQRS)
│   └── Exceptions/                 # Excepciones custom
│
├── Domain/                          # Capa de Dominio
│   ├── Entities/                   # Entidades de negocio
│   ├── Validators/                 # Validadores FluentValidation
│   └── Enums/                      # Enumeraciones
│
├── Infrastructure/                  # Capa de Infraestructura
│   ├── Repositories/Sql/           # Implementaciones EF Core
│   ├── Factories/                  # DatabaseFactory
│   └── Registrations/              # DI registrations
│
├── Core.Domain/                     # Base entities, validation
├── Core.Application.*/              # Buses, mapeo, adaptadores
├── Core.Infraestructure.*/          # Base repositories
│
└── Domain.Tests/                    # Tests unitarios (xUnit)
```

---

## 2. Entidades del Dominio

### 2.1 Diagrama de Relaciones

```
┌──────────┐     1:N     ┌──────────┐
│ Especie  │────────────►│   Raza   │
│  (int)   │             │  (int)   │
└────┬─────┘             └──────────┘
     │ 1:N                     │ 0..1:N
     │                         │
     ▼                         ▼
┌──────────────────────────────────┐
│           Paciente               │
│          (string GUID)           │
└────────────┬─────────────────────┘
             │ N:1
             ▼
┌──────────────────────────────────┐
│         Propietario              │
│          (string GUID)           │
└──────────────────────────────────┘
```

### 2.2 Detalle de Entidades

#### Especie
| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `Id` | `int` (autoincrement) | Sí | Identificador único |
| `Nombre` | `string(50)` | Sí | Nombre de la especie |
| `Descripcion` | `string(200)` | No | Descripción |
| `Activo` | `bool` | Sí | Estado (soft delete) |

**Métodos de negocio:** `Actualizar()`, `Desactivar()`, `Activar()`

#### Raza
| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `Id` | `int` (autoincrement) | Sí | Identificador único |
| `Nombre` | `string(50)` | Sí | Nombre de la raza |
| `Descripcion` | `string(200)` | No | Descripción |
| `EspecieId` | `int` | Sí | FK → Especie |
| `Activo` | `bool` | Sí | Estado |

**Métodos de negocio:** `Actualizar()`, `CambiarEspecie()`, `Desactivar()`, `Activar()`  
**FK:** `EspecieId` → `Especies.Id` (ON DELETE RESTRICT)

#### Propietario
| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `Id` | `string` (GUID) | Sí | Identificador único |
| `Nombre` | `string(50)` | Sí | Nombre |
| `Apellido` | `string(50)` | Sí | Apellido |
| `DNI` | `string(20)` | Sí | DNI (unique index) |
| `Telefono` | `string(20)` | Sí | Teléfono |
| `Email` | `string(100)` | No | Email (validación formato) |
| `Direccion` | `string(200)` | No | Dirección |
| `FechaRegistro` | `DateTime` | Sí | Fecha de alta (auto) |
| `Activo` | `bool` | Sí | Estado |

**Propiedad calculada:** `NombreCompleto` = `Nombre + " " + Apellido`  
**Índice único:** `DNI`

#### Paciente
| Propiedad | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `Id` | `string` (GUID) | Sí | Identificador único |
| `Nombre` | `string(50)` | Sí | Nombre de la mascota |
| `EspecieId` | `int` | Sí | FK → Especie |
| `RazaId` | `int?` | No | FK → Raza |
| `Sexo` | `string(1)` | Sí | "M" o "H" |
| `FechaNacimiento` | `DateTime?` | No | Fecha de nacimiento |
| `PropietarioId` | `string` | Sí | FK → Propietario |
| `FotoUrl` | `string` | No | URL foto |
| `Observaciones` | `string(500)` | No | Notas |
| `FechaRegistro` | `DateTime` | Sí | Fecha de alta (auto) |
| `Activo` | `bool` | Sí | Estado |

**Propiedad calculada:** `EdadEnAnios` (basada en `FechaNacimiento`)  
**Métodos de negocio:** `Actualizar()`, `CambiarPropietario()`, `CambiarEspecie()`, `ActualizarFoto()`, `Desactivar()`

---

## 3. Patrón de Validación

Cada entidad hereda de `DomainEntity<TKey, TValidator>` y tiene un validador asociado que usa **FluentValidation**.

```csharp
// Ejemplo: PacienteValidator.cs
public class PacienteValidator : EntityValidator<Paciente>
{
    public PacienteValidator()
    {
        RuleFor(p => p.Nombre).NotEmpty().MaximumLength(50);
        RuleFor(p => p.EspecieId).GreaterThan(0);
        RuleFor(p => p.PropietarioId).NotEmpty();
        RuleFor(p => p.Sexo).NotEmpty().Must(s => s == "M" || s == "H")
            .WithMessage("El sexo debe ser 'M' o 'H'");
        RuleFor(p => p.FechaNacimiento).LessThanOrEqualTo(DateTime.Now)
            .When(p => p.FechaNacimiento.HasValue);
    }
}
```

La validación se ejecuta al acceder a `entity.IsValid`. Los errores se obtienen con `entity.GetErrors()`.

---

## 4. API REST - Endpoints

### 4.1 Base URL
```
http://localhost:5204/api/v1/
```

### 4.2 Formato de Respuesta Estándar
Todas las respuestas siguen el formato del `BaseController`:
```json
{
  "success": true,
  "data": { },
  "message": "",
  "code": "",
  "statusCode": 200
}
```

### 4.3 Endpoints por Entidad

#### Especie (`/api/v1/Especie`)
| Método | Ruta | Descripción | Body |
|--------|------|-------------|------|
| `GET` | `/api/v1/Especie` | Listar especies | Query: `soloActivas=true` |
| `GET` | `/api/v1/Especie/{id}` | Obtener por ID | - |
| `POST` | `/api/v1/Especie` | Crear especie | `{ "nombre", "descripcion" }` |
| `PUT` | `/api/v1/Especie` | Actualizar | `{ "id", "nombre", "descripcion" }` |
| `DELETE` | `/api/v1/Especie/{id}` | Desactivar | - |

#### Raza (`/api/v1/Raza`)
| Método | Ruta | Descripción | Body |
|--------|------|-------------|------|
| `GET` | `/api/v1/Raza` | Listar razas | Query: `soloActivas=true` |
| `GET` | `/api/v1/Raza/{id}` | Obtener por ID | - |
| `GET` | `/api/v1/Raza/byEspecie/{especieId}` | Filtrar por especie | - |
| `POST` | `/api/v1/Raza` | Crear raza | `{ "nombre", "descripcion", "especieId" }` |
| `PUT` | `/api/v1/Raza` | Actualizar | `{ "id", "nombre", "descripcion", "especieId" }` |
| `DELETE` | `/api/v1/Raza/{id}` | Desactivar | - |

#### Propietario (`/api/v1/Propietario`)
| Método | Ruta | Descripción | Body |
|--------|------|-------------|------|
| `GET` | `/api/v1/Propietario` | Listar propietarios | Query: `soloActivos=true` |
| `GET` | `/api/v1/Propietario/{id}` | Obtener por ID | - |
| `GET` | `/api/v1/Propietario/search?nombre=` | Buscar por nombre | - |
| `GET` | `/api/v1/Propietario/byDni/{dni}` | Buscar por DNI | - |
| `POST` | `/api/v1/Propietario` | Crear propietario | `{ "nombre", "apellido", "dni", "telefono", "email", "direccion" }` |
| `PUT` | `/api/v1/Propietario` | Actualizar | `{ "id", "nombre", "apellido", "telefono", "email", "direccion" }` |
| `DELETE` | `/api/v1/Propietario/{id}` | Desactivar | - |

#### Paciente (`/api/v1/Paciente`)
| Método | Ruta | Descripción | Body |
|--------|------|-------------|------|
| `GET` | `/api/v1/Paciente` | Listar pacientes | Query: `soloActivos=true` |
| `GET` | `/api/v1/Paciente/{id}` | Obtener por ID | - |
| `GET` | `/api/v1/Paciente/search?nombre=` | Buscar por nombre | - |
| `GET` | `/api/v1/Paciente/byPropietario/{id}` | Por propietario | - |
| `GET` | `/api/v1/Paciente/byEspecie/{id}` | Por especie | - |
| `POST` | `/api/v1/Paciente` | Crear paciente | `{ "nombre", "especieId", "razaId", "sexo", "propietarioId", "fechaNacimiento", "fotoUrl", "observaciones" }` |
| `PUT` | `/api/v1/Paciente` | Actualizar | `{ "id", "nombre", "sexo", "fechaNacimiento", "fotoUrl", "observaciones" }` |
| `PUT` | `/api/v1/Paciente/{id}/cambiarPropietario` | Cambiar dueño | `"nuevoPropietarioId"` |
| `DELETE` | `/api/v1/Paciente/{id}` | Desactivar | - |

### 4.4 Códigos de Respuesta HTTP

| Código | Significado | Uso |
|--------|-------------|-----|
| `200` | OK | GET exitoso |
| `201` | Created | POST exitoso (retorna `{ "id": ... }`) |
| `204` | No Content | PUT/DELETE exitoso |
| `400` | Bad Request | Validación fallida o datos incorrectos |
| `404` | Not Found | Recurso no encontrado |
| `500` | Server Error | Error interno del servidor |

---

## 5. Patrones de Diseño Implementados

### 5.1 Repository Pattern
- **Interfaces** en `Application/Repositories/` (ej: `IEspecieRepository`)
- **Implementaciones** en `Infrastructure/Repositories/Sql/` (ej: `EspecieRepository`)
- **Base genérica** en `Core.Infraestructure.Repositories.Sql/BaseRepository<T>`

```csharp
// Interfaz
public interface IEspecieRepository : IRepository<Especie>
{
    Task<Especie> GetByNombreAsync(string nombre);
    Task<IEnumerable<Especie>> GetActivasAsync();
}

// Implementación
public class EspecieRepository : BaseRepository<Especie>, IEspecieRepository
{
    public EspecieRepository(StoreDbContext context) : base(context) { }
    // implementación de métodos custom...
}
```

### 5.2 Soft Delete
Ninguna entidad se elimina físicamente. El campo `Activo` se establece en `false`.

### 5.3 Factory Pattern
`DatabaseFactory` selecciona el proveedor de base de datos según la configuración:
- `SQLSERVER` → `UseSqlServer()` + Migrations
- `SQLITE` → `UseSqlite()` + EnsureCreated()
- `MONGODB` → Conexión MongoDB

### 5.4 CQRS (Command Query Responsibility Segregation)
Implementado para Especie con MediatR:
- **Commands:** `CreateEspecieCommand`, `UpdateEspecieCommand`, `DeleteEspecieCommand`
- **Queries:** `GetAllEspeciesQuery`, `GetEspecieByIdQuery`

Los demás controllers usan repositorios directamente por simplicidad.

---

## 6. Base de Datos

### 6.1 Proveedor
**SQLite** via `Microsoft.EntityFrameworkCore.Sqlite`

### 6.2 Conexión
Configurada en `appsettings.json`:
```json
{
  "Configurations": { "UseDatabase": "sqlite" },
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=VeterinariaDB.sqlite"
  }
}
```

### 6.3 Tablas

| Tabla | PK | Índices |
|-------|----|---------|
| `Especies` | `Id` (int, autoincrement) | - |
| `Razas` | `Id` (int, autoincrement) | `IX_Razas_EspecieId` |
| `Propietarios` | `Id` (GUID) | `IX_Propietarios_DNI` (unique) |
| `Pacientes` | `Id` (GUID) | `IX_Pacientes_EspecieId`, `IX_Pacientes_PropietarioId`, `IX_Pacientes_RazaId` |

### 6.4 Restricciones FK

| Tabla | FK | Referencia | On Delete |
|-------|----|-----------|-----------|
| `Razas` | `EspecieId` | `Especies.Id` | RESTRICT |
| `Pacientes` | `EspecieId` | `Especies.Id` | RESTRICT |
| `Pacientes` | `RazaId` | `Razas.Id` | RESTRICT |
| `Pacientes` | `PropietarioId` | `Propietarios.Id` | RESTRICT |

---

## 7. Tests Unitarios

### 7.1 Framework
- **xUnit** con proyecto `Domain.Tests`

### 7.2 Cobertura

| Clase de Test | Entidad | Pruebas | Aspectos cubiertos |
|---------------|---------|---------|-------------------|
| `EspecieTests` | Especie | 5 | Creación, validación, activar/desactivar, actualizar |
| `RazaTests` | Raza | 5 | Creación, validación, cambio de especie |
| `PropietarioTests` | Propietario | 7 | Creación, validación, email, DNI, actualizar |
| `PacienteTests` | Paciente | 10 | Creación, validación sexo, edad, cambio propietario/especie |

**Total: 27 tests** — todos aprobados ✅

### 7.3 Ejecutar tests
```bash
dotnet test Domain.Tests/Domain.Tests.csproj
```

---

## 8. Guía para Agregar Nuevas Entidades

Para agregar una nueva entidad al sistema, seguir estos pasos:

1. **Domain/Entities/** → Crear clase heredando `DomainEntity<TKey, TValidator>`
2. **Domain/Validators/** → Crear validador heredando `EntityValidator<T>`
3. **Application/Repositories/** → Crear interfaz `IXxxRepository : IRepository<T>`
4. **Infrastructure/Repositories/Sql/** → Implementar repo heredando `BaseRepository<T>`
5. **Infrastructure/Repositories/Sql/StoreDbContext.cs** → Agregar `DbSet<T>` y configurar en `OnModelCreating`
6. **Infrastructure/Factories/DatabaseFactory.cs** → Registrar en `RegisterSqlRepositories()`
7. **Application/DataTransferObjects/** → Crear DTO para respuestas API
8. **Template-API/Controllers/** → Crear controller heredando `BaseController`
9. **Domain.Tests/** → Crear tests unitarios
10. **Compilar y verificar:** `dotnet build && dotnet test`
