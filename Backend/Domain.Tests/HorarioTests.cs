using Domain.Entities;

namespace Domain.Tests
{
    public class HorarioTests
    {
        [Fact]
        public void TipoHorario_Create_ValidData_ShouldBeValid()
        {
            // Arrange & Act
            var tipoNormal = new TipoHorario(1, "Normal", "Horario regular de atención");
            var tipoGuardia = new TipoHorario(2, "Guardia", "Horario de guardia");

            // Assert
            Assert.Equal("Normal", tipoNormal.Nombre);
            Assert.True(tipoNormal.IsValid);

            Assert.Equal("Guardia", tipoGuardia.Nombre);
            Assert.True(tipoGuardia.IsValid);
        }

        [Fact]
        public void Horario_Create_ValidData_ShouldBeValid()
        {
            // Arrange & Act
            var horario = new Horario(
                veterinarioId: "vet-123",
                diaSemana: 1, // Lunes
                horaInicio: new TimeSpan(8, 0, 0),
                horaFin: new TimeSpan(16, 0, 0),
                tipoHorarioId: 1 // Normal
            );

            // Assert
            Assert.True(horario.IsValid);
            Assert.Equal("vet-123", horario.VeterinarioId);
            Assert.Equal(1, horario.DiaSemana);
            Assert.Equal(new TimeSpan(8, 0, 0), horario.HoraInicio);
            Assert.Equal(new TimeSpan(16, 0, 0), horario.HoraFin);
            Assert.Equal(1, horario.TipoHorarioId);
        }

        [Fact]
        public void Horario_HoraFinBeforeHoraInicio_ShouldBeInvalid()
        {
            // Arrange & Act
            var horario = new Horario(
                veterinarioId: "vet-123",
                diaSemana: 1,
                horaInicio: new TimeSpan(16, 0, 0),
                horaFin: new TimeSpan(8, 0, 0), // inválido: fin antes de inicio
                tipoHorarioId: 1
            );

            // Assert
            Assert.False(horario.IsValid);
            Assert.Contains(horario.GetErrors(), e => e.PropertyName == "HoraFin");
        }

        [Fact]
        public void Horario_InvalidDiaSemana_ShouldBeInvalid()
        {
            // Arrange & Act
            var horario = new Horario(
                veterinarioId: "vet-123",
                diaSemana: 8, // Día inválido (solo 1..7)
                horaInicio: new TimeSpan(8, 0, 0),
                horaFin: new TimeSpan(16, 0, 0),
                tipoHorarioId: 1
            );

            // Assert
            Assert.False(horario.IsValid);
            Assert.Contains(horario.GetErrors(), e => e.PropertyName == "DiaSemana");
        }

        [Fact]
        public void CalcularDisponibilidad_SinHorarios_DebeRetornarNoDisponible()
        {
            // Arrange
            var horarios = new List<Horario>();

            // Act
            var disponibilidad = Horario.CalcularDisponibilidad(horarios, DateTime.Now);

            // Assert
            Assert.Equal("No Disponible", disponibilidad);
        }

        [Fact]
        public void CalcularDisponibilidad_EnRangoNormal_DebeRetornarDisponible()
        {
            // Arrange
            var testTime = new DateTime(2026, 7, 20, 10, 0, 0); // Un Lunes a las 10:00
            int currentDayIso = 1; // Lunes

            var horarios = new List<Horario>
            {
                new Horario("vet-1", currentDayIso, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0), 1)
            };

            // Act
            var disponibilidad = Horario.CalcularDisponibilidad(horarios, testTime);

            // Assert
            Assert.Equal("Disponible", disponibilidad);
        }

        [Fact]
        public void CalcularDisponibilidad_EnRangoGuardia_DebeRetornarGuardia()
        {
            // Arrange
            var testTime = new DateTime(2026, 7, 20, 22, 0, 0); // Un Lunes a las 22:00
            int currentDayIso = 1; // Lunes

            var horarios = new List<Horario>
            {
                new Horario("vet-1", currentDayIso, new TimeSpan(20, 0, 0), new TimeSpan(23, 59, 0), 2)
            };

            // Act
            var disponibilidad = Horario.CalcularDisponibilidad(horarios, testTime);

            // Assert
            Assert.Equal("Guardia", disponibilidad);
        }
    }
}
