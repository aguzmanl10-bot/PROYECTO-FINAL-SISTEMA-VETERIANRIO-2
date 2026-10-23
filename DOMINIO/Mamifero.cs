namespace SistemaVeterinario.Dominio;

public class Mamifero : Animal
{
    public Mamifero(string codigo, string nombre, string especie, int edad, int propietarioId)
        : base(codigo, nombre, especie, edad, propietarioId)
    {
    }

    public override string Tipo => "Mamifero";
    public override string AplicarTratamiento() => "Tratamiento para mamiferos";
}