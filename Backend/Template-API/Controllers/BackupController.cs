using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Controllers
{
    /// <summary>
    /// Controller para backup y restauración de la base de datos SQLite
    /// </summary>
    [ApiController]
    public class BackupController(IWebHostEnvironment env) : BaseController
    {
        private string BackupDir => Path.Combine(env.ContentRootPath, "Backups");
        private string DbPath => Path.Combine(env.ContentRootPath, "VeterinariaDB.sqlite");

        /// <summary>
        /// Crea un backup de la base de datos
        /// </summary>
        [HttpPost("api/v1/Backup/crear")]
        public async Task<IActionResult> CrearBackup([FromQuery] string descripcion = "")
        {
            try
            {
                if (!System.IO.File.Exists(DbPath))
                    return NotFound("No se encontró el archivo de base de datos.");

                if (!Directory.Exists(BackupDir))
                    Directory.CreateDirectory(BackupDir);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"backup_{timestamp}.sqlite";
                var backupPath = Path.Combine(BackupDir, fileName);

                // Usar backup nativo de SQLite
                await using var sourceDb = new SqliteConnection($"Data Source={DbPath}");
                await using var backupDb = new SqliteConnection($"Data Source={backupPath}");
                await sourceDb.OpenAsync();
                await backupDb.OpenAsync();
                sourceDb.BackupDatabase(backupDb);

                // Guardar metadata
                var metaPath = Path.Combine(BackupDir, $"backup_{timestamp}.json");
                var metadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Archivo = fileName,
                    FechaCreacion = DateTime.Now,
                    Descripcion = descripcion,
                    TamañoBytes = new FileInfo(backupPath).Length,
                    Entorno = env.EnvironmentName
                });
                await System.IO.File.WriteAllTextAsync(metaPath, metadata);

                return Ok(new
                {
                    Mensaje = "✅ Backup creado exitosamente",
                    Archivo = fileName,
                    Tamaño = FormatSize(new FileInfo(backupPath).Length),
                    Ruta = backupPath,
                    FechaCreacion = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensaje = "Error al crear backup", Error = ex.Message });
            }
        }

        /// <summary>
        /// Lista los backups disponibles
        /// </summary>
        [HttpGet("api/v1/Backup/listar")]
        public IActionResult ListarBackups()
        {
            if (!Directory.Exists(BackupDir))
                return Ok(new { Total = 0, Items = new List<object>() });

            var backups = Directory.GetFiles(BackupDir, "*.sqlite")
                .Select(f =>
                {
                    var info = new FileInfo(f);
                    return new
                    {
                        Archivo = info.Name,
                        Tamaño = FormatSize(info.Length),
                        TamañoBytes = info.Length,
                        FechaCreacion = info.CreationTime,
                        FechaModificacion = info.LastWriteTime
                    };
                })
                .OrderByDescending(b => b.FechaCreacion)
                .ToList();

            return Ok(new
            {
                Total = backups.Count,
                TamañoTotal = FormatSize(backups.Sum(b => b.TamañoBytes)),
                Items = backups
            });
        }

        /// <summary>
        /// Descarga un archivo de backup
        /// </summary>
        [HttpGet("api/v1/Backup/descargar/{archivo}")]
        public IActionResult DescargarBackup(string archivo)
        {
            if (archivo.Contains("..") || archivo.Contains("/") || archivo.Contains("\\"))
                return BadRequest("Nombre de archivo inválido");

            var filePath = Path.Combine(BackupDir, archivo);
            if (!System.IO.File.Exists(filePath))
                return NotFound($"No se encontró el backup: {archivo}");

            return PhysicalFile(filePath, "application/x-sqlite3", archivo);
        }

        /// <summary>
        /// Elimina un backup específico
        /// </summary>
        [HttpDelete("api/v1/Backup/eliminar/{archivo}")]
        public IActionResult EliminarBackup(string archivo)
        {
            if (archivo.Contains("..") || archivo.Contains("/") || archivo.Contains("\\"))
                return BadRequest("Nombre de archivo inválido");

            var filePath = Path.Combine(BackupDir, archivo);
            if (!System.IO.File.Exists(filePath))
                return NotFound($"No se encontró el backup: {archivo}");

            System.IO.File.Delete(filePath);
            var metaPath = Path.ChangeExtension(filePath, ".json");
            if (System.IO.File.Exists(metaPath))
                System.IO.File.Delete(metaPath);

            return Ok(new { Mensaje = $"✅ Backup '{archivo}' eliminado exitosamente" });
        }

        /// <summary>
        /// Información del sistema y la base de datos
        /// </summary>
        [HttpGet("api/v1/Backup/info")]
        public async Task<IActionResult> InfoSistema()
        {
            var dbInfo = System.IO.File.Exists(DbPath) ? new FileInfo(DbPath) : null;

            long backupTotalSize = 0;
            int backupCount = 0;
            if (Directory.Exists(BackupDir))
            {
                var files = Directory.GetFiles(BackupDir, "*.sqlite");
                backupCount = files.Length;
                backupTotalSize = files.Sum(f => new FileInfo(f).Length);
            }

            // Contar registros por tabla
            var tablas = new Dictionary<string, long>();
            if (dbInfo != null)
            {
                await using var conn = new SqliteConnection($"Data Source={DbPath}");
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EF%'";
                    using var reader = await cmd.ExecuteReaderAsync();
                    var nombres = new List<string>();
                    while (await reader.ReadAsync())
                        nombres.Add(reader.GetString(0));

                    foreach (var nombre in nombres)
                    {
                        try
                        {
                            using var countCmd = conn.CreateCommand();
                            countCmd.CommandText = $"SELECT COUNT(*) FROM \"{nombre}\"";
                            tablas[nombre] = (long)(await countCmd.ExecuteScalarAsync() ?? 0);
                        }
                        catch { tablas[nombre] = -1; }
                    }
                }
            }

            return Ok(new
            {
                Sistema = new
                {
                    FechaActual = DateTime.Now,
                    Entorno = env.EnvironmentName,
                    DotNetVersion = Environment.Version.ToString()
                },
                BaseDeDatos = new
                {
                    Archivo = "VeterinariaDB.sqlite",
                    Tamaño = dbInfo != null ? FormatSize(dbInfo.Length) : "N/A",
                    TamañoBytes = dbInfo?.Length ?? 0,
                    UltimaModificacion = dbInfo?.LastWriteTime,
                    TotalTablas = tablas.Count,
                    TotalRegistros = tablas.Values.Where(v => v >= 0).Sum(),
                    Tablas = tablas.OrderByDescending(t => t.Value).Select(t => new
                    {
                        Tabla = t.Key,
                        Registros = t.Value
                    })
                },
                Backups = new
                {
                    Directorio = BackupDir,
                    CantidadBackups = backupCount,
                    TamañoTotal = FormatSize(backupTotalSize)
                }
            });
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}
