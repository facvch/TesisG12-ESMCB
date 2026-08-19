namespace Application.DataTransferObjects
{
    public class TipoHorarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class HorarioDto
    {
        public string Id { get; set; } = string.Empty;
        public string VeterinarioId { get; set; } = string.Empty;
        public int DiaSemana { get; set; }
        public string DiaSemanaNombre => DiaSemana switch
        {
            1 => "Lunes",
            2 => "Martes",
            3 => "Miércoles",
            4 => "Jueves",
            5 => "Viernes",
            6 => "Sábado",
            7 => "Domingo",
            _ => "Desconocido"
        };
        public string HoraInicio { get; set; } = string.Empty; // HH:mm
        public string HoraFin { get; set; } = string.Empty;    // HH:mm
        public int TipoHorarioId { get; set; }
        public string TipoHorarioNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class CreateHorarioRequest
    {
        public string VeterinarioId { get; set; } = string.Empty;
        public int DiaSemana { get; set; }
        public string HoraInicio { get; set; } = string.Empty; // e.g. "08:00"
        public string HoraFin { get; set; } = string.Empty;    // e.g. "16:00"
        public int TipoHorarioId { get; set; }
    }

    public class UpdateHorarioRequest
    {
        public string Id { get; set; } = string.Empty;
        public int DiaSemana { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public int TipoHorarioId { get; set; }
    }
}
