namespace Application.DataTransferObjects
{
    public class VacunaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Laboratorio { get; set; }
        public int? IntervaloDosisDias { get; set; }
        public bool Activo { get; set; }
    }
}
