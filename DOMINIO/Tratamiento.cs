namespace SistemaVeterinario.Dominio;

public class Tratamiento
{
    public int RegistroMedicoId { get; }
    public string Descripcion { get; }
    public string? Medicamento { get; }
    public DateTime FechaInicio { get; }
    public DateTime? FechaFin { get; }
    public double Costo { get; }

    public Tratamiento(int registroMedicoId, string descripcion, string? medicamento,
                       DateTime fechaInicio, DateTime? fechaFin, double costo)
    {
        if (registroMedicoId <= 0) throw new ArgumentException("Seleccione un registro medico valido.");
        if (costo < 0) throw new ArgumentException("El costo no puede ser negativo.");
        if (fechaFin.HasValue && fechaFin.Value.Date < fechaInicio.Date)
            throw new ArgumentException("La fecha fin no puede ser anterior a la fecha de inicio.");
        RegistroMedicoId = registroMedicoId;
        Descripcion = Validacion.Requerido(descripcion, "Descripcion");
        Medicamento = string.IsNullOrWhiteSpace(medicamento) ? null : medicamento.Trim();
        FechaInicio = fechaInicio.Date;
        FechaFin = fechaFin?.Date;
        Costo = costo;
    }
}