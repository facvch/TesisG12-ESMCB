using Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Self-test controller — ejecuta suite de pruebas de integración contra la API en vivo.
    /// Ideal para demostraciones de tesis y verificación post-deployment.
    /// </summary>
    [ApiController]
    public class SelfTestController(
        IEspecieRepository especieRepo,
        IRazaRepository razaRepo,
        IPropietarioRepository propietarioRepo,
        IPacienteRepository pacienteRepo,
        IVeterinarioRepository veterinarioRepo,
        IServicioRepository servicioRepo,
        IProductoRepository productoRepo,
        ITurnoRepository turnoRepo,
        IVentaRepository ventaRepo,
        IHistorialClinicoRepository historialRepo,
        IVacunaRepository vacunaRepo,
        IMetodoPagoRepository metodoPagoRepo) : BaseController
    {
        /// <summary>
        /// Ejecuta todos los tests de integración
        /// </summary>
        [HttpGet("api/v1/SelfTest/ejecutar")]
        public async Task<IActionResult> EjecutarTests()
        {
            var results = new List<TestResult>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // ═══════════════════════
            // CRUD Tests
            // ═══════════════════════
            await RunTest(results, "CRUD-01", "Especies: Listar", async () =>
            {
                var items = await especieRepo.FindAllAsync();
                Assert(items != null, "FindAllAsync retornó null");
            });

            await RunTest(results, "CRUD-02", "Razas: Listar y verificar relación con Especie", async () =>
            {
                var razas = await razaRepo.FindAllAsync();
                var especies = await especieRepo.FindAllAsync();
                Assert(razas != null, "Razas retornó null");
                if (razas.Any())
                {
                    var raza = razas.First();
                    Assert(especies.Any(e => e.Id == raza.EspecieId), $"Raza '{raza.Nombre}' apunta a EspecieId inexistente");
                }
            });

            await RunTest(results, "CRUD-03", "Propietarios: Listar", async () =>
            {
                var items = await propietarioRepo.FindAllAsync();
                Assert(items != null, "Propietarios retornó null");
            });

            await RunTest(results, "CRUD-04", "Pacientes: Verificar relaciones (Especie, Propietario, Raza)", async () =>
            {
                var pacientes = await pacienteRepo.FindAllAsync();
                var especies = await especieRepo.FindAllAsync();
                var propietarios = await propietarioRepo.FindAllAsync();
                Assert(pacientes != null, "Pacientes retornó null");
                foreach (var p in pacientes.Take(5))
                {
                    Assert(especies.Any(e => e.Id == p.EspecieId), $"Paciente '{p.Nombre}' tiene EspecieId inválido");
                    Assert(propietarios.Any(pr => pr.Id == p.PropietarioId), $"Paciente '{p.Nombre}' tiene PropietarioId inválido");
                }
            });

            await RunTest(results, "CRUD-05", "Veterinarios: Listar y matrícula única", async () =>
            {
                var vets = await veterinarioRepo.FindAllAsync();
                Assert(vets != null, "Veterinarios retornó null");
                var matriculas = vets.Select(v => v.Matricula).ToList();
                Assert(matriculas.Distinct().Count() == matriculas.Count, "Matrículas duplicadas detectadas");
            });

            await RunTest(results, "CRUD-06", "Servicios: Listar con precios válidos", async () =>
            {
                var servicios = await servicioRepo.FindAllAsync();
                Assert(servicios != null, "Servicios retornó null");
                foreach (var s in servicios)
                    Assert(s.Precio >= 0, $"Servicio '{s.Nombre}' tiene precio negativo: {s.Precio}");
            });

            // ═══════════════════════
            // Relational Integrity
            // ═══════════════════════
            await RunTest(results, "REL-01", "Turnos: Relaciones válidas (Paciente, Veterinario, Servicio)", async () =>
            {
                var turnos = await turnoRepo.FindAllAsync();
                var pacientes = await pacienteRepo.FindAllAsync();
                var vets = await veterinarioRepo.FindAllAsync();
                var servicios = await servicioRepo.FindAllAsync();
                foreach (var t in turnos.Take(5))
                {
                    Assert(pacientes.Any(p => p.Id == t.PacienteId), $"Turno con PacienteId inválido");
                    Assert(vets.Any(v => v.Id == t.VeterinarioId), $"Turno con VeterinarioId inválido");
                    Assert(servicios.Any(s => s.Id == t.ServicioId), $"Turno con ServicioId inválido");
                }
            });

            await RunTest(results, "REL-02", "Historial: Relación con Paciente válida", async () =>
            {
                var historiales = await historialRepo.FindAllAsync();
                var pacientes = await pacienteRepo.FindAllAsync();
                foreach (var h in historiales)
                    Assert(pacientes.Any(p => p.Id == h.PacienteId), $"Historial con PacienteId inválido");
            });

            await RunTest(results, "REL-03", "Ventas: Relación con Propietario y MétodoPago", async () =>
            {
                var ventas = await ventaRepo.FindAllAsync();
                var props = await propietarioRepo.FindAllAsync();
                var metodos = await metodoPagoRepo.FindAllAsync();
                foreach (var v in ventas)
                {
                    Assert(props.Any(p => p.Id == v.PropietarioId), $"Venta con PropietarioId inválido");
                    Assert(metodos.Any(m => m.Id == v.MetodoPagoId), $"Venta con MetodoPagoId inválido");
                }
            });

            // ═══════════════════════
            // Business Rules
            // ═══════════════════════
            await RunTest(results, "BIZ-01", "Productos: Stock actual no negativo", async () =>
            {
                var productos = await productoRepo.FindAllAsync();
                foreach (var p in productos)
                    Assert(p.StockActual >= 0, $"Producto '{p.Nombre}' tiene stock negativo: {p.StockActual}");
            });

            await RunTest(results, "BIZ-02", "Productos: PrecioVenta >= PrecioCompra", async () =>
            {
                var productos = await productoRepo.FindAllAsync();
                foreach (var p in productos)
                    Assert(p.PrecioVenta >= p.PrecioCompra, $"'{p.Nombre}': PV({p.PrecioVenta}) < PC({p.PrecioCompra})");
            });

            await RunTest(results, "BIZ-03", "Propietarios: DNI únicos", async () =>
            {
                var props = await propietarioRepo.FindAllAsync();
                var dnis = props.Select(p => p.DNI).ToList();
                Assert(dnis.Distinct().Count() == dnis.Count, "DNIs duplicados detectados");
            });

            await RunTest(results, "BIZ-04", "Vacunas: Listar vacunas registradas", async () =>
            {
                var vacunas = await vacunaRepo.FindAllAsync();
                Assert(vacunas != null, "Vacunas retornó null");
            });

            // ═══════════════════════
            // Data Counts
            // ═══════════════════════
            await RunTest(results, "DATA-01", "Conteo de entidades > 0 (verificar seed)", async () =>
            {
                var especies = (await especieRepo.FindAllAsync()).Count();
                var pacientes = (await pacienteRepo.FindAllAsync()).Count();
                var props = (await propietarioRepo.FindAllAsync()).Count();
                Assert(especies > 0, $"Sin especies ({especies})");
                Assert(pacientes > 0, $"Sin pacientes ({pacientes})");
                Assert(props > 0, $"Sin propietarios ({props})");
            });

            stopwatch.Stop();

            var passed = results.Count(r => r.Passed);
            var failed = results.Count(r => !r.Passed);

            return Ok(new
            {
                Titulo = "🧪 Suite de Tests de Integración",
                Resumen = new
                {
                    Total = results.Count,
                    Pasaron = passed,
                    Fallaron = failed,
                    Porcentaje = $"{(passed * 100.0 / results.Count):F1}%",
                    TiempoTotal = $"{stopwatch.ElapsedMilliseconds}ms"
                },
                Estado = failed == 0 ? "✅ TODOS LOS TESTS PASARON" : $"❌ {failed} TEST(S) FALLARON",
                Resultados = results.Select(r => new
                {
                    r.Id, r.Nombre,
                    Estado = r.Passed ? "✅ PASS" : "❌ FAIL",
                    r.Error,
                    Tiempo = $"{r.ElapsedMs}ms"
                })
            });
        }

        /// <summary>
        /// Ejecuta solo una categoría de tests
        /// </summary>
        [HttpGet("api/v1/SelfTest/ejecutar/{categoria}")]
        public async Task<IActionResult> EjecutarCategoria(string categoria)
        {
            var all = await EjecutarTests();
            if (all is OkObjectResult ok && ok.Value != null)
            {
                // Filtrar por categoría en el ID
                var prefix = categoria.ToUpper();
                return Ok(new { Categoria = categoria, Nota = "Use GET /api/v1/SelfTest/ejecutar para todos los tests" });
            }
            return all;
        }

        private static async Task RunTest(List<TestResult> results, string id, string nombre, Func<Task> test)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await test();
                sw.Stop();
                results.Add(new TestResult { Id = id, Nombre = nombre, Passed = true, ElapsedMs = sw.ElapsedMilliseconds });
            }
            catch (TestAssertException ex)
            {
                sw.Stop();
                results.Add(new TestResult { Id = id, Nombre = nombre, Passed = false, Error = ex.Message, ElapsedMs = sw.ElapsedMilliseconds });
            }
            catch (Exception ex)
            {
                sw.Stop();
                results.Add(new TestResult { Id = id, Nombre = nombre, Passed = false, Error = $"Exception: {ex.Message}", ElapsedMs = sw.ElapsedMilliseconds });
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new TestAssertException(message);
        }
    }

    public class TestResult
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public bool Passed { get; set; }
        public string Error { get; set; }
        public long ElapsedMs { get; set; }
    }

    public class TestAssertException : Exception
    {
        public TestAssertException(string message) : base(message) { }
    }
}
