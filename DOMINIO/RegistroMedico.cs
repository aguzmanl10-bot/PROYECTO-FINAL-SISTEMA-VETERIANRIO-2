namespace SistemaVeterinario.Dominio;

public class RegistroMedico
{
    public int AnimalId { get; }
    public int? CitaId { get; }
    public DateTime FechaRegistro { get; }
    public string Diagnostico { get; }
    public string? Observaciones { get; }

    public RegistroMedico(int animalId, int? citaId, DateTime fechaRegistro, string diagnostico, string? observaciones)
    {
        if (animalId <= 0) throw new ArgumentException("Seleccione un animal valido.");
        AnimalId = animalId;
        CitaId = citaId;
        FechaRegistro = fechaRegistro.Date;
        Diagnostico = Validacion.Requerido(diagnostico, "Diagnostico");
        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
    }
}