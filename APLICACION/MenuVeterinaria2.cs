using System.Data;
using System.Globalization;
using SistemaVeterinario.Dominio;
using SistemaVeterinario.Persistencia;

namespace SistemaVeterinario.Aplicacion;

internal sealed class MenuVeterinaria
{
    private readonly RepositorioVeterinaria _repositorio;
    private bool _continuar = true;

    public MenuVeterinaria(RepositorioVeterinaria repositorio) => _repositorio = repositorio;

    public void Ejecutar()
    {
        while (_continuar)
        {
            MostrarMenu();
            try
            {
                var entrada = Console.ReadLine();
                if (entrada is null) break;
                switch (entrada.Trim())
                {
                    case "1": CrearPropietario(); break;
                    case "2": ListarPropietarios(); break;
                    case "3": ActualizarPropietario(); break;
                    case "4": EliminarPropietario(); break;
                    case "5": CrearAnimal(); break;
                    case "6": ListarAnimales(); break;
                    case "7": ActualizarAnimal(); break;
                    case "8": EliminarAnimal(); break;
                    case "9": CrearCita(); break;
                    case "10": ListarCitas(); break;
                    case "11": ActualizarCita(); break;
                    case "12": CambiarEstadoCita(); break;
                    case "13": EliminarCita(); break;
                    case "14": CrearRegistroMedico(); break;
                    case "15": ListarRegistrosMedicos(); break;
                    case "16": ActualizarRegistroMedico(); break;
                    case "17": EliminarRegistroMedico(); break;
                    case "18": CrearTratamiento(); break;
                    case "19": ListarTratamientos(); break;
                    case "20": ActualizarTratamiento(); break;
                    case "21": EliminarTratamiento(); break;
                    case "22": DemostrarPolimorfismo(); break;
                    case "0": _continuar = false; break;
                    default: Console.WriteLine("Seleccione una opcion valida."); break;
                }
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error controlado con try-catch: {error.Message}");
                Console.ResetColor();
            }

            if (_continuar) Pausar();
        }
    }

    private static void MostrarMenu()
    {
        if (!Console.IsOutputRedirected) Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("===============================================");
        Console.WriteLine("            SISTEMA VETERINARIO");
        Console.WriteLine("===============================================");
        Console.ResetColor();
        Console.WriteLine("PROPIETARIOS:  1 Crear | 2 Listar | 3 Actualizar | 4 Eliminar");
        Console.WriteLine("ANIMALES:      5 Crear | 6 Listar | 7 Actualizar | 8 Eliminar");
        Console.WriteLine("CITAS:         9 Crear | 10 Listar | 11 Actualizar | 12 Atender/Cancelar | 13 Eliminar");
        Console.WriteLine("REG. MEDICOS:  14 Crear | 15 Listar | 16 Actualizar | 17 Eliminar");
        Console.WriteLine("TRATAMIENTOS:  18 Crear | 19 Listar | 20 Actualizar | 21 Eliminar");
        Console.WriteLine("POO: 22 Demostrar polimorfismo | 0 Salir");
        Console.Write("Opcion: ");
    }

    // ---------- Propietarios ----------
    private void CrearPropietario()
    {
        _repositorio.CrearPropietario(LeerPropietario());
        Console.WriteLine("Propietario creado correctamente.");
    }

    private void ListarPropietarios() => MostrarTabla(_repositorio.ObtenerPropietarios(), "PROPIETARIOS");

    private void ActualizarPropietario()
    {
        MostrarTabla(_repositorio.ObtenerPropietarios(), "PROPIETARIOS");
        _repositorio.ActualizarPropietario(LeerId("Id del propietario a actualizar"), LeerPropietario());
        Console.WriteLine("Propietario actualizado correctamente.");
    }

    private void EliminarPropietario()
    {
        MostrarTabla(_repositorio.ObtenerPropietarios(), "PROPIETARIOS");
        _repositorio.EliminarPropietario(LeerId("Id del propietario a eliminar"));
        Console.WriteLine("Propietario eliminado.");
    }

    // ---------- Animales ----------
    private void CrearAnimal()
    {
        MostrarTabla(_repositorio.ObtenerPropietarios(), "PROPIETARIOS");
        _repositorio.CrearAnimal(LeerAnimal());
        Console.WriteLine("Animal creado correctamente.");
    }

    private void ListarAnimales() => MostrarTabla(_repositorio.ObtenerAnimales(), "ANIMALES");

    private void ActualizarAnimal()
    {
        MostrarTabla(_repositorio.ObtenerAnimales(), "ANIMALES");
        var id = LeerId("Id del animal a actualizar");
        MostrarTabla(_repositorio.ObtenerPropietarios(), "PROPIETARIOS");
        _repositorio.ActualizarAnimal(id, LeerAnimal());
        Console.WriteLine("Animal actualizado correctamente (el tipo debe ser el mismo).");
    }

    private void EliminarAnimal()
    {
        MostrarTabla(_repositorio.ObtenerAnimales(), "ANIMALES");
        _repositorio.EliminarAnimal(LeerId("Id del animal a eliminar"));
        Console.WriteLine("Animal eliminado; sus citas y registros medicos tambien se eliminan.");
    }

    // ---------- Citas ----------
    private void CrearCita()
    {
        MostrarTabla(_repositorio.ObtenerAnimales(), "ANIMALES");
        var animalId = LeerId("Id del animal");
        var fecha = LeerFecha("Fecha de la cita (AAAA-MM-DD)");
        var motivo = LeerTexto("Motivo");
        _repositorio.CrearCita(new Cita(animalId, fecha, motivo));
        Console.WriteLine("Cita registrada como PROGRAMADA.");
    }

    private void ListarCitas() => MostrarTabla(_repositorio.ObtenerCitas(), "CITAS");

    private void ActualizarCita()
    {
        MostrarTabla(_repositorio.ObtenerCitas(), "CITAS");
        var id = LeerId("Id de la cita a actualizar");
        MostrarTabla(_repositorio.ObtenerAnimales(), "ANIMALES");
        var animalId = LeerId("Id del animal");
        var fecha = LeerFecha("Fecha de la cita (AAAA-MM-DD)");
        var motivo = LeerTexto("Motivo");
        var estado = LeerEstadoCita();
        _repositorio.ActualizarCita(id, new Cita(animalId, fecha, motivo, estado));
        Console.WriteLine("Cita actualizada correctamente.");
    }

    private void CambiarEstadoCita()
    {
        MostrarTabla(_repositorio.ObtenerCitas(), "CITAS");
        var id = LeerId("Id de la cita");
        var opcion = LeerEntero("Nuevo estado (1 Atendida, 2 Cancelada)");
        var estado = opcion switch
        {
            1 => "ATENDIDA",
            2 => "CANCELADA",
            _ => throw new ArgumentException("Opcion de estado no valida.")
        };
        _repositorio.CambiarEstadoCita(id, estado);
        Console.WriteLine($"Cita marcada como {estado}.");
    }

    private void EliminarCita()
    {
        MostrarTabla(_repositorio.ObtenerCitas(), "CITAS");
        _repositorio.EliminarCita(LeerId("Id de la cita a eliminar"));
        Console.WriteLine("Cita eliminada.");
    }

    // ---------- Registros medicos ----------
    private void CrearRegistroMedico()
    {
        MostrarTabla(_repositorio.ObtenerAnimales(), "ANIMALES");
        MostrarTabla(_repositorio.ObtenerCitas(), "CITAS");
        _repositorio.CrearRegistroMedico(LeerRegistroMedico());
        Console.WriteLine("Registro medico creado correctamente.");
    }

    private void ListarRegistrosMedicos() => MostrarTabla(_repositorio.ObtenerRegistrosMedicos(), "REGISTROS MEDICOS");

    private void ActualizarRegistroMedico()
    {
        MostrarTabla(_repositorio.ObtenerRegistrosMedicos(), "REGISTROS MEDICOS");
        var id = LeerId("Id del registro a actualizar");
        MostrarTabla(_repositorio.ObtenerAnimales(), "ANIMALES");
        MostrarTabla(_repositorio.ObtenerCitas(), "CITAS");
        _repositorio.ActualizarRegistroMedico(id, LeerRegistroMedico());
        Console.WriteLine("Registro medico actualizado correctamente.");
    }

    private void EliminarRegistroMedico()
    {
        MostrarTabla(_repositorio.ObtenerRegistrosMedicos(), "REGISTROS MEDICOS");
        _repositorio.EliminarRegistroMedico(LeerId("Id del registro a eliminar"));
        Console.WriteLine("Registro medico eliminado; sus tratamientos tambien se eliminan.");
    }

    // ---------- Tratamientos ----------
    private void CrearTratamiento()
    {
        MostrarTabla(_repositorio.ObtenerRegistrosMedicos(), "REGISTROS MEDICOS");
        _repositorio.CrearTratamiento(LeerTratamiento());
        Console.WriteLine("Tratamiento creado correctamente.");
    }

    private void ListarTratamientos() => MostrarTabla(_repositorio.ObtenerTratamientos(), "TRATAMIENTOS");

    private void ActualizarTratamiento()
    {
        MostrarTabla(_repositorio.ObtenerTratamientos(), "TRATAMIENTOS");
        var id = LeerId("Id del tratamiento a actualizar");
        MostrarTabla(_repositorio.ObtenerRegistrosMedicos(), "REGISTROS MEDICOS");
        _repositorio.ActualizarTratamiento(id, LeerTratamiento());
        Console.WriteLine("Tratamiento actualizado correctamente.");
    }

    private void EliminarTratamiento()
    {
        MostrarTabla(_repositorio.ObtenerTratamientos(), "TRATAMIENTOS");
        _repositorio.EliminarTratamiento(LeerId("Id del tratamiento a eliminar"));
        Console.WriteLine("Tratamiento eliminado.");
    }

    // ---------- POO ----------
    private static void DemostrarPolimorfismo()
    {
        var animales = new List<Animal>
        {
            new Mamifero("DEMO-1", "Firulais", "Perro", 3, 1),
            new Ave("DEMO-2", "Kiwi", "Loro", 2, 1),
            new Reptil("DEMO-3", "Rex", "Iguana", 4, 1),
            new Anfibio("DEMO-4", "Ranita", "Rana", 1, 1)
        };

        Console.WriteLine("Lista de Animal con instancias de distintos subtipos:");
        foreach (var animal in animales)
        {
            Console.WriteLine(animal.ObtenerInformacion());
            Console.WriteLine($"  -> {animal.AplicarTratamiento()}");
        }
    }

    // ---------- Lectura de datos ----------
    private static Propietario LeerPropietario() => new(
        LeerTexto("Codigo"), LeerTexto("Nombre"), LeerTexto("Telefono"), LeerTexto("Correo"));

    private static Animal LeerAnimal()
    {
        var tipo = LeerEntero("Tipo (1 Mamifero, 2 Ave, 3 Reptil, 4 Anfibio)");
        var codigo = LeerTexto("Codigo");
        var nombre = LeerTexto("Nombre");
        var especie = LeerTexto("Especie");
        var edad = LeerEntero("Edad");
        var propietarioId = LeerId("Id del propietario");
        return tipo switch
        {
            1 => new Mamifero(codigo, nombre, especie, edad, propietarioId),
            2 => new Ave(codigo, nombre, especie, edad, propietarioId),
            3 => new Reptil(codigo, nombre, especie, edad, propietarioId),
            4 => new Anfibio(codigo, nombre, especie, edad, propietarioId),
            _ => throw new ArgumentException("Tipo de animal no valido.")
        };
    }

    private static string LeerEstadoCita()
    {
        var opcion = LeerEntero("Estado (1 Programada, 2 Atendida, 3 Cancelada)");
        return opcion switch
        {
            1 => "PROGRAMADA",
            2 => "ATENDIDA",
            3 => "CANCELADA",
            _ => throw new ArgumentException("Opcion de estado no valida.")
        };
    }

    private static RegistroMedico LeerRegistroMedico()
    {
        var animalId = LeerId("Id del animal");
        var citaId = LeerEnteroOpcional("Id de la cita (Enter si no aplica)");
        var fecha = LeerFecha("Fecha del registro (AAAA-MM-DD)");
        var diagnostico = LeerTexto("Diagnostico");
        var observaciones = LeerTextoOpcional("Observaciones (Enter para omitir)");
        return new RegistroMedico(animalId, citaId, fecha, diagnostico, observaciones);
    }

    private static Tratamiento LeerTratamiento()
    {
        var registroId = LeerId("Id del registro medico");
        var descripcion = LeerTexto("Descripcion");
        var medicamento = LeerTextoOpcional("Medicamento (Enter para omitir)");
        var inicio = LeerFecha("Fecha de inicio (AAAA-MM-DD)");
        var fin = LeerFechaOpcional("Fecha fin (AAAA-MM-DD, Enter si sigue en curso)");
        var costo = LeerDecimal("Costo");
        return new Tratamiento(registroId, descripcion, medicamento, inicio, fin, costo);
    }

    private static int LeerId(string mensaje) => LeerEntero(mensaje);

    private static int LeerEntero(string mensaje)
    {
        Console.Write($"{mensaje}: ");
        if (!int.TryParse(Console.ReadLine(), out var valor)) throw new ArgumentException("Ingrese un numero entero valido.");
        return valor;
    }

    private static int? LeerEnteroOpcional(string mensaje)
    {
        Console.Write($"{mensaje}: ");
        var texto = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(texto)) return null;
        if (!int.TryParse(texto, out var valor)) throw new ArgumentException("Ingrese un numero entero valido.");
        return valor;
    }

    private static double LeerDecimal(string mensaje)
    {
        Console.Write($"{mensaje}: ");
        var texto = (Console.ReadLine() ?? string.Empty).Replace(',', '.');
        if (!double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out var valor))
            throw new ArgumentException("Ingrese un numero valido.");
        return valor;
    }

    private static DateTime LeerFecha(string mensaje)
    {
        Console.Write($"{mensaje}: ");
        if (!DateTime.TryParse(Console.ReadLine(), out var fecha)) throw new ArgumentException("Ingrese una fecha valida.");
        return fecha.Date;
    }

    private static DateTime? LeerFechaOpcional(string mensaje)
    {
        Console.Write($"{mensaje}: ");
        var texto = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(texto)) return null;
        if (!DateTime.TryParse(texto, out var fecha)) throw new ArgumentException("Ingrese una fecha valida.");
        return fecha.Date;
    }

    private static string LeerTexto(string mensaje)
    {
        Console.Write($"{mensaje}: ");
        var texto = Console.ReadLine();
        return string.IsNullOrWhiteSpace(texto)
            ? throw new ArgumentException($"El campo '{mensaje}' es obligatorio.")
            : texto.Trim();
    }

    private static string? LeerTextoOpcional(string mensaje)
    {
        Console.Write($"{mensaje}: ");
        var texto = Console.ReadLine();
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static void MostrarTabla(DataTable tabla, string titulo)
    {
        Console.WriteLine($"\n=== {titulo} ===");
        if (tabla.Rows.Count == 0)
        {
            Console.WriteLine("No hay registros.");
            return;
        }

        foreach (DataColumn columna in tabla.Columns) Console.Write($"{columna.ColumnName,-22}");
        Console.WriteLine();
        foreach (DataRow fila in tabla.Rows)
        {
            foreach (var valor in fila.ItemArray) Console.Write($"{valor,-22}");
            Console.WriteLine();
        }
    }

    private static void Pausar()
    {
        if (Console.IsInputRedirected) return;
        Console.WriteLine("\nPresione Enter para continuar...");
        Console.ReadLine();
    }
}