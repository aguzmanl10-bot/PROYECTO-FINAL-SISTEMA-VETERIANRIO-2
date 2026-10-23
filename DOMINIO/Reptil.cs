namespace SistemaVeterinario.Dominio;

public class Reptil : Animal
{
    public Reptil(string codigo, string nombre, string especie, int edad, int propietarioId)
        : base(codigo, nombre, especie, edad, propietarioId)
    {
    }

    public override string Tipo => "Reptil";
    public override string AplicarTratamiento() => "Tratamiento para reptiles";
}