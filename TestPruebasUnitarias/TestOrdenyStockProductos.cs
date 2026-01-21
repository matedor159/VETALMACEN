using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPruebasUnitarias
{
    // Modelo Producto
    public class Producto3
    {
        public int Id { get; set; }
        public int Stock { get; set; }
    }

    // Modelo Orden
    public class Orden
    {
        public int Cantidad { get; set; }
        public string Estado { get; set; }
        public Producto3 Producto { get; set; }
    }

    // Servicio común que valida y descuenta stock
    public class InventarioService
    {
        public (bool exito, string mensaje) ConfirmarOrden(Orden orden)
        {
            if (orden == null || orden.Producto == null)
                return (false, "Orden o producto inválido.");

            if (orden.Estado == "Confirmado")
                return (false, "La orden ya está confirmada.");

            if (orden.Producto.Stock >= orden.Cantidad)
            {
                orden.Producto.Stock -= orden.Cantidad;
                orden.Estado = "Confirmado";
                return (true, $"Orden confirmada. Stock reducido en {orden.Cantidad} unidades.");
            }

            return (false, "No hay suficiente stock para confirmar esta orden.");
        }

        public (bool exito, string mensaje) RegistrarSalida(Producto3 producto, int cantidad)
        {
            if (producto == null)
                return (false, "Producto no encontrado.");

            if (cantidad <= 0)
                return (false, "Cantidad inválida.");

            if (producto.Stock < cantidad)
                return (false, "No hay suficiente stock disponible.");

            producto.Stock -= cantidad;
            return (true, $"Stock reducido en {cantidad} unidades.");
        }
    }

    [TestClass]
    public class InventarioTests
    {
        [TestMethod]
        public void ConfirmarOrden_Deberia_DescontarStock_Si_HayStock()
        {
            var producto = new Producto3 { Id = 1, Stock = 10 };
            var orden = new Orden { Cantidad = 5, Estado = "Pendiente", Producto = producto };
            var servicio = new InventarioService();

            var resultado = servicio.ConfirmarOrden(orden);

            Assert.IsTrue(resultado.exito);
            Assert.AreEqual(5, producto.Stock);
            Assert.AreEqual("Confirmado", orden.Estado);
        }

        [TestMethod]
        public void ConfirmarOrden_Deberia_Fallar_Si_No_HayStock()
        {
            var producto = new Producto3 { Id = 1, Stock = 3 };
            var orden = new Orden { Cantidad = 5, Estado = "Pendiente", Producto = producto };
            var servicio = new InventarioService();

            var resultado = servicio.ConfirmarOrden(orden);

            Assert.IsFalse(resultado.exito);
            Assert.AreEqual(3, producto.Stock);
            Assert.AreEqual("Pendiente", orden.Estado);
        }

        [TestMethod]
        public void RegistrarSalida_Deberia_Descontar_Si_Valido()
        {
            var producto = new Producto3 { Id = 2, Stock = 20 };
            var servicio = new InventarioService();

            var resultado = servicio.RegistrarSalida(producto, 8);

            Assert.IsTrue(resultado.exito);
            Assert.AreEqual(12, producto.Stock);
        }

        [TestMethod]
        public void RegistrarSalida_Deberia_Fallar_Si_Stock_Insuficiente()
        {
            var producto = new Producto3 { Id = 3, Stock = 4 };
            var servicio = new InventarioService();

            var resultado = servicio.RegistrarSalida(producto, 10);

            Assert.IsFalse(resultado.exito);
            Assert.AreEqual(4, producto.Stock);
        }
    }
}
