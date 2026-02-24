namespace Application.DataTransferObjects
{
    public class RazaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int EspecieId { get; set; }
        public string EspecieNombre { get; set; }
        public bool Activo { get; set; }
    }
}
