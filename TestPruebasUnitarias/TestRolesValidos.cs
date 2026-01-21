using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TestPruebasUnitarias
{
    // Clase del modelo
    public class User1
    {
        public string Username { get; set; }
        public string Role { get; set; }
    }

    // Clase de prueba unitaria
    [TestClass]
    public class UsuariosTests1
    {
        [TestMethod]
        public void Solo_Se_Deberia_Asignar_Roles_Validos()
        {
            // Arrange
            var rolesValidos = new List<string> { "Administrador", "Logistica", "Almacenero", "Cliente" };
            var nuevoUsuario = new User1 { Username = "pruebaUsuario", Role = "Administrador" };

            // Act
            bool rolValido = rolesValidos.Contains(nuevoUsuario.Role);

            // Assert
            Assert.IsTrue(rolValido, "El rol asignado no está en la lista de roles válidos.");
        }
    }
}