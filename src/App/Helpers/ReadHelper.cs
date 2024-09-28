
using App.Domain;
using src.Exceptions;

namespace src.Helpers;

// Classe Helper de Leitura, responsável por desserializar os arquivos nos contextos
// da aplicação, como endereços e cartas de pokemon.
public static class ReadHelper
{
    private static readonly string KNOWN_HOST_FILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KnownHostList.txt");
    private static readonly string CARDS_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cards");
    private static readonly object _lock = new object();

    public static List<Address> GetKnownAddresses()
    {
        try
        {
            string[] addressesString;
            List<Address> addresses = new List<Address>();

            lock (_lock)
            {
                addressesString = File.ReadAllLines(KNOWN_HOST_FILE);
            }

            foreach (var address in addressesString)
            {
                if (address != null)
                {
                    string[] parts = address.Split(':');
                    addresses.Add(new Address(parts[0], parts[1]));
                }
            }

            Random rnd = new Random();

            return addresses.OrderBy(_ => rnd.Next()).ToList();
        }
        catch (Exception ex)
        {
            throw new FileUnreadableException(ex.Message);
        }
    }

    public static string[] GetFilesMatchPattern(string pattern)
    {
        string[] files = [];

        lock (_lock)
        {
            files = Directory.GetFiles(CARDS_PATH, pattern);
            files = files.OrderBy(OrderByNumber()).ToArray();
        }

        return files;
    }

    private static Func<string, int> OrderByNumber()
    {
        return file =>
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string[] parts = fileName.Split('-');

            if (int.TryParse(parts[0], out int number))
                return number;

            return 0;
        };
    }

    public static string GetFileName(string filePath)
    {
        string fileName = "";

        lock (_lock)
        {
            fileName = Path.GetFileName(filePath);
        }

        return fileName;
    }

    public static byte[] GetFileBytes(string filePath)
    {
        byte[] bytes = [];

        lock (_lock)
        {
            bytes = File.ReadAllBytes(filePath);
        }

        return bytes;
    }

    public static List<string> GetAllPokemonCardsName()
    {
        List<string> filesName = new List<string>();

        lock (_lock)
        {
            string[] filesPath = GetFilesMatchPattern($"*-*.jpg");
            
            foreach (string filaPath in filesPath)
            {
                string fileName = GetFileName(filaPath);
                string[] parts = fileName.Split('-');
                string number = parts[0];
                string name = parts[1].Replace(".jpg", "");
                filesName.Add($"{number} - {name}");
            }
        }

        return filesName;
    }
}
