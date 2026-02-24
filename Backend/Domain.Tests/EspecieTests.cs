using Domain.Entities;

namespace Domain.Tests
{
    public class EspecieTests
    {
        [Fact]
        public void Especie_Create_ShouldBeValid()
        {
            // Arrange & Act
            var especie = new Especie("Canino", "Perros domésticos");

            // Assert
            Assert.Equal("Canino", especie.Nombre);
            Assert.Equal("Perros domésticos", especie.Descripcion);
            Assert.True(especie.Activo);
            Assert.True(especie.IsValid);
        }

        [Fact]
        public void Especie_CreateWithEmptyName_ShouldBeInvalid()
        {
            // Arrange & Act
            var especie = new Especie("", "Descripción");

            // Assert
            Assert.False(especie.IsValid);
            Assert.Contains(especie.GetErrors(), e => e.PropertyName == "Nombre");
        }

        [Fact]
        public void Especie_Desactivar_ShouldSetActivoFalse()
        {
            // Arrange
            var especie = new Especie("Felino");

            // Act
            especie.Desactivar();

            // Assert
            Assert.False(especie.Activo);
        }

        [Fact]
        public void Especie_Activar_ShouldSetActivoTrue()
        {
            // Arrange
            var especie = new Especie("Ave");
            especie.Desactivar();

            // Act
            especie.Activar();

            // Assert
            Assert.True(especie.Activo);
        }

        [Fact]
        public void Especie_Actualizar_ShouldUpdateProperties()
        {
            // Arrange
            var especie = new Especie("Canino", "Descripción inicial");

            // Act
            especie.Actualizar("Caninos", "Perros y lobos");

            // Assert
            Assert.Equal("Caninos", especie.Nombre);
            Assert.Equal("Perros y lobos", especie.Descripcion);
        }
    }
}
