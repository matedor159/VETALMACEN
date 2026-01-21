using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPruebasUnitarias
{
    // Clase del modelo
    public class Producto1
    {
        public decimal Precio { get; set; }
    }

    // Clase de prueba unitaria
    [TestClass]
    public class ProductoTests1
    {
        [TestMethod]
        public void Precio_Deberia_Ser_Mayor_O_Igual_A_Cero()
        {
            // Arrange
            var producto = new Producto1 { Precio = 10.5m };

            // Act & Assert
            Assert.IsTrue(producto.Precio >= 0, "El precio del producto no puede ser negativo.");
        }
    }
}
