# Manual del Operador - Sistema de Gestión Veterinaria

**Versión:** 1.0  
**Fecha:** Febrero 2026  
**Destinado a:** Operadores del sistema, personal de IT y administradores de infraestructura

---

## 1. Requisitos del Sistema

### 1.1 Requisitos de Software (Servidor)

| Componente | Versión mínima |
|------------|---------------|
| .NET Runtime | 8.0 o superior |
| .NET SDK (para compilar) | 8.0 o superior |
| Sistema Operativo | Windows 10+, Linux, macOS |
| SQLite | Incluido (no requiere instalación separada) |

### 1.2 Requisitos de Hardware (Servidor)

| Recurso | Mínimo | Recomendado |
|---------|--------|-------------|
| CPU | 2 cores | 4 cores |
| RAM | 2 GB | 4 GB |
| Disco | 1 GB libre | 5 GB libre |
| Red | Conexión de red local | - |

### 1.3 Requisitos del Cliente

- Navegador web moderno (Chrome 90+, Edge 90+, Firefox 88+)
- Resolución mínima: 1280x720
- Conexión de red al servidor

---

## 2. Instalación y Configuración

### 2.1 Instalación del SDK .NET (si no está instalado)

**Windows:**
```powershell
# Descargar desde https://dotnet.microsoft.com/download/dotnet/8.0
# O via winget:
winget install Microsoft.DotNet.SDK.8
```

**Verificar instalación:**
```bash
dotnet --version
```

### 2.2 Clonar/Copiar el Proyecto

Copiar la carpeta del proyecto al servidor destino:
```
Base/Backend/  → Carpeta de la aplicación backend
```

### 2.3 Restaurar Dependencias

```bash
cd Base/Backend
dotnet restore HybridDDDArchitecture.sln
```

### 2.4 Compilar la Solución

```bash
dotnet build HybridDDDArchitecture.sln
```

**Resultado esperado:** `Compilación correcta. 0 Errores.`

### 2.5 Configuración del Archivo `appsettings.json`

Ubicación: `Base/Backend/Template-API/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Configurations": {
    "UseDatabase": "sqlite"
  },
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=VeterinariaDB.sqlite"
  }
}
```

#### Opciones configurables:

| Parámetro | Valores posibles | Descripción |
|-----------|-----------------|-------------|
| `UseDatabase` | `sqlite`, `sqlserver`, `mongodb` | Motor de base de datos |
| `SqliteConnection` | Ruta al archivo .sqlite | Ubicación de la BD SQLite |
| `SqlConnection` | Connection string SQL Server | Para uso con SQL Server |
| `LogLevel:Default` | `Information`, `Warning`, `Error` | Nivel de detalle de logs |
| `AllowedHosts` | `*` o dominios específicos | Hosts permitidos |

#### Cambiar la ubicación de la base de datos:
```json
"SqliteConnection": "Data Source=C:/datos/VeterinariaDB.sqlite"
```

---

## 3. Ejecutar la Aplicación

### 3.1 Modo Desarrollo
```bash
cd Base/Backend/Template-API
dotnet run
```

La API escuchará en:
- `https://localhost:7204` (HTTPS)
- `http://localhost:5204` (HTTP)

### 3.2 Modo Producción
```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet Template-API.dll
```

### 3.3 Cambiar Puertos
Editar `Properties/launchSettings.json` o usar variables de entorno:
```bash
dotnet run --urls "http://0.0.0.0:8080"
```

### 3.4 Verificar que la API está funcionando
```
http://localhost:5204/swagger         → Documentación Swagger
http://localhost:5204/api/v1/Especie  → Debe retornar JSON
```

---

## 4. Base de Datos SQLite

### 4.1 Ubicación
El archivo `VeterinariaDB.sqlite` se crea automáticamente en la carpeta del proyecto `Template-API/` al primer inicio.

### 4.2 Creación Automática de Tablas
Al iniciar la aplicación, `EnsureCreated()` verifica si las tablas existen. Si no existen, las crea automáticamente:
- `Especies`
- `Razas`
- `Propietarios`
- `Pacientes`
- `Alumnos` (legacy)
- `DummyEntity` (legacy)

### 4.3 Backup de la Base de Datos

**Manual:**
```bash
# Detener la aplicación primero
copy VeterinariaDB.sqlite VeterinariaDB_backup_20260212.sqlite
```

**Script de backup automatizado (Windows):**
```powershell
# backup_db.ps1
$fecha = Get-Date -Format "yyyyMMdd_HHmmss"
$origen = "Base\Backend\Template-API\VeterinariaDB.sqlite"
$destino = "Backups\VeterinariaDB_$fecha.sqlite"
Copy-Item $origen $destino
Write-Host "Backup creado: $destino"
```

**Programar backup diario (Task Scheduler):**
1. Abrir "Programador de tareas" de Windows
2. Crear tarea básica
3. Establecer frecuencia: Diaria
4. Acción: Iniciar programa → `powershell.exe`
5. Argumentos: `-File "C:\ruta\backup_db.ps1"`

### 4.4 Restaurar Backup
```bash
# Detener la aplicación
# Reemplazar el archivo
copy VeterinariaDB_backup.sqlite VeterinariaDB.sqlite
# Reiniciar la aplicación
```

### 4.5 Consultar la Base de Datos Directamente

Instalar **DB Browser for SQLite** (https://sqlitebrowser.org/) para inspeccionar y consultar la BD.

```sql
-- Ejemplo: Ver todas las especies activas
SELECT * FROM Especies WHERE Activo = 1;

-- Ver pacientes con sus propietarios
SELECT p.Nombre AS Mascota, pr.Nombre || ' ' || pr.Apellido AS Dueno
FROM Pacientes p
JOIN Propietarios pr ON p.PropietarioId = pr.Id
WHERE p.Activo = 1;
```

---

## 5. Monitoreo y Logs

### 5.1 Logs de Consola
Al ejecutar la aplicación, los logs se muestran en consola:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5204
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[], CommandType='...]
```

### 5.2 Niveles de Log

| Nivel | Descripción | Cuándo usar |
|-------|-------------|-------------|
| `Trace/Debug` | Detalle máximo | Solo en desarrollo |
| `Information` | Operaciones normales | Desarrollo y staging |
| `Warning` | Situaciones anómalas no críticas | Producción |
| `Error` | Errores que afectan funcionalidad | Siempre |

### 5.3 Cambiar Nivel de Log
En `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Warning",
    "Microsoft.EntityFrameworkCore": "Error"
  }
}
```

---

## 6. Solución de Problemas

### 6.1 La API no inicia

| Síntoma | Causa probable | Solución |
|---------|---------------|----------|
| Puerto en uso | Otra instancia corriendo | Cerrar proceso previo o cambiar puerto |
| Error de certificado SSL | Certificado dev no confiable | Usar HTTP (puerto 5204) o ejecutar `dotnet dev-certs https --trust` |
| Error en base de datos | Archivo .sqlite corrupto | Restaurar backup |

### 6.2 Errores de Base de Datos

**"database is locked":**
- Cerrar otras aplicaciones que accedan al archivo .sqlite
- Verificar que solo una instancia de la API esté corriendo

**Tablas no se crean:**
- Eliminar el archivo `VeterinariaDB.sqlite`
- Reiniciar la aplicación (las tablas se recrean automáticamente)

### 6.3 Errores HTTP comunes

| Código | Significado | Acción |
|--------|-------------|--------|
| 400 | Datos inválidos | Verificar los campos enviados en el request |
| 404 | Recurso no encontrado | Verificar que el ID existe |
| 500 | Error interno | Revisar logs de la consola |

---

## 7. Mantenimiento Preventivo

### 7.1 Tareas Periódicas

| Frecuencia | Tarea | Descripción |
|------------|-------|-------------|
| **Diaria** | Backup de BD | Copiar `VeterinariaDB.sqlite` |
| **Semanal** | Revisar logs | Verificar que no haya errores recurrentes |
| **Mensual** | Actualizar .NET | `dotnet --list-sdks` para ver versión actual |
| **Trimestral** | Optimizar BD | Ejecutar `VACUUM` en SQLite |

### 7.2 Optimización de SQLite
```sql
-- Ejecutar en DB Browser for SQLite
VACUUM;
ANALYZE;
```

### 7.3 Verificar Estado del Sistema
```powershell
# Verificar que la API responde
Invoke-RestMethod -Uri "http://localhost:5204/api/v1/Especie" -Method Get

# Verificar tamaño de la BD
Get-Item "Base\Backend\Template-API\VeterinariaDB.sqlite" | Select-Object Name, Length
```

---

## 8. Seguridad

### 8.1 Recomendaciones Básicas

- **No exponer** la API directamente a Internet sin un reverse proxy (Nginx, IIS)
- **Configurar CORS** adecuadamente en `Program.cs`
- **Implementar JWT** para autenticación (pendiente, planificado en Sprint 1)
- **Cambiar `AllowedHosts`** de `"*"` a los dominios específicos en producción

### 8.2 Protección del Archivo SQLite
- Ubicar el archivo `.sqlite` fuera de la carpeta pública del servidor
- Establecer permisos de archivo restrictivos (solo lectura/escritura para el usuario de la aplicación)
- Nunca compartir o versionar el archivo `.sqlite` en Git

---

## 9. Contacto y Soporte

Para soporte técnico, contactar al equipo de desarrollo con la siguiente información:
- **Descripción del problema**
- **Logs relevantes** (copiar salida de la consola)
- **Captura de pantalla** (si aplica)
- **Fecha y hora** del incidente
- **Versión del sistema** (`dotnet --version`)
