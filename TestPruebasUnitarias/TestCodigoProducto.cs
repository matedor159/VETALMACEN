using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPruebasUnitarias
{
    // Clase del modelo
    public class Producto
    {
        public string Codigo { get; set; }
    }

    // Clase de prueba unitaria
    [TestClass]
    public class ProductoTests
    {
        [TestMethod]
        public void Codigo_No_Deberia_Ser_Mayor_A_Cinco_Caracteres()
        {
            // Arrange
            var producto = new Producto { Codigo = "ABCDE" };

            // Act
            int longitudCodigo = producto.Codigo.Length;

            // Assert
            Assert.IsTrue(longitudCodigo <= 5, "El código del producto tiene más de cinco caracteres.");
        }
    }
}
