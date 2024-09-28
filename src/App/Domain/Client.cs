using System.Numerics;
using App.Helpers;
using App.Services;
using Google.Protobuf;
using Grpc.Net.Client;
using MyDHT;
using src.Domain;
using src.Helpers;
using static MyDHT.DHTService;

namespace App.Domain;

// Classe responsável por ser o Cliente do Nó, ou seja, o que realizará as chamadas
// do protocolo via gRPC
public class Client
{
    public List<Address> ListAddresses { get; set; } = ReadHelper.GetKnownAddresses();

    public async Task SendJOIN(Node node)
    {
        bool messageSent = false;

        foreach (Address address in ListAddresses)
        {
            try
            {
                if (address.Port == node.Address.Port)
                    continue;

                using var channel = GrpcChannel.ForAddress($"http://{address.IP}:{address.Port}");
                var client = new DHTServiceClient(channel);
                var reply = await client.JoinDHTAsync(new JOIN()
                {
                    Node = new MyDHT.Node()
                    {
                        Id = ByteString.CopyFrom(node.IDBytes),
                        Ip = node.Address.IP,
                        Port = int.Parse(node.Address.Port)
                    }
                });

                messageSent = true;
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Se nao conseguiu enviar a mensagem para ninguem, quer dizer
        // que é o nó inicial, entao o successor e o predecessor
        // vao ser o mesmo nó inicial.
        if (!messageSent)
            await SendJOIN_OK_Bootstrap(node);
    }

    public async Task SendJOIN_OK_Bootstrap(Node node)
    {
        using var channel = GrpcChannel.ForAddress($"http://{node.Address.IP}:{node.Address.Port}");
        var client = new DHTServiceClient(channel);
        MyDHT.Node newNode = new MyDHT.Node()
        {
            Id = ByteString.CopyFrom(node.IDBytes),
            Ip = node.Address.IP,
            Port = int.Parse(node.Address.Port)
        };
        var reply = await client.OKToJoinDHTAsync(new JOIN_OK()
        {
            Predecessor = newNode,
            Successor = newNode,
        });
    }

    public async Task ForwardJOINToSuccessor(JOIN request, Node node)
    {
        using var channel = GrpcChannel.ForAddress($"http://{node.Successor.Address.IP}:{node.Successor.Address.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.JoinDHTAsync(request);
    }

    public async Task SendJOIN_OK(MyDHT.Node newNode, Node originNode)
    {
        using var channel = GrpcChannel.ForAddress($"http://{newNode.Ip}:{newNode.Port}");
        var client = new DHTServiceClient(channel);
        Console.WriteLine($"Canal aberto em: {newNode.Ip}:{newNode.Port}");
        var reply = await client.OKToJoinDHTAsync(new JOIN_OK()
        {
            Predecessor = new MyDHT.Node()
            {
                Id = ByteString.CopyFrom(originNode.Predecessor.IDBytes),
                Ip = originNode.Predecessor.Address.IP,
                Port = int.Parse(originNode.Predecessor.Address.Port)
            },
            Successor = new MyDHT.Node()
            {
                Id = ByteString.CopyFrom(originNode.IDBytes),
                Ip = originNode.Address.IP,
                Port = int.Parse(originNode.Address.Port)
            }
        });

        BigInteger idBig = TypeHelper.ConvertByteStringToBigInteger(newNode.Id);
        
        List<PokemonCard> pokemonCards = originNode.Storage;
        foreach (PokemonCard pokemonCard in pokemonCards)
        {
            if (DHTServices.IsResponsible(idBig, originNode.ID, pokemonCard.ID))
            {
                var replyT = await client.TransferPokemonCardAsync(new TRANSFER()
                {
                    Transfer = new STORE()
                    {
                        Id = ByteString.CopyFrom(pokemonCard.IDBytes),
                        FileName = pokemonCard.Key,
                        Value = ByteString.CopyFrom(pokemonCard.Value)
                    }
                });
                originNode.RemovePokemonCard(pokemonCard.ID);
            }
        }
    }

    public async Task SendNEW_NODE(Node predecessorNode, Node newSuccessor)
    {
        using var channel = GrpcChannel.ForAddress($"http://{predecessorNode.Address.IP}:{predecessorNode.Address.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.NotifyPredecessorAboutNewNodeAsync(new NEW_NODE()
        {
            NewSuccessor = new MyDHT.Node()
            {
                Id = ByteString.CopyFrom(newSuccessor.IDBytes),
                Ip = newSuccessor.Address.IP,
                Port = int.Parse(newSuccessor.Address.Port)
            }
        });
    }

    public async Task SendLEAVE(Node leavingNode)
    {
        using var channel = GrpcChannel.ForAddress($"http://{leavingNode.Successor.Address.IP}:{leavingNode.Successor.Address.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.NotifySuccessorAboutLeaveDHTAsync(new LEAVE()
        {
            NewPredecessor = new MyDHT.Node()
            {
                Id = ByteString.CopyFrom(leavingNode.Predecessor.IDBytes),
                Ip = leavingNode.Predecessor.Address.IP,
                Port = int.Parse(leavingNode.Predecessor.Address.Port)
            }
        });

        List<PokemonCard> pokemonCards = leavingNode.Storage;
        foreach (PokemonCard pokemonCard in pokemonCards)
        {
            var replyT = await client.TransferPokemonCardAsync(new TRANSFER()
            {
                Transfer = new STORE()
                {
                    Id = ByteString.CopyFrom(pokemonCard.IDBytes),
                    FileName = pokemonCard.Key,
                    Value = ByteString.CopyFrom(pokemonCard.Value)
                }
            });
            leavingNode.RemovePokemonCard(pokemonCard.ID);
        }
    }

    public async Task SendNODE_GONE(Node leavingNode)
    {
        using var channel = GrpcChannel.ForAddress($"http://{leavingNode.Predecessor.Address.IP}:{leavingNode.Predecessor.Address.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.NotifyPredecessorAboutLeaveDHTAsync(new NODE_GONE()
        {
            NewSuccessor = new MyDHT.Node()
            {
                Id = ByteString.CopyFrom(leavingNode.Successor.IDBytes),
                Ip = leavingNode.Successor.Address.IP,
                Port = int.Parse(leavingNode.Successor.Address.Port)
            }
        });
    }

    public async Task SendSTORE(PokemonCard pokemonCard, Node node)
    {
        using var channel = GrpcChannel.ForAddress($"http://{node.Address.IP}:{node.Address.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.StorePokemonCardAsync(new STORE()
        {
            Id = ByteString.CopyFrom(pokemonCard.IDBytes),
            FileName = pokemonCard.Key,
            Value = ByteString.CopyFrom(pokemonCard.Value)
        });
    }

    public async Task ForwardSTOREToSuccessor(STORE request, Node node)
    {
        using var channel = GrpcChannel.ForAddress($"http://{node.Successor.Address.IP}:{node.Successor.Address.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.StorePokemonCardAsync(request);
    }

    public async Task SendRETRIEVE(PokemonCard pokemonCard, Node node)
    {
        using var channel = GrpcChannel.ForAddress($"http://{node.Address.IP}:{node.Address.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.RetrievePokemonCardAsync(new RETRIEVE()
        {
            Id = ByteString.CopyFrom(pokemonCard.IDBytes),
            FileName = pokemonCard.Key,
            RetrieverNode = new MyDHT.Node()
            {
                Id = ByteString.CopyFrom(node.IDBytes),
                Ip = node.Address.IP,
                Port = int.Parse(node.Address.Port)
            }
        });
    }

    public async Task ForwardRETRIEVEToSuccessor(RETRIEVE request, Node node)
    {
        using var channel = GrpcChannel.ForAddress($"http://{node.Successor.Address.IP}:{node.Successor.Address.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.RetrievePokemonCardAsync(request);
    }

    public async Task SendOK(RETRIEVE request, PokemonCard pokemonCard)
    {
        using var channel = GrpcChannel.ForAddress($"http://{request.RetrieverNode.Ip}:{request.RetrieverNode.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.ResponseOKRetrieveAsync(new OK()
        {
            PokemonCard = new STORE()
            {
                Id = ByteString.CopyFrom(pokemonCard.IDBytes),
                FileName = pokemonCard.Key,
                Value = ByteString.CopyFrom(pokemonCard.Value)
            }
        });
    }

    public async Task SendNOT_FOUND(RETRIEVE request)
    {
        using var channel = GrpcChannel.ForAddress($"http://{request.RetrieverNode.Ip}:{request.RetrieverNode.Port}");
        var client = new DHTServiceClient(channel);
        var reply = await client.ResponseNotFoundRetrieveAsync(new NOT_FOUND());
    }
}
