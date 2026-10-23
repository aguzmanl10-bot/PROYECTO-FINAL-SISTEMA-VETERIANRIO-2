CREATE TABLE IF NOT EXISTS Propietarios (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo   TEXT NOT NULL UNIQUE,
    Nombre   TEXT NOT NULL,
    Telefono TEXT NOT NULL,
    Correo   TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Animales (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo        TEXT NOT NULL UNIQUE,
    Nombre        TEXT NOT NULL,
    Especie       TEXT NOT NULL,
    Edad          INTEGER NOT NULL CHECK (Edad >= 0),
    Tipo          TEXT NOT NULL CHECK (Tipo IN ('Mamifero', 'Ave', 'Reptil', 'Anfibio')),
    PropietarioId INTEGER NOT NULL REFERENCES Propietarios(Id) ON DELETE RESTRICT
);
CREATE TABLE IF NOT EXISTS Mamiferos (
    AnimalId   INTEGER PRIMARY KEY REFERENCES Animales(Id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS Aves (
    AnimalId    INTEGER PRIMARY KEY REFERENCES Animales(Id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS Reptiles (
    AnimalId     INTEGER PRIMARY KEY REFERENCES Animales(Id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS Anfibios (
    AnimalId    INTEGER PRIMARY KEY REFERENCES Animales(Id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS Citas (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    AnimalId  INTEGER NOT NULL REFERENCES Animales(Id) ON DELETE CASCADE,
    FechaCita TEXT NOT NULL,
    Motivo    TEXT NOT NULL,
    Estado    TEXT NOT NULL DEFAULT 'PROGRAMADA' CHECK (Estado IN ('PROGRAMADA', 'ATENDIDA', 'CANCELADA'))
);
CREATE TABLE IF NOT EXISTS RegistrosMedicos (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    AnimalId      INTEGER NOT NULL REFERENCES Animales(Id) ON DELETE CASCADE,
    CitaId        INTEGER NULL REFERENCES Citas(Id) ON DELETE SET NULL,
    FechaRegistro TEXT NOT NULL,
    Diagnostico   TEXT NOT NULL,
    Observaciones TEXT NULL
);
CREATE TABLE IF NOT EXISTS Tratamientos (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    RegistroMedicoId INTEGER NOT NULL REFERENCES RegistrosMedicos(Id) ON DELETE CASCADE,
    Descripcion      TEXT NOT NULL,
    Medicamento      TEXT NULL,
    FechaInicio      TEXT NOT NULL,
    FechaFin         TEXT NULL,
    Costo            REAL NOT NULL CHECK (Costo >= 0),
    CHECK (FechaFin IS NULL OR FechaFin >= FechaInicio)
);