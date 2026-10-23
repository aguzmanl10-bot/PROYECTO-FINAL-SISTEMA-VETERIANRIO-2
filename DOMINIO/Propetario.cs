namespace SistemaVeterinario.Dominio;

public class Propietario
{
    public string Codigo { get; }
    public string Nombre { get; }
    public string Telefono { get; }
    public string Correo { get; }

    public Propietario(string codigo, string nombre, string telefono, string correo)
    {
        Codigo = Validacion.Requerido(codigo, "Codigo");
        Nombre = Validacion.Requerido(nombre, "Nombre");
        Telefono = Validacion.Requerido(telefono, "Telefono");
        Correo = Validacion.Requerido(correo, "Correo");
    }

    public string ObtenerInformacion() => $"[{Codigo}] {Nombre} - Tel: {Telefono} - Correo: {Correo}";
}