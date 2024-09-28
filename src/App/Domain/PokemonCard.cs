using System.Numerics;
using Google.Protobuf.WellKnownTypes;
using src.Exceptions;
using src.HashFunctions;
using src.Helpers;

namespace src.Domain;

// Classe que define o modelo do artefato que é transmitido na rede.
// Contém informações como Nome da carta, número, Key (que é o nome do arquivo .jpg),
// Valor, que contém o conteúdo do jpg descrito em array de bytes, etc.
public class PokemonCard
{
    public BigInteger ID { get; set; }
    public byte[] IDBytes { get; set; }
    public int Number { get; set; }
    public string Name { get; set; }
    public string Key { get; set; }
    public int Size { get; set; }
    public byte[] Value { get; set; }

    public PokemonCard(string fileName, byte[] content)
    {
        Number = GetNumber(fileName);
        Name = GetName(fileName);
        Key = fileName;
        Value = content;
        Size = content.Length;

        ID = SHA256Hash.GenerateHashBigInteger(Key);
        IDBytes = SHA256Hash.GenerateHashBytes(Key);
    }

    private int GetNumber(string fileName)
    {
        string[] parts = fileName.Split('-');
        return int.Parse(parts[0]);
    }

    private string GetName(string fileName)
    {
        string[] parts = fileName.Split('-');
        return parts[1].Replace(".jpg", "");
    }

    public static PokemonCard Get(int number)
    {
        string filePattern = $"{number}-*.jpg";

        string[] files = ReadHelper.GetFilesMatchPattern(filePattern);

        if (files.Length == 0)
            throw new PokemonCardNotFoundException("Pokemon Card not found! Try another number.");

        string filePath = files[0];

        try
        {
            string fileName = ReadHelper.GetFileName(filePath);
            byte[] fileContent = ReadHelper.GetFileBytes(filePath);

            return new PokemonCard(fileName, fileContent);
        }
        catch (Exception ex)
        {
            throw new FileUnreadableException(ex.Message);
        }
    }

    public override string ToString()
    {
        return $"PokemonCard [ID={ID}, Name= {Number}-{Name}]";
    }
}
