using App.Domain;
using src.Domain;

namespace src.Interfaces;

// Interface que representa as 4 funções básicas do DHT
public interface IDHT
{
    Task<Node> Join();
    Task Leave(Node node);
    Task Store(int pokemonNumber, Node node);
    Task<PokemonCard> Retrieve(int pokemonNumber, Node node);
}
