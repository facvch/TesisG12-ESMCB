using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Representa una vacuna disponible en la clínica (Antirrábica, Quíntuple, etc.)
    /// </summary>
    public class Vacuna : DomainEntity<int, VacunaValidator>
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public string Laboratorio { get; private set; }
        public int? IntervaloDosisDias { get; private set; } // Intervalo entre dosis en días
        public bool Activo { get; private set; }

        // Navegación
        public virtual ICollection<RegistroVacunacion> Registros { get; private set; }

        protected Vacuna()
        {
            Registros = new List<RegistroVacunacion>();
        }

        public Vacuna(string nombre, string descripcion = "", string laboratorio = "", int? intervaloDosisDias = null) : this()
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Laboratorio = laboratorio;
            IntervaloDosisDias = intervaloDosisDias;
            Activo = true;
        }

        public void Actualizar(string nombre, string descripcion, string laboratorio, int? intervaloDosisDias)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Laboratorio = laboratorio;
            IntervaloDosisDias = intervaloDosisDias;
        }

        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;
    }
}
