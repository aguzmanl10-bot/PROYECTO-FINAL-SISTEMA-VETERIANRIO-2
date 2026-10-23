namespace SistemaVeterinario.Dominio;

public class Ave : Animal
{
    public Ave(string codigo, string nombre, string especie, int edad, int propietarioId)
        : base(codigo, nombre, especie, edad, propietarioId)
    {
    }

    public override string Tipo => "Ave";
    public override string AplicarTratamiento() => "Tratamiento para aves";
}