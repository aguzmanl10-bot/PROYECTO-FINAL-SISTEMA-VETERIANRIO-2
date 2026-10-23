using SistemaVeterinario.Aplicacion;
using SistemaVeterinario.Persistencia;

namespace SistemaVeterinario;

internal static class Program
{
    private static void Main()
    {
        try
        {
            var inicializador = new InicializadorBaseDatos();
            inicializador.Inicializar();
            var repositorio = new RepositorioVeterinaria(inicializador.CadenaConexion);
            new MenuVeterinaria(repositorio).Ejecutar();
        }
        catch (Exception error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"No fue posible iniciar el sistema: {error.Message}");
            Console.ResetColor();
        }
    }
}