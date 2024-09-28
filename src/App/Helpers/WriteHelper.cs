using App.Domain;
using src.Domain;

namespace src.Helpers;

// Classe Helper para realizar a escrita na tela
public static class WriteHelper
{
    public static void PrintLogo()
    {
        Console.WriteLine(@"
 ____        __                                            ____    __  __  ______   
/\  _`\     /\ \                                          /\  _`\ /\ \/\ \/\__  _\  
\ \ \L\ \___\ \ \/'\      __    ___ ___     ___    ___    \ \ \/\ \ \ \_\ \/_/\ \/  
 \ \ ,__/ __`\ \ , <    /'__`\/' __` __`\  / __`\/' _ `\   \ \ \ \ \ \  _  \ \ \ \  
  \ \ \/\ \L\ \ \ \\`\ /\  __//\ \/\ \/\ \/\ \L\ /\ \/\ \   \ \ \_\ \ \ \ \ \ \ \ \ 
   \ \_\ \____/\ \_\ \_\ \____\ \_\ \_\ \_\ \____\ \_\ \_\   \ \____/\ \_\ \_\ \ \_\
    \/_/\/___/  \/_/\/_/\/____/\/_/\/_/\/_/\/___/ \/_/\/_/    \/___/  \/_/\/_/  \/_/
                                                                                    
    By: Pedro Antunes Negrão
        ");
    }

    public static int PrintMenu()
    {
        PrintLogo();
        Console.WriteLine("**************** MENU ****************");
        Console.WriteLine("*                                    *");
        Console.WriteLine("*  Select one option:                *");
        Console.WriteLine("*                                    *");
        Console.WriteLine("*  1) Join DHT                       *");
        Console.WriteLine("*  2) Store Pokemon Card             *");
        Console.WriteLine("*  3) Retrieve Pokemon Card          *");
        Console.WriteLine("*  4) Leave DHT                      *");
        Console.WriteLine("*                                    *");
        Console.WriteLine("*  5) Show Current Node              *");
        Console.WriteLine("*  0) Exit app                       *");
        Console.WriteLine("*                                    *");
        Console.WriteLine("**************************************\n");
        Console.Write("Option: ");
        string input = Console.ReadLine();
        if (int.TryParse(input, out int number) && IsValidNumber(input))
            return number;
        else
            return InvalidOption();
    }

    public static int InvalidOption()
    {
        Console.Clear();
        Console.WriteLine("\nOnly [0..5] numbers are allowed. Please try again!");
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    public static int NodeCreated(Node node)
    {
        Console.Clear();
        Console.WriteLine($"\n{node} created successfully!");
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    public static int ShowCurrentNode(Node node)
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine(node.ShowConnectedNode());
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    public static int NodeAlreadyExistsInDHT()
    {
        Console.Clear();
        Console.WriteLine("\nNode already part of DHT!");
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    public static int NodeMissingInDHT()
    {
        Console.Clear();
        Console.WriteLine("\nNode is missing =/ ! Try to join DHT first ;)");
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    public static int NodeLeftDHT()
    {
        Console.Clear();
        Console.WriteLine("\nNode left DHT successfully!");
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    public static int ShowAllPokemonCards()
    {
        List<string> allPokemonCards = ReadHelper.GetAllPokemonCardsName();

        Console.Clear();
        Console.WriteLine("\nChoose one of the available pokemon cards to store:\n");
        Console.WriteLine("********* POKEMON CARDS *********");
        Console.WriteLine("*                               *");

        foreach (string pokemonCard in allPokemonCards)
        {
            int length = pokemonCard.Length;
            int totalLength = 28 - length;
            string text = $"*   ";
            text += pokemonCard;
            for (int i = 0; i < totalLength; i++)
            {
                text += " ";
            }
            text += "*";
            Console.WriteLine(text);
        }

        Console.WriteLine("*                               *");
        Console.WriteLine("*********************************\n");
        Console.Write("Pokemon Card: ");
        string input = Console.ReadLine();
        if (IsValidPokemonCard(input))
            return int.Parse(input);
        else
            return InvalidPokemonCard();
    }

    public static int InvalidPokemonCard()
    {
        Console.Clear();
        Console.WriteLine("\nOnly [1..20] numbers are allowed. Please try again!");
        Console.WriteLine("\nPress any key to return to pokemon cards menu...");
        Console.ReadKey();
        Console.Clear();
        return ShowAllPokemonCards();
    }

    public static int PokemonCardStored()
    {
        Console.Clear();
        Console.WriteLine("\nPokemon card stored successfully!");
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    public static int RetrievePokemonCard()
    {
        Console.Clear();
        Console.Write("\nEnter the pokemon card number to retrieve ([1..20]): ");
        string input = Console.ReadLine();
        if (IsValidPokemonCard(input))
            return int.Parse(input);
        else
            return InvalidRetrievePokemonCard();
    }

    public static int InvalidRetrievePokemonCard()
    {
        Console.Clear();
        Console.WriteLine("\nOnly [1..20] numbers are allowed. Please try again!");
        Console.WriteLine("\nPress any key to return to retrieve menu...");
        Console.ReadKey();
        Console.Clear();
        return RetrievePokemonCard();
    }

    public static int FoundPokemonCard(PokemonCard pokemonCard)
    {
        Console.Clear();
        Console.WriteLine("\nPokemon Card found!\n");
        Console.WriteLine(pokemonCard);
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    public static int NotFoundPokemonCard()
    {
        Console.Clear();
        Console.WriteLine("\nPokemon Card not found! =/\n");
        Console.WriteLine("\nPress any key to return to menu to try another card...");
        Console.ReadKey();
        Console.Clear();
        return PrintMenu();
    }

    private static bool IsValidPokemonCard(string input)
    {
        if (int.TryParse(input, out int number))
        {
            if (number >= 1 && number <= 20)
                return true;
        }

        return false;
    }

    private static bool IsValidNumber(string input)
    {
        return input == "0" || input == "1" || input == "2" || 
        input == "3" || input == "4" || input == "5";
    }
}
