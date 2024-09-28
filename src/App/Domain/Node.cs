using System.Numerics;
using App.Helpers;
using Google.Protobuf;
using MyDHT;
using src.Domain;
using src.HashFunctions;

namespace App.Domain;

// Classe principal que modela o Nó.
// Possui alguns atributos como ID, IDBytes (que é transmitidos nas mensagens gRPC),
// Nós Sucessor e Predecessor, Server e Client, Storage (que armazena as cartas de pokemon)
// e RetrieverBox, que é uma abstração da resposta da mensagem RETRIEVER.
// Então sempre quando recebe um carta (se encontrada), ele atualiza esse atributo para que seja
// possível mostrar na tela posteriormente. Após mostrar, esse atributo é resetado, para permitir
// que outras cartas sejam buscadas
public class Node
{
    public BigInteger ID { get; set; }
    public byte[] IDBytes { get; set; }
    public Address Address { get; set; }
    public Node Successor { get; set; }
    public Node Predecessor { get; set; }
    public GrpcServer Server { get; set; }
    public Client Client { get; set; }
    public List<PokemonCard> Storage { get; set; }
    public PokemonCard RetrieverBox { get; set; }

    public Node(Address address)
    {
        Address = address;
        Server = new GrpcServer(address.IP, address.Port);
        Client = new Client();
        Storage = new List<PokemonCard>();

        ID = SHA256Hash.GenerateHashBigInteger(Address.Key);
        IDBytes = SHA256Hash.GenerateHashBytes(Address.Key);
    }

    public Empty SetSuccessorAndPredecessorFromJoinOk(JOIN_OK request)
    {
        Address successorAddress = new Address(request.Successor.Ip, request.Successor.Port.ToString());
        Address predecessorAddress = new Address(request.Predecessor.Ip, request.Predecessor.Port.ToString());
        Successor = new Node(successorAddress);
        Predecessor = new Node(predecessorAddress);
        return new Empty();
    }

    public Empty SetPredecessorAfterSentJoinOk(JOIN request)
    {
        Address predecessorAddress = new Address(request.Node.Ip, request.Node.Port.ToString());
        Predecessor = new Node(predecessorAddress);
        return new Empty();
    }

    public Empty SetNewSuccessorFromNewNode(NEW_NODE request)
    {
        if (ID == Successor.ID)
            return BootstrapSetNewSuccessorAndPredecessor(request);

        Address successorAddress = new Address(request.NewSuccessor.Ip, request.NewSuccessor.Port.ToString());
        Successor = new Node(successorAddress);
        return new Empty();
    }

    public Empty BootstrapSetNewSuccessorAndPredecessor(NEW_NODE request)
    {
        Address bothAddress = new Address(request.NewSuccessor.Ip, request.NewSuccessor.Port.ToString());
        Successor = new Node(bothAddress);
        Predecessor = new Node(bothAddress);
        return new Empty();
    }

    public Empty SetNewPredecessorFromLeave(LEAVE request)
    {
        Address predecessorAddress = new Address(request.NewPredecessor.Ip, request.NewPredecessor.Port.ToString());
        Predecessor = new Node(predecessorAddress);
        return new Empty();
    }

    public Empty SetNewSuccessorFromNodeGone(NODE_GONE request)
    {
        Address successorAddress = new Address(request.NewSuccessor.Ip, request.NewSuccessor.Port.ToString());
        Successor = new Node(successorAddress);
        return new Empty();
    }

    public Empty StoreNewPokemonCard(STORE request)
    {
        PokemonCard pokemonCard = new PokemonCard(request.FileName, request.Value.ToArray());
        if (!Storage.Any(p => p.Name == pokemonCard.Name))
        {
            Storage.Add(pokemonCard);
        }
        return new Empty();
    }

    public PokemonCard SearchPokemonCard(RETRIEVE request)
    {
        PokemonCard pokemonCard = Storage.FirstOrDefault(p => p.Key == request.FileName);
        return pokemonCard;
    }

    public Empty SetRetrieverBox(OK request)
    {
        PokemonCard pokemonCard = new PokemonCard(request.PokemonCard.FileName, request.PokemonCard.Value.ToArray());
        RetrieverBox = pokemonCard;
        return new Empty();
    }

    public void ResetRetrieverBox()
    {
        RetrieverBox = null;
    }

    public void RemovePokemonCard(BigInteger idBig)
    {
        Storage = Storage.Where(p => p.ID != idBig).ToList();
    }

    public Empty TransferPokemonCard(TRANSFER request)
    {
        PokemonCard pokemonCard = new PokemonCard(request.Transfer.FileName, request.Transfer.Value.ToArray());
        if (!Storage.Any(p => p.Name == pokemonCard.Name))
        {
            Storage.Add(pokemonCard);
        }
        return new Empty();
    }

    public override string ToString()
    {
        return $"Node [ID={ID}, Address={Address.Key}]";
    }

    public string ShowConnectedNode()
    {
        string text = $"Node [ID={ID}, Address={Address.Key}, Succ={Successor.ID}, Pred={Predecessor.ID}]\n\n";
        text += "***** Storage *****\n\n";
        if (Storage.Count == 0)
            text += "Nothing here...\n";
        else
        {
            foreach (PokemonCard pokemonCard in Storage)
            {
                text += pokemonCard.ToString() + "\n";
            }
        }
        text += "\n*******************\n";
        return text;
    }
}
