using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPruebasUnitarias
{
    // Clase del modelo
    public class Producto2
    {
        public string ImagenUrl { get; set; }
    }

    // Clase de prueba unitaria
    [TestClass]
    public class ProductoTests2
    {
        [TestMethod]
        public void ImagenUrl_Deberia_Comenzar_Con_Https()
        {
            // Arrange
            var producto = new Producto2 { ImagenUrl = "https://example.com/imagen.jpg" };

            // Act & Assert
            StringAssert.StartsWith(producto.ImagenUrl, "https://", "La URL de la imagen debe comenzar con 'https://'.");
        }
    }
}
