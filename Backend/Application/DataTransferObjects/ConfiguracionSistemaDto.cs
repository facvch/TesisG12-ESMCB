namespace Application.DataTransferObjects
{
    public class ConfiguracionSistemaDto
    {
        public string Id { get; set; }
        public string Clave { get; set; }
        public string Valor { get; set; }
        public string Descripcion { get; set; }
        public string Grupo { get; set; }
        public string TipoDato { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
