using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    /// <summary>
    /// Controller para poblar la base de datos con datos de ejemplo realistas.
    /// Ideal para demostraciones de la tesis.
    /// </summary>
    [ApiController]
    public class SeedController(
        IEspecieRepository especieRepo,
        IRazaRepository razaRepo,
        IPropietarioRepository propietarioRepo,
        IPacienteRepository pacienteRepo,
        IVeterinarioRepository veterinarioRepo,
        IServicioRepository servicioRepo,
        IVacunaRepository vacunaRepo,
        ITurnoRepository turnoRepo,
        IHistorialClinicoRepository historialRepo,
        IRegistroVacunacionRepository vacunacionRepo,
        ITratamientoRepository tratamientoRepo,
        IProductoRepository productoRepo,
        IVentaRepository ventaRepo,
        IDetalleVentaRepository detalleVentaRepo,
        IMetodoPagoRepository metodoPagoRepo,
        ICategoriaRepository categoriaRepo,
        IMarcaRepository marcaRepo,
        IProveedorRepository proveedorRepo,
        IDepositoRepository depositoRepo) : BaseController
    {
        /// <summary>
        /// Pobla la base de datos con datos de ejemplo completos e interrelacionados
        /// </summary>
        [HttpPost("api/v1/Seed/completo")]
        public async Task<IActionResult> SeedCompleto()
        {
            var especiesExistentes = await especieRepo.FindAllAsync();
            if (especiesExistentes.Any())
                return BadRequest("Ya existen datos. Elimine la DB primero para re-seedear.");

            var resumen = new Dictionary<string, int>();

            // ═══════════════════════
            // 1. ESPECIES Y RAZAS
            // ═══════════════════════
            var canino = new Especie("Canino", "Perros domésticos");
            var felino = new Especie("Felino", "Gatos domésticos");
            var ave = new Especie("Ave", "Aves domésticas y exóticas");
            var roedor = new Especie("Roedor", "Roedores domésticos");
            await especieRepo.AddAsync(canino);
            await especieRepo.AddAsync(felino);
            await especieRepo.AddAsync(ave);
            await especieRepo.AddAsync(roedor);
            resumen["Especies"] = 4;

            // Raza(string nombre, int especieId, string descripcion = "")
            var razas = new List<Raza>
            {
                new("Labrador Retriever", canino.Id, "Raza grande, amigable"),
                new("Pastor Alemán", canino.Id, "Raza grande, inteligente"),
                new("Golden Retriever", canino.Id, "Raza grande, cariñosa"),
                new("Bulldog Francés", canino.Id, "Raza pequeña, compañía"),
                new("Caniche", canino.Id, "Raza mediana, hipoalergénica"),
                new("Mestizo", canino.Id, "Sin raza definida"),
                new("Siamés", felino.Id, "Gato elegante, vocal"),
                new("Persa", felino.Id, "Gato de pelo largo"),
                new("Común Europeo", felino.Id, "Gato doméstico estándar"),
                new("Loro", ave.Id, "Ave parlante"),
                new("Canario", ave.Id, "Ave cantora"),
                new("Hámster", roedor.Id, "Roedor pequeño"),
                new("Cobayo", roedor.Id, "Cuy, roedor mediano"),
            };
            foreach (var r in razas) await razaRepo.AddAsync(r);
            resumen["Razas"] = razas.Count;

            // ═══════════════════════
            // 2. PROPIETARIOS
            // ═══════════════════════
            var props = new List<Propietario>
            {
                new("Juan", "Pérez", "30555111", "1155550001", "juan@email.com", "Av. Mitre 1234"),
                new("María", "García", "30555222", "1155550002", "maria@email.com", "Calle 9 de Julio 567"),
                new("Carlos", "López", "30555333", "1155550003", "carlos@email.com", "Belgrano 890"),
                new("Ana", "Martínez", "30555444", "1155550004", "ana@email.com", "San Martín 321"),
                new("Roberto", "Fernández", "30555555", "1155550005", "roberto@email.com", "Rivadavia 456"),
                new("Laura", "Rodríguez", "30555666", "1155550006", "laura@email.com", "Sarmiento 789"),
                new("Diego", "Sánchez", "30555777", "1155550007", "diego@email.com", "Moreno 1011"),
                new("Patricia", "Torres", "30555888", "1155550008", "patricia@email.com", "Lavalle 1213"),
            };
            foreach (var p in props) await propietarioRepo.AddAsync(p);
            resumen["Propietarios"] = props.Count;

            // ═══════════════════════
            // 3. PACIENTES
            // Paciente(nombre, especieId, propietarioId, sexo, razaId?, fechaNacimiento?)
            // ═══════════════════════
            var pacientes = new List<Paciente>
            {
                new("Rex", canino.Id, props[0].Id, "Macho", razas[0].Id, DateTime.Today.AddYears(-3)),
                new("Luna", canino.Id, props[0].Id, "Hembra", razas[2].Id, DateTime.Today.AddYears(-2)),
                new("Max", canino.Id, props[1].Id, "Macho", razas[1].Id, DateTime.Today.AddYears(-5)),
                new("Mia", felino.Id, props[1].Id, "Hembra", razas[6].Id, DateTime.Today.AddYears(-1)),
                new("Rocky", canino.Id, props[2].Id, "Macho", razas[3].Id, DateTime.Today.AddMonths(-8)),
                new("Nina", canino.Id, props[3].Id, "Hembra", razas[4].Id, DateTime.Today.AddYears(-4)),
                new("Simón", felino.Id, props[4].Id, "Macho", razas[7].Id, DateTime.Today.AddYears(-2)),
                new("Coco", ave.Id, props[5].Id, "Macho", razas[9].Id, DateTime.Today.AddYears(-6)),
                new("Toby", canino.Id, props[6].Id, "Macho", razas[5].Id, DateTime.Today.AddYears(-7)),
                new("Mishi", felino.Id, props[7].Id, "Hembra", razas[8].Id, DateTime.Today.AddMonths(-10)),
                new("Pelusa", roedor.Id, props[3].Id, "Hembra", razas[11].Id, DateTime.Today.AddMonths(-4)),
                new("Firulais", canino.Id, props[5].Id, "Macho", razas[5].Id, DateTime.Today.AddYears(-1)),
            };
            foreach (var p in pacientes) await pacienteRepo.AddAsync(p);
            resumen["Pacientes"] = pacientes.Count;

            // ═══════════════════════
            // 4. VETERINARIOS
            // Veterinario(nombre, apellido, matricula, telefono, email?, especialidad?)
            // ═══════════════════════
            var vets = new List<Veterinario>
            {
                new("Alejandro", "Gómez", "MP-1001", "1140001001", "agomez@vet.com", "Clínica General"),
                new("Valentina", "Ruiz", "MP-1002", "1140001002", "vruiz@vet.com", "Cirugía"),
                new("Martín", "Díaz", "MP-1003", "1140001003", "mdiaz@vet.com", "Dermatología"),
            };
            foreach (var v in vets) await veterinarioRepo.AddAsync(v);
            resumen["Veterinarios"] = vets.Count;

            // ═══════════════════════
            // 5. SERVICIOS
            // Servicio(nombre, descripcion, duracionMinutos, precio)
            // ═══════════════════════
            var servicios = new List<Servicio>
            {
                new("Consulta General", "Revisión clínica general", 30, 5000m),
                new("Vacunación", "Aplicación de vacunas", 15, 3500m),
                new("Cirugía menor", "Cirugías ambulatorias", 60, 15000m),
                new("Castración", "Esterilización quirúrgica", 45, 12000m),
                new("Limpieza dental", "Profilaxis dental", 40, 8000m),
                new("Análisis clínico", "Extracción y análisis de sangre", 20, 4500m),
                new("Ecografía", "Estudio por imágenes", 30, 6000m),
                new("Desparasitación", "Tratamiento antiparasitario", 10, 2500m),
            };
            foreach (var s in servicios) await servicioRepo.AddAsync(s);
            resumen["Servicios"] = servicios.Count;

            // ═══════════════════════
            // 6. VACUNAS
            // Vacuna(nombre, descripcion?, laboratorio?, intervaloDosisDias?)
            // ═══════════════════════
            var vacunas = new List<Vacuna>
            {
                new("Antirrábica", "Vacuna antirrábica obligatoria", "Laboratorio XYZ", 365),
                new("Quíntuple Canina", "Moquillo, Hepatitis, Parvo, Parainfluenza, Lepto", "Laboratorio ABC", 365),
                new("Triple Felina", "Panleucopenia, Rinotraqueitis, Calicivirus", "Laboratorio ABC", 365),
                new("Antitetánica", "Protección contra tétanos", "Laboratorio DEF", 365),
                new("Leucemia Felina", "Virus de Leucemia Felina (FeLV)", "Laboratorio ABC", 365),
            };
            foreach (var v in vacunas) await vacunaRepo.AddAsync(v);
            resumen["Vacunas"] = vacunas.Count;

            // ═══════════════════════
            // 7. TURNOS
            // Turno(pacienteId, veterinarioId, servicioId, fechaHora, duracionMinutos, motivo?, observaciones?)
            // ═══════════════════════
            var turnosList = new List<Turno>();
            for (int i = 1; i <= 5; i++)
            {
                var t = new Turno(pacientes[i % pacientes.Count].Id, vets[i % vets.Count].Id,
                    servicios[i % servicios.Count].Id, DateTime.Today.AddDays(i).AddHours(9 + i),
                    30, "Control de rutina");
                turnosList.Add(t);
            }
            for (int i = 1; i <= 8; i++)
            {
                var t = new Turno(pacientes[i % pacientes.Count].Id, vets[i % vets.Count].Id,
                    servicios[0].Id, DateTime.Today.AddDays(-i * 5).AddHours(10),
                    30, "Consulta de seguimiento");
                t.Completar("Paciente en buen estado");
                turnosList.Add(t);
            }
            foreach (var t in turnosList) await turnoRepo.AddAsync(t);
            resumen["Turnos"] = turnosList.Count;

            // ═══════════════════════
            // 8. HISTORIAL CLÍNICO
            // HistorialClinico(pacienteId, fecha, motivo, veterinario, sintomas?, diagnostico?, indicaciones?, peso?, temp?, obs?)
            // ═══════════════════════
            var historiales = new List<HistorialClinico>
            {
                new(pacientes[0].Id, DateTime.Today.AddMonths(-6), "Control anual", "Dr. Gómez",
                    "Buen estado general", "Saludable", "Control en 6 meses", 28.5m, 38.2m, "Peso ideal"),
                new(pacientes[0].Id, DateTime.Today.AddMonths(-2), "Diarrea", "Dr. Gómez",
                    "Deposiciones blandas, inapetencia", "Gastroenteritis leve", "Dieta blanda 3 días + probióticos", 27.8m, 38.5m, "Mejoría esperada en 48hs"),
                new(pacientes[2].Id, DateTime.Today.AddMonths(-3), "Cojera", "Dra. Ruiz",
                    "Cojera en pata trasera izq", "Esguince leve", "Reposo 1 semana + antiinflamatorio", 32m, 38.3m),
                new(pacientes[4].Id, DateTime.Today.AddMonths(-1), "Primera consulta", "Dr. Díaz",
                    "Cachorro saludable", "Sin patologías", "Plan de vacunación iniciado", 4.2m, 38.8m, "Cachorro en excelente estado"),
                new(pacientes[6].Id, DateTime.Today.AddMonths(-4), "Vómitos", "Dr. Gómez",
                    "Vómitos recurrentes, pelo opaco", "Bola de pelo", "Pasta de malta + cepillado diario", 4.5m, 38.6m, "Típico en persas"),
                new(pacientes[8].Id, DateTime.Today.AddDays(-10), "Herida", "Dra. Ruiz",
                    "Herida cortante en almohadilla", "Herida superficial", "Limpieza + antibiótico tópico", 18m, 38.4m),
            };
            foreach (var h in historiales) await historialRepo.AddAsync(h);
            resumen["HistorialesClínicos"] = historiales.Count;

            // ═══════════════════════
            // 9. VACUNACIONES
            // RegistroVacunacion(pacienteId, vacunaId, fechaAplicacion, veterinario, nroLote?, fechaProximaDosis?, obs?)
            // ═══════════════════════
            var vacunaciones = new List<RegistroVacunacion>
            {
                new(pacientes[0].Id, vacunas[0].Id, DateTime.Today.AddMonths(-6), "Dr. Gómez",
                    "Lote-2026A", DateTime.Today.AddMonths(6), "Sin reacciones"),
                new(pacientes[0].Id, vacunas[1].Id, DateTime.Today.AddMonths(-6), "Dr. Gómez",
                    "Lote-2026B", DateTime.Today.AddMonths(6)),
                new(pacientes[2].Id, vacunas[0].Id, DateTime.Today.AddMonths(-2), "Dra. Ruiz",
                    "Lote-2026C", DateTime.Today.AddMonths(10)),
                new(pacientes[3].Id, vacunas[2].Id, DateTime.Today.AddMonths(-4), "Dr. Díaz",
                    "Lote-2026D", DateTime.Today.AddMonths(8)),
                new(pacientes[4].Id, vacunas[1].Id, DateTime.Today.AddDays(-15), "Dr. Gómez",
                    "Lote-2026E", DateTime.Today.AddDays(-15).AddDays(365), "Primera dosis"),
            };
            foreach (var v in vacunaciones) await vacunacionRepo.AddAsync(v);
            resumen["Vacunaciones"] = vacunaciones.Count;

            // ═══════════════════════
            // 10. TRATAMIENTOS
            // Tratamiento(pacienteId, fecha, diagnostico, descripcion, veterinario, medicacion?, obs?)
            // ═══════════════════════
            var tratamientos = new List<Tratamiento>
            {
                new(pacientes[0].Id, DateTime.Today.AddMonths(-2), "Gastroenteritis", "Dieta blanda + probióticos",
                    "Dr. Gómez", "Probiótico veterinario 1 sobre/día x 5 días", "Recuperación completa"),
                new(pacientes[2].Id, DateTime.Today.AddMonths(-3), "Esguince", "Reposo + antiinflamatorio",
                    "Dra. Ruiz", "Meloxicam 0.1mg/kg x 5 días", "Mejoría al 3er día"),
                new(pacientes[8].Id, DateTime.Today.AddDays(-10), "Herida en almohadilla", "Curación + antibiótico",
                    "Dra. Ruiz", "Amoxicilina 20mg/kg c/12hs x 7 días"),
            };
            // Finalizar los dos primeros
            tratamientos[0].Finalizar();
            tratamientos[1].Finalizar();
            foreach (var t in tratamientos) await tratamientoRepo.AddAsync(t);
            resumen["Tratamientos"] = tratamientos.Count;

            // ═══════════════════════
            // 11. STOCK
            // ═══════════════════════
            var cats = new List<Categoria>
            {
                new("Medicamentos"), new("Alimentos"), new("Accesorios"), new("Higiene"),
            };
            foreach (var c in cats) await categoriaRepo.AddAsync(c);

            var marcas = new List<Marca>
            {
                new("Royal Canin"), new("Bayer"), new("Purina"), new("Holliday"),
            };
            foreach (var m in marcas) await marcaRepo.AddAsync(m);

            var provs = new List<Proveedor>
            {
                new("Distribuidora VetPlus", "30-70001111-9", "1140009001", "vetplus@dist.com", "Zona Norte", "Pedro"),
                new("Droguería Animal", "30-70002222-9", "1140009002", "drogueria@animal.com", "Zona Sur", "Ana"),
            };
            foreach (var p in provs) await proveedorRepo.AddAsync(p);

            var deps = new List<Deposito>
            {
                new("Depósito Principal", "Sala de atrás"),
                new("Refrigerados", "Heladera de medicamentos"),
            };
            foreach (var d in deps) await depositoRepo.AddAsync(d);

            // Producto(nombre, descripcion, codigoBarras, categoriaId, precioCompra, precioVenta, stockActual, stockMinimo, marcaId?, proveedorId?, depositoId?)
            var productos = new List<Producto>
            {
                new("Amoxicilina 250mg x 20 comp", "Antibiótico amplio espectro", "7790001001",
                    cats[0].Id, 1200m, 2500m, 50, 10, marcas[1].Id, provs[0].Id, deps[0].Id),
                new("Meloxicam 1.5mg/ml x 10ml", "Antiinflamatorio no esteroideo", "7790001002",
                    cats[0].Id, 1800m, 3500m, 30, 5, marcas[3].Id, provs[0].Id, deps[1].Id),
                new("Royal Canin Adult 15kg", "Alimento seco para perro adulto", "7790002001",
                    cats[1].Id, 18000m, 28000m, 20, 5, marcas[0].Id, provs[0].Id, deps[0].Id),
                new("Purina Cat Chow 8kg", "Alimento seco para gato", "7790002002",
                    cats[1].Id, 8000m, 12500m, 15, 5, marcas[2].Id, provs[1].Id, deps[0].Id),
                new("Collar antipulgas canino", "Collar antiparasitario externo", "7790003001",
                    cats[2].Id, 2000m, 4500m, 25, 10, marcas[1].Id, provs[1].Id, deps[0].Id),
                new("Shampoo dermatológico 250ml", "Shampoo medicado piel sensible", "7790004001",
                    cats[3].Id, 1500m, 3000m, 40, 10, marcas[3].Id, provs[0].Id, deps[0].Id),
                new("Probiótico veterinario x 10", "Suplemento para flora intestinal", "7790001003",
                    cats[0].Id, 900m, 1800m, 3, 10, marcas[3].Id, provs[0].Id, deps[1].Id),
                new("Jeringa descartable 5ml x 100", "Jeringas descartables uso veterinario", "7790003002",
                    cats[2].Id, 3000m, 5500m, 8, 20, marcas[1].Id, provs[1].Id, deps[0].Id),
            };
            foreach (var p in productos) await productoRepo.AddAsync(p);
            resumen["Productos"] = productos.Count;

            // ═══════════════════════
            // 12. MÉTODOS DE PAGO Y VENTAS
            // ═══════════════════════
            var metodos = new List<MetodoPago>
            {
                new("Efectivo"), new("Tarjeta de Débito"), new("Tarjeta de Crédito"),
                new("Transferencia"), new("Mercado Pago"),
            };
            foreach (var m in metodos) await metodoPagoRepo.AddAsync(m);

            // DetalleVenta(ventaId, productoId, descripcion, cantidad, precioUnitario)
            var v1 = new Venta(props[0].Id, metodos[0].Id, "Compra de alimento y medicación");
            await ventaRepo.AddAsync(v1);
            await detalleVentaRepo.AddAsync(new DetalleVenta(v1.Id, productos[2].Id, productos[2].Nombre, 1, productos[2].PrecioVenta));
            await detalleVentaRepo.AddAsync(new DetalleVenta(v1.Id, productos[0].Id, productos[0].Nombre, 2, productos[0].PrecioVenta));
            v1.ActualizarTotal(28000m + 2 * 2500m);
            v1.Confirmar();
            ventaRepo.Update(v1.Id, v1);

            var v2 = new Venta(props[1].Id, metodos[1].Id, "Accesorios");
            await ventaRepo.AddAsync(v2);
            await detalleVentaRepo.AddAsync(new DetalleVenta(v2.Id, productos[4].Id, productos[4].Nombre, 1, productos[4].PrecioVenta));
            await detalleVentaRepo.AddAsync(new DetalleVenta(v2.Id, productos[5].Id, productos[5].Nombre, 1, productos[5].PrecioVenta));
            v2.ActualizarTotal(4500m + 3000m);
            v2.Confirmar();
            ventaRepo.Update(v2.Id, v2);

            var v3 = new Venta(props[2].Id, metodos[4].Id, "Alimento felino");
            await ventaRepo.AddAsync(v3);
            await detalleVentaRepo.AddAsync(new DetalleVenta(v3.Id, productos[3].Id, productos[3].Nombre, 2, productos[3].PrecioVenta));
            v3.ActualizarTotal(2 * 12500m);
            v3.Confirmar();
            ventaRepo.Update(v3.Id, v3);

            resumen["Ventas"] = 3;
            resumen["MétodosPago"] = metodos.Count;

            return Ok(new
            {
                Mensaje = "✅ Datos de ejemplo creados exitosamente",
                Resumen = resumen,
                TotalRegistros = resumen.Values.Sum(),
                StockBajo = new[] { "Probiótico veterinario x 10 (3/10)", "Jeringa descartable 5ml x 100 (8/20)" }
            });
        }

        /// <summary>
        /// Info para reseteo
        /// </summary>
        [HttpDelete("api/v1/Seed/reset")]
        public IActionResult Reset()
        {
            return Ok(new
            {
                Mensaje = "Para resetear, elimine el archivo VeterinariaDB.sqlite y reinicie la aplicación.",
                Ruta = "Template-API/VeterinariaDB.sqlite"
            });
        }
    }
}
