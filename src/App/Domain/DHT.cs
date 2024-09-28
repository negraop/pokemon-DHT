using src.Domain;
using src.Helpers;
using src.Interfaces;

namespace App.Domain;

// Classe intermediária que relaciona a interface (AppPokemon) com os métodos do Client
public class DHT : IDHT
{
    public async Task<Node> Join()
    {
        List<Address> listAddresses = ReadHelper.GetKnownAddresses();

        Node node = await TryConnectPort(listAddresses);

        Console.Write("New node: ");
        Console.WriteLine(node);

        await node.Client.SendJOIN(node);

        await node.Client.SendNEW_NODE(node.Predecessor, node);

        return node;
    }

     public async Task<Node> TryConnectPort(List<Address> listAddresses)
    {
        foreach (var address in listAddresses)
        {
            GrpcServer grpcServer = await IsPortAvailable(address);
            if (grpcServer != null)
            {
                Node node = grpcServer.GetNode();
                node.Server = grpcServer;
                return node;
            }
        }
        return null;
    }

    public async Task<GrpcServer> IsPortAvailable(Address address)
    {
        try
        {
            var grpcServer = new GrpcServer(address.IP, address.Port);
            await grpcServer.StartAsync(address);
            return await Task.FromResult(grpcServer);
        }
        catch (Exception)
        {
            Console.WriteLine($"Address {address.IP}:{address.Port} already in use.");
        }

        return null;
    }

    public async Task Leave(Node node)
    {
        await node.Client.SendLEAVE(node);

        await node.Client.SendNODE_GONE(node);
    }


    public async Task Store(int pokemonNumber, Node node)
    {
        PokemonCard pokemonCard = PokemonCard.Get(pokemonNumber);

        await node.Client.SendSTORE(pokemonCard, node);
    }

    public async Task<PokemonCard> Retrieve(int pokemonNumber, Node node)
    {
        PokemonCard pokemonCard = PokemonCard.Get(pokemonNumber);

        Console.WriteLine($"Pokemon Card GET: {pokemonCard}");

        await node.Client.SendRETRIEVE(pokemonCard, node);

        return node.RetrieverBox;
    }
    
}
