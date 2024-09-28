using System.Numerics;
using App.Helpers;
using Grpc.Core;
using MyDHT;
using src.Domain;

namespace App.Services;

// Classe que implementa os métodos do gRPC no lado do Server
public class DHTServices : DHTService.DHTServiceBase
{
    private readonly Domain.Node _node;
    public DHTServices(Domain.Node node)
    {
        _node = node;
    }

    public override async Task<Empty> JoinDHT(JOIN request, ServerCallContext context)
    {
        // Significa que tem só um nó na rede
        if (_node.ID == _node.Predecessor.ID)
        {
            await _node.Client.SendJOIN_OK(request.Node, _node);
        } 
        else
        {
            BigInteger idToCheckBig = TypeHelper.ConvertByteStringToBigInteger(request.Node.Id);
            if (IsResponsible(_node.ID, _node.Predecessor.ID, idToCheckBig))
            {
                await _node.Client.SendJOIN_OK(request.Node, _node);
                _node.SetPredecessorAfterSentJoinOk(request);
            }
            else
                await _node.Client.ForwardJOINToSuccessor(request, _node);
        }
        return await Task.FromResult(new Empty());
    }

    public static bool IsResponsible(BigInteger nodeId, BigInteger predecessorId, BigInteger idToCheckBig)
    {
        Console.WriteLine($"NodeId={nodeId}, PredecessorId={predecessorId}, idToCheck={idToCheckBig}");
        // Sem dar a volta no anel
        if (nodeId > predecessorId)
            return nodeId >= idToCheckBig && idToCheckBig > predecessorId;

        // Deu a volta no anel
        else
            return idToCheckBig <= nodeId || idToCheckBig > predecessorId;
    }

    public override async Task<Empty> OKToJoinDHT(JOIN_OK request, ServerCallContext context)
    {
        Console.WriteLine($"\nClient: {context.Peer}");
        Console.WriteLine($"Method: {context.Method}");

        return await Task.FromResult(_node.SetSuccessorAndPredecessorFromJoinOk(request));
    }

    public override async Task<Empty> NotifyPredecessorAboutNewNode(NEW_NODE request, ServerCallContext context)
    {
        Console.WriteLine($"\nClient: {context.Peer}");
        Console.WriteLine($"Method: {context.Method}");

        return await Task.FromResult(_node.SetNewSuccessorFromNewNode(request));
    }

    public override async Task<Empty> NotifySuccessorAboutLeaveDHT(LEAVE request, ServerCallContext context)
    {
        Console.WriteLine($"\nClient: {context.Peer}");
        Console.WriteLine($"Method: {context.Method}");

        return await Task.FromResult(_node.SetNewPredecessorFromLeave(request));
    }

    public override async Task<Empty> NotifyPredecessorAboutLeaveDHT(NODE_GONE request, ServerCallContext context)
    {
        Console.WriteLine($"\nClient: {context.Peer}");
        Console.WriteLine($"Method: {context.Method}");

        return await Task.FromResult(_node.SetNewSuccessorFromNodeGone(request));
    }

    public override async Task<Empty> StorePokemonCard(STORE request, ServerCallContext context)
    {
        // Significa que tem só um nó na rede
        if (_node.ID == _node.Predecessor.ID)
        {
            _node.StoreNewPokemonCard(request);
        }
        else
        {
            BigInteger idToCheckBig = TypeHelper.ConvertByteStringToBigInteger(request.Id);
            if (IsResponsible(_node.ID, _node.Predecessor.ID, idToCheckBig))
            {
                _node.StoreNewPokemonCard(request);
            }
            else
            {
                await _node.Client.ForwardSTOREToSuccessor(request, _node);
            }
        }
        return await Task.FromResult(new Empty());
    }

    public override async Task<Empty> RetrievePokemonCard(RETRIEVE request, ServerCallContext context)
    {
        // Significa que tem só um nó na rede
        if (_node.ID == _node.Predecessor.ID)
        {
            await HandleRetrieveRequest(request);
        }
        else
        {
            BigInteger idToCheckBig = TypeHelper.ConvertByteStringToBigInteger(request.Id);
            if (IsResponsible(_node.ID, _node.Predecessor.ID, idToCheckBig))
            {
                await HandleRetrieveRequest(request);
            }
            else
            {
                await _node.Client.ForwardRETRIEVEToSuccessor(request, _node);
            }
        }
        return await Task.FromResult(new Empty());
    }

    private async Task HandleRetrieveRequest(RETRIEVE request)
    {
        PokemonCard pokemonCard = _node.SearchPokemonCard(request);

        if (pokemonCard == null)
            await _node.Client.SendNOT_FOUND(request);
        else
            await _node.Client.SendOK(request, pokemonCard);
    }

    public override async Task<Empty> ResponseOKRetrieve(OK request, ServerCallContext context)
    {
        Console.WriteLine($"\nClient: {context.Peer}");
        Console.WriteLine($"Method: {context.Method}");

        return await Task.FromResult(_node.SetRetrieverBox(request));
    }

    public override async Task<Empty> ResponseNotFoundRetrieve(NOT_FOUND request, ServerCallContext context)
    {
        Console.WriteLine($"\nClient: {context.Peer}");
        Console.WriteLine($"Method: {context.Method}");

        return await Task.FromResult(new Empty());
    }

    public override async Task<Empty> TransferPokemonCard(TRANSFER request, ServerCallContext context)
    {
        Console.WriteLine($"\nClient: {context.Peer}");
        Console.WriteLine($"Method: {context.Method}");

        return await Task.FromResult(_node.TransferPokemonCard(request));
    }
}
