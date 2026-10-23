using System.Data;
using Microsoft.Data.Sqlite;
using SistemaVeterinario.Dominio;

namespace SistemaVeterinario.Persistencia;

internal sealed class RepositorioVeterinaria
{
    private readonly string _cadenaConexion;

    public RepositorioVeterinaria(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    // ===================== CRUD de propietarios =====================
    public DataTable ObtenerPropietarios() => Consultar(
        "SELECT Id, Codigo, Nombre, Telefono, Correo FROM Propietarios ORDER BY Nombre;");

    public void CrearPropietario(Propietario propietario) => Ejecutar(
        "INSERT INTO Propietarios (Codigo, Nombre, Telefono, Correo) VALUES ($codigo, $nombre, $telefono, $correo);",
        ("$codigo", propietario.Codigo), ("$nombre", propietario.Nombre),
        ("$telefono", propietario.Telefono), ("$correo", propietario.Correo));

    public void ActualizarPropietario(int id, Propietario propietario)
    {
        using var conexion = AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "UPDATE Propietarios SET Codigo = $codigo, Nombre = $nombre, Telefono = $telefono, Correo = $correo WHERE Id = $id;";
        comando.Parameters.AddWithValue("$codigo", propietario.Codigo);
        comando.Parameters.AddWithValue("$nombre", propietario.Nombre);
        comando.Parameters.AddWithValue("$telefono", propietario.Telefono);
        comando.Parameters.AddWithValue("$correo", propietario.Correo);
        comando.Parameters.AddWithValue("$id", id);
        if (comando.ExecuteNonQuery() == 0) throw new InvalidOperationException("No existe un propietario con ese Id.");
    }

    public void EliminarPropietario(int id)
    {
        using var conexion = AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT COUNT(*) FROM Animales WHERE PropietarioId = $id;";
        comando.Parameters.AddWithValue("$id", id);
        if (Convert.ToInt32(comando.ExecuteScalar()) > 0)
            throw new InvalidOperationException("No se puede eliminar: el propietario tiene animales registrados.");
        comando.CommandText = "DELETE FROM Propietarios WHERE Id = $id;";
        comando.ExecuteNonQuery();
    }

    // ===================== CRUD de animales (con subtipos) =====================
    public DataTable ObtenerAnimales() => Consultar(@"SELECT a.Id, a.Codigo, a.Nombre, a.Especie, a.Edad, a.Tipo,
        p.Nombre AS Propietario
        FROM Animales a
        INNER JOIN Propietarios p ON p.Id = a.PropietarioId
        ORDER BY a.Nombre;");

    public void CrearAnimal(Animal animal)
    {
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction();
        ValidarExiste(conexion, transaccion, "Propietarios", animal.PropietarioId, "El propietario seleccionado no existe.");

        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = @"INSERT INTO Animales (Codigo, Nombre, Especie, Edad, Tipo, PropietarioId)
            VALUES ($codigo, $nombre, $especie, $edad, $tipo, $propietario);";
        AgregarParametrosAnimal(comando, animal);
        comando.ExecuteNonQuery();

        comando.CommandText = "SELECT last_insert_rowid();";
        comando.Parameters.Clear();
        var id = Convert.ToInt32(comando.ExecuteScalar());

        comando.CommandText = ObtenerSqlSubtipo(animal);
        comando.Parameters.Clear();
        comando.Parameters.AddWithValue("$id", id);
        comando.ExecuteNonQuery();

        transaccion.Commit();
    }

    public void ActualizarAnimal(int id, Animal animal)
    {
        using var conexion = AbrirConexion();
        using var transaccion = conexion.BeginTransaction();
        ValidarExiste(conexion, transaccion, "Propietarios", animal.PropietarioId, "El propietario seleccionado no existe.");

        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = "SELECT Tipo FROM Animales WHERE Id = $id;";
        comando.Parameters.AddWithValue("$id", id);
        var tipoActual = comando.ExecuteScalar() as string
            ?? throw new InvalidOperationException("No existe un animal con ese Id.");
        if (tipoActual != animal.Tipo)
            throw new InvalidOperationException($"El animal es de tipo {tipoActual}; no se puede cambiar a {animal.Tipo}.");

        comando.Parameters.Clear();
        comando.CommandText = @"UPDATE Animales SET Codigo = $codigo, Nombre = $nombre, Especie = $especie,
            Edad = $edad, Tipo = $tipo, PropietarioId = $propietario WHERE Id = $id;";
        AgregarParametrosAnimal(comando, animal);
        comando.Parameters.AddWithValue("$id", id);
        comando.ExecuteNonQuery();

        transaccion.Commit();
    }

    public void EliminarAnimal(int id) => Ejecutar("DELETE FROM Animales WHERE Id = $id;", ("$id", id));

    private static string ObtenerSqlSubtipo(Animal animal) => animal switch
    {
        Mamifero => "INSERT INTO Mamiferos (AnimalId) VALUES ($id);",
        Ave => "INSERT INTO Aves (AnimalId) VALUES ($id);",
        Reptil => "INSERT INTO Reptiles (AnimalId) VALUES ($id);",
        Anfibio => "INSERT INTO Anfibios (AnimalId) VALUES ($id);",
        _ => throw new InvalidOperationException("Tipo de animal no soportado.")
    };

    private static void AgregarParametrosAnimal(SqliteCommand comando, Animal animal)
    {
        comando.Parameters.AddWithValue("$codigo", animal.Codigo);
        comando.Parameters.AddWithValue("$nombre", animal.Nombre);
        comando.Parameters.AddWithValue("$especie", animal.Especie);
        comando.Parameters.AddWithValue("$edad", animal.Edad);
        comando.Parameters.AddWithValue("$tipo", animal.Tipo);
        comando.Parameters.AddWithValue("$propietario", animal.PropietarioId);
    }

    // ===================== CRUD de citas =====================
    public DataTable ObtenerCitas() => Consultar(@"SELECT c.Id, c.AnimalId, a.Nombre AS Animal, p.Nombre AS Propietario,
        c.FechaCita, c.Motivo, c.Estado
        FROM Citas c
        INNER JOIN Animales a ON a.Id = c.AnimalId
        INNER JOIN Propietarios p ON p.Id = a.PropietarioId
        ORDER BY c.FechaCita DESC, c.Id DESC;");

    public void CrearCita(Cita cita)
    {
        using var conexion = AbrirConexion();
        ValidarExiste(conexion, null, "Animales", cita.AnimalId, "El animal seleccionado no existe.");
        using var comando = conexion.CreateCommand();
        comando.CommandText = "INSERT INTO Citas (AnimalId, FechaCita, Motivo, Estado) VALUES ($animal, $fecha, $motivo, $estado);";
        AgregarParametrosCita(comando, cita);
        comando.ExecuteNonQuery();
    }

    public void ActualizarCita(int id, Cita cita)
    {
        using var conexion = AbrirConexion();
        ValidarExiste(conexion, null, "Animales", cita.AnimalId, "El animal seleccionado no existe.");
        using var comando = conexion.CreateCommand();
        comando.CommandText = "UPDATE Citas SET AnimalId = $animal, FechaCita = $fecha, Motivo = $motivo, Estado = $estado WHERE Id = $id;";
        AgregarParametrosCita(comando, cita);
        comando.Parameters.AddWithValue("$id", id);
        if (comando.ExecuteNonQuery() == 0) throw new InvalidOperationException("No existe una cita con ese Id.");
    }

    public void CambiarEstadoCita(int id, string estado)
    {
        if (estado is not ("ATENDIDA" or "CANCELADA"))
            throw new ArgumentException("El nuevo estado debe ser ATENDIDA o CANCELADA.");
        using var conexion = AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "UPDATE Citas SET Estado = $estado WHERE Id = $id AND Estado = 'PROGRAMADA';";
        comando.Parameters.AddWithValue("$estado", estado);
        comando.Parameters.AddWithValue("$id", id);
        if (comando.ExecuteNonQuery() == 0)
            throw new InvalidOperationException("La cita no existe o ya fue atendida/cancelada.");
    }

    public void EliminarCita(int id) => Ejecutar("DELETE FROM Citas WHERE Id = $id;", ("$id", id));

    private static void AgregarParametrosCita(SqliteCommand comando, Cita cita)
    {
        comando.Parameters.AddWithValue("$animal", cita.AnimalId);
        comando.Parameters.AddWithValue("$fecha", cita.FechaCita.ToString("yyyy-MM-dd"));
        comando.Parameters.AddWithValue("$motivo", cita.Motivo);
        comando.Parameters.AddWithValue("$estado", cita.Estado);
    }

    // ===================== CRUD de registros medicos =====================
    public DataTable ObtenerRegistrosMedicos() => Consultar(@"SELECT r.Id, r.AnimalId, a.Nombre AS Animal, r.CitaId,
        r.FechaRegistro, r.Diagnostico, r.Observaciones
        FROM RegistrosMedicos r
        INNER JOIN Animales a ON a.Id = r.AnimalId
        ORDER BY r.FechaRegistro DESC, r.Id DESC;");

    public void CrearRegistroMedico(RegistroMedico registro)
    {
        using var conexion = AbrirConexion();
        ValidarRegistro(conexion, registro);
        using var comando = conexion.CreateCommand();
        comando.CommandText = @"INSERT INTO RegistrosMedicos (AnimalId, CitaId, FechaRegistro, Diagnostico, Observaciones)
            VALUES ($animal, $cita, $fecha, $diagnostico, $observaciones);";
        AgregarParametrosRegistro(comando, registro);
        comando.ExecuteNonQuery();
    }

    public void ActualizarRegistroMedico(int id, RegistroMedico registro)
    {
        using var conexion = AbrirConexion();
        ValidarRegistro(conexion, registro);
        using var comando = conexion.CreateCommand();
        comando.CommandText = @"UPDATE RegistrosMedicos SET AnimalId = $animal, CitaId = $cita, FechaRegistro = $fecha,
            Diagnostico = $diagnostico, Observaciones = $observaciones WHERE Id = $id;";
        AgregarParametrosRegistro(comando, registro);
        comando.Parameters.AddWithValue("$id", id);
        if (comando.ExecuteNonQuery() == 0) throw new InvalidOperationException("No existe un registro medico con ese Id.");
    }

    public void EliminarRegistroMedico(int id) => Ejecutar("DELETE FROM RegistrosMedicos WHERE Id = $id;", ("$id", id));

    private static void ValidarRegistro(SqliteConnection conexion, RegistroMedico registro)
    {
        ValidarExiste(conexion, null, "Animales", registro.AnimalId, "El animal seleccionado no existe.");
        if (!registro.CitaId.HasValue) return;
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT COUNT(*) FROM Citas WHERE Id = $cita AND AnimalId = $animal;";
        comando.Parameters.AddWithValue("$cita", registro.CitaId.Value);
        comando.Parameters.AddWithValue("$animal", registro.AnimalId);
        if (Convert.ToInt32(comando.ExecuteScalar()) == 0)
            throw new InvalidOperationException("La cita no existe o no pertenece a ese animal.");
    }

    private static void AgregarParametrosRegistro(SqliteCommand comando, RegistroMedico registro)
    {
        comando.Parameters.AddWithValue("$animal", registro.AnimalId);
        comando.Parameters.AddWithValue("$cita", registro.CitaId.HasValue ? registro.CitaId.Value : DBNull.Value);
        comando.Parameters.AddWithValue("$fecha", registro.FechaRegistro.ToString("yyyy-MM-dd"));
        comando.Parameters.AddWithValue("$diagnostico", registro.Diagnostico);
        comando.Parameters.AddWithValue("$observaciones", registro.Observaciones is null ? DBNull.Value : registro.Observaciones);
    }

    // ===================== CRUD de tratamientos =====================
    public DataTable ObtenerTratamientos() => Consultar(@"SELECT t.Id, t.RegistroMedicoId, a.Nombre AS Animal,
        t.Descripcion, t.Medicamento, t.FechaInicio, t.FechaFin, t.Costo,
        CASE WHEN t.FechaFin IS NOT NULL AND date(t.FechaFin) < date('now', 'localtime') THEN 'Finalizado'
             ELSE 'En curso' END AS Estado
        FROM Tratamientos t
        INNER JOIN RegistrosMedicos r ON r.Id = t.RegistroMedicoId
        INNER JOIN Animales a ON a.Id = r.AnimalId
        ORDER BY t.FechaInicio DESC, t.Id DESC;");

    public void CrearTratamiento(Tratamiento tratamiento)
    {
        using var conexion = AbrirConexion();
        ValidarExiste(conexion, null, "RegistrosMedicos", tratamiento.RegistroMedicoId, "El registro medico seleccionado no existe.");
        using var comando = conexion.CreateCommand();
        comando.CommandText = @"INSERT INTO Tratamientos (RegistroMedicoId, Descripcion, Medicamento, FechaInicio, FechaFin, Costo)
            VALUES ($registro, $descripcion, $medicamento, $inicio, $fin, $costo);";
        AgregarParametrosTratamiento(comando, tratamiento);
        comando.ExecuteNonQuery();
    }

    public void ActualizarTratamiento(int id, Tratamiento tratamiento)
    {
        using var conexion = AbrirConexion();
        ValidarExiste(conexion, null, "RegistrosMedicos", tratamiento.RegistroMedicoId, "El registro medico seleccionado no existe.");
        using var comando = conexion.CreateCommand();
        comando.CommandText = @"UPDATE Tratamientos SET RegistroMedicoId = $registro, Descripcion = $descripcion,
            Medicamento = $medicamento, FechaInicio = $inicio, FechaFin = $fin, Costo = $costo WHERE Id = $id;";
        AgregarParametrosTratamiento(comando, tratamiento);
        comando.Parameters.AddWithValue("$id", id);
        if (comando.ExecuteNonQuery() == 0) throw new InvalidOperationException("No existe un tratamiento con ese Id.");
    }

    public void EliminarTratamiento(int id) => Ejecutar("DELETE FROM Tratamientos WHERE Id = $id;", ("$id", id));

    private static void AgregarParametrosTratamiento(SqliteCommand comando, Tratamiento tratamiento)
    {
        comando.Parameters.AddWithValue("$registro", tratamiento.RegistroMedicoId);
        comando.Parameters.AddWithValue("$descripcion", tratamiento.Descripcion);
        comando.Parameters.AddWithValue("$medicamento", tratamiento.Medicamento is null ? DBNull.Value : tratamiento.Medicamento);
        comando.Parameters.AddWithValue("$inicio", tratamiento.FechaInicio.ToString("yyyy-MM-dd"));
        comando.Parameters.AddWithValue("$fin", tratamiento.FechaFin.HasValue
            ? tratamiento.FechaFin.Value.ToString("yyyy-MM-dd")
            : DBNull.Value);
        comando.Parameters.AddWithValue("$costo", tratamiento.Costo);
    }

    private static void ValidarExiste(SqliteConnection conexion, SqliteTransaction? transaccion, string tabla, int id, string mensaje)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = $"SELECT COUNT(*) FROM {tabla} WHERE Id = $id;";
        comando.Parameters.AddWithValue("$id", id);
        if (Convert.ToInt32(comando.ExecuteScalar()) == 0) throw new InvalidOperationException(mensaje);
    }

    private DataTable Consultar(string sql)
    {
        using var conexion = AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        using var lector = comando.ExecuteReader();
        var tabla = new DataTable();
        tabla.Load(lector);
        return tabla;
    }

    private void Ejecutar(string sql, params (string Nombre, object? Valor)[] parametros)
    {
        using var conexion = AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        foreach (var (nombre, valor) in parametros) comando.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
        comando.ExecuteNonQuery();
    }

    private SqliteConnection AbrirConexion()
    {
        var conexion = new SqliteConnection(_cadenaConexion);
        conexion.Open();
        return conexion;
    }
}