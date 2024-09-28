using App.Domain;
using src.Domain;
using src.Exceptions;
using src.Helpers;
using src.Interfaces;

namespace src.Application;

// Classe responsável por interagir com o usuário via CLI
public class AppPokemon
{
    private static IDHT _dht;
    public Node AppNode { get; set; }
    public AppPokemon(IDHT dht)
    {
        _dht = dht;
    }
    public static void Configure(IDHT dht)
    {
        _dht = dht ?? throw new ArgumentNullException(nameof(dht));
    }
    public async Task Run()
    {
        try
        {
            int input = WriteHelper.PrintMenu();

            await SelectOption(input);
        }
        catch (PokemonCardNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (FileUnreadableException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task SelectOption(int input)
    {
        switch(input)
        {
            case 1: await JoinDHT();
                break;
            case 2: await StorePokemonCard();
                break;
            case 3: await RetrievePokemonCard();
                break;
            case 4: await LeaveDHT();
                break;
            case 5: await ShowCurrentNode();
                break;
            case 0: ExitApp();
                break;
            default: Console.WriteLine("\nInvalid input!");
                break; 
        }
    }

    public async Task JoinDHT()
    {
        int input;

        if (AppNode != null)
        {
            input = WriteHelper.NodeAlreadyExistsInDHT();
        }
        else
        {
            AppNode = await _dht.Join();

            input = WriteHelper.NodeCreated(AppNode);
        }
            
        await SelectOption(input);
    }

    public async Task ShowCurrentNode()
    {
        int input;

        if (AppNode == null)
            input = WriteHelper.NodeMissingInDHT();
        else
            input = WriteHelper.ShowCurrentNode(AppNode);

        await SelectOption(input);
    }

    public async Task StorePokemonCard()
    {
        int input;

        if (AppNode == null)
            input = WriteHelper.NodeMissingInDHT();
        else
        {
            input = WriteHelper.ShowAllPokemonCards();

            await _dht.Store(input, AppNode);

            input = WriteHelper.PokemonCardStored();
        }

        await SelectOption(input);
    }

    public async Task RetrievePokemonCard()
    {
        int input; 

        if (AppNode == null)
            input = WriteHelper.NodeMissingInDHT();
        else
        {
            input = WriteHelper.RetrievePokemonCard();

            PokemonCard pokemonCard = await _dht.Retrieve(input, AppNode);

            if (pokemonCard == null)
            {
                input = WriteHelper.NotFoundPokemonCard();
            }
            else
            {
                input = WriteHelper.FoundPokemonCard(pokemonCard);
                AppNode.ResetRetrieverBox();
            }
        }    

        await SelectOption(input);
    }

    public async Task LeaveDHT()
    {
        int input;

        if (AppNode == null)
        {
            input = WriteHelper.NodeMissingInDHT();
        }
        else
        {
            await _dht.Leave(AppNode);

            input = WriteHelper.NodeLeftDHT();
        }

        AppNode = null;

        await SelectOption(input);
    }

    public void ExitApp()
    {
        Console.Clear();
        Console.WriteLine("\nThanks for using my PokemonDHT! =)\n");
    }
}
