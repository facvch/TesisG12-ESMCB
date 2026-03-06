using Core.Domain.Entities;
using Domain.Validators;

namespace Domain.Entities
{
    /// <summary>
    /// Configuración del sistema - almacena parámetros clave-valor
    /// </summary>
    public class ConfiguracionSistema : DomainEntity<string, ConfiguracionSistemaValidator>
    {
        public string Clave { get; private set; }
        public string Valor { get; private set; }
        public string Descripcion { get; private set; }
        public string Grupo { get; private set; }       // "Clinica", "Facturacion", "Turnos", etc.
        public TipoDato TipoDato { get; private set; }   // String, Numero, Booleano, Fecha
        public DateTime FechaModificacion { get; private set; }

        protected ConfiguracionSistema() { }

        public ConfiguracionSistema(string clave, string valor, string descripcion,
            string grupo, TipoDato tipoDato = TipoDato.String) : this()
        {
            Id = Guid.NewGuid().ToString();
            Clave = clave;
            Valor = valor;
            Descripcion = descripcion;
            Grupo = grupo;
            TipoDato = tipoDato;
            FechaModificacion = DateTime.Now;
        }

        public void ActualizarValor(string nuevoValor)
        {
            Valor = nuevoValor;
            FechaModificacion = DateTime.Now;
        }
    }

    public enum TipoDato
    {
        String = 0,
        Numero = 1,
        Booleano = 2,
        Fecha = 3
    }
}
