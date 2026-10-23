namespace SistemaVeterinario.Dominio;

public class Cita
{
    public static readonly string[] EstadosValidos = { "PROGRAMADA", "ATENDIDA", "CANCELADA" };

    public int AnimalId { get; }
    public DateTime FechaCita { get; }
    public string Motivo { get; }
    public string Estado { get; }

    public Cita(int animalId, DateTime fechaCita, string motivo, string estado = "PROGRAMADA")
    {
        if (animalId <= 0) throw new ArgumentException("Seleccione un animal valido.");
        if (!EstadosValidos.Contains(estado)) throw new ArgumentException("Estado de cita no valido.");
        AnimalId = animalId;
        FechaCita = fechaCita.Date;
        Motivo = Validacion.Requerido(motivo, "Motivo");
        Estado = estado;
    }
}