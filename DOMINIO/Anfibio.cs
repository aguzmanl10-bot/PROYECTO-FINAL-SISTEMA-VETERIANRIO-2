namespace SistemaVeterinario.Dominio;

public class Anfibio : Animal
{
    public Anfibio(string codigo, string nombre, string especie, int edad, int propietarioId)
        : base(codigo, nombre, especie, edad, propietarioId)
    {
    }

    public override string Tipo => "Anfibio";
    public override string AplicarTratamiento() => "Tratamiento para anfibios";
}