namespace SistemaVeterinario.Dominio;

public abstract class Animal
{
    public string Codigo { get; }
    public string Nombre { get; }
    public string Especie { get; }
    public int Edad { get; }
    public int PropietarioId { get; }

    public abstract string Tipo { get; }

    protected Animal(string codigo, string nombre, string especie, int edad, int propietarioId)
    {
        if (edad < 0) throw new ArgumentException("La edad no puede ser negativa.");
        if (propietarioId <= 0) throw new ArgumentException("Seleccione un propietario valido.");
        Codigo = Validacion.Requerido(codigo, "Codigo");
        Nombre = Validacion.Requerido(nombre, "Nombre");
        Especie = Validacion.Requerido(especie, "Especie");
        Edad = edad;
        PropietarioId = propietarioId;
    }

    public abstract string AplicarTratamiento();

    public string ObtenerInformacion() => $"[{Codigo}] {Nombre} ({Tipo} - {Especie}), {Edad} anio(s)";
}

internal static class Validacion
{
    public static string Requerido(string? valor, string campo) =>
        string.IsNullOrWhiteSpace(valor)
            ? throw new ArgumentException($"El campo '{campo}' es obligatorio.")
            : valor.Trim();
}