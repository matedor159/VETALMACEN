using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/demo")]
public class DemoController : ControllerBase
{
    private readonly AuditoriaService _auditoria;
    private readonly ReporteInventarioService _reporte;

    public DemoController()
    {
        _auditoria = new AuditoriaService();
        _reporte = new ReporteInventarioService();
    }

    [HttpGet("auditoria")]
    public IActionResult VerAuditoria()
    {
        _auditoria.RegistrarAccion(
            "admin",
            "CONSULTA",
            "INVENTARIO",
            DateTime.Now
        );

        return Ok("Auditoría registrada (ver consola)");
    }

    [HttpGet("reporte")]
    public IActionResult VerReporte()
    {
        var data = _reporte.GenerarReporteInventario();
        return File(data, "text/plain", "reporte.txt");
    }
}
