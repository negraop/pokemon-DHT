namespace src.Exceptions;

public class PokemonCardNotFoundException : Exception
{
    public PokemonCardNotFoundException(string message)
        : base(message) { }
}
