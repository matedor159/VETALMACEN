public class AuditoriaService
{
    public void RegistrarAccion(
        string usuario,
        string accion,
        string entidad,
        DateTime fecha)
    {
        // Preparado para guardar en BD o archivo
        Console.WriteLine(
            $"[{fecha}] Usuario:{usuario} Acción:{accion} Entidad:{entidad}"
        );
    }
}
