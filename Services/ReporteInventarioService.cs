namespace SisAlmacenProductos.Services
{
    public class ReporteInventarioService
{
    public byte[] GenerarReporteInventario()
    {
        // Simulación de reporte binario (PDF/Excel)
        var contenido = "REPORTE DE INVENTARIO\n\nGenerado: " + DateTime.Now;
        return System.Text.Encoding.UTF8.GetBytes(contenido);
    }
}
}