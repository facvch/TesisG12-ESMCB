using Domain.Entities;

namespace Domain.Tests
{
    public class RazaTests
    {
        [Fact]
        public void Raza_Create_ShouldBeValid()
        {
            // Arrange & Act
            var raza = new Raza("Golden Retriever", 1, "Raza de perro amigable");

            // Assert
            Assert.Equal("Golden Retriever", raza.Nombre);
            Assert.Equal(1, raza.EspecieId);
            Assert.Equal("Raza de perro amigable", raza.Descripcion);
            Assert.True(raza.Activo);
            Assert.True(raza.IsValid);
        }

        [Fact]
        public void Raza_CreateWithEmptyName_ShouldBeInvalid()
        {
            // Arrange & Act
            var raza = new Raza("", 1);

            // Assert
            Assert.False(raza.IsValid);
            Assert.Contains(raza.GetErrors(), e => e.PropertyName == "Nombre");
        }

        [Fact]
        public void Raza_CreateWithInvalidEspecieId_ShouldBeInvalid()
        {
            // Arrange & Act
            var raza = new Raza("Siamés", 0);

            // Assert
            Assert.False(raza.IsValid);
            Assert.Contains(raza.GetErrors(), e => e.PropertyName == "EspecieId");
        }

        [Fact]
        public void Raza_CambiarEspecie_ShouldUpdateEspecieId()
        {
            // Arrange
            var raza = new Raza("Mestizo", 1);

            // Act
            raza.CambiarEspecie(2);

            // Assert
            Assert.Equal(2, raza.EspecieId);
        }

        [Fact]
        public void Raza_Desactivar_ShouldSetActivoFalse()
        {
            // Arrange
            var raza = new Raza("Labrador", 1);

            // Act
            raza.Desactivar();

            // Assert
            Assert.False(raza.Activo);
        }
    }
}
