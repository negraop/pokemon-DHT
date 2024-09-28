using src.Domain;
using src.Exceptions;
using src.Helpers;
using Microsoft.Extensions.DependencyInjection;
using App.Domain;
using System.Security.Cryptography;
using System.Text;
using src.HashFunctions;

namespace Tests;

public class PokemonTests
{
    // Configurando a injeção de dependencias no Tests também
    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }
    
    [Fact]
    public void GetKnownAddresses_ShouldReturnAllKnownAddresses()
    {
        // Given

        // When
        var result = ReadHelper.GetKnownAddresses();

        // Then
        Assert.True(result.Count == 20);
    }

    [Fact]
    public void PokemonCard_Get_ShouldReturnNewPokemonCard_WhenPassingExistingNumber()
    {
        // Given
        var serviceProvider = ConfigureServices();
        int cardNumber = 17;

        // When
        var pokemonCard = PokemonCard.Get(cardNumber);

        // Then
        Assert.NotNull(pokemonCard);
        Assert.True(pokemonCard.Number == 17);
    }

    [Fact]
    public void PokemonCard_Get_ShouldThrowPokemonCardNotFoundException_WhenPassingUnexistingNumber()
    {
        // Given
        int cardNumber = 34;

        // When & Then
        var exception = Assert.Throws<PokemonCardNotFoundException>(() => PokemonCard.Get(cardNumber));
    }

    [Fact]
    public void PokemonCard_ShouldInitializeCorrectly_WithValidFileName()
    {
        // Arrange
        var expectedNumber = 10;
        var expectedName = "pikachu";
        string ValidFileName = "10-pikachu.jpg";
        byte[] ValidFileContent = [ 1, 2, 3, 4];

        // Act
        var card = new PokemonCard(ValidFileName, ValidFileContent);

        // Assert
        Assert.Equal(expectedNumber, card.Number);
        Assert.Equal(expectedName, card.Name);
        Assert.Equal(ValidFileName, card.Key);
    }

    [Fact]
    public void Address_ShouldInitializeCorrectly_WithValidIPAndPort()
    {
        // Arrange
        string ip = "127.0.0.1";
        string port = "8080";

        // Act
        var address = new Address(ip, port);

        // Assert
        Assert.Equal(ip, address.IP);
        Assert.Equal(port, address.Port);
        Assert.Equal($"{ip}:{port}", address.Key);
    }

    [Fact]
    public void Node_ShouldInitializeCorrectly_WithValidAddress()
    {
        // Arrange
        var address = new Address("127.0.0.1", "8080");

        // Act
        var node = new Node(address);

        // Assert
        Assert.Equal(address, node.Address);
        Assert.NotNull(node.Server);
        Assert.NotNull(node.Client);
    }

    [Fact]
    public void RemovePokemonCard_ShouldRemoveCard_WhenCardExists()
    {
        // Arrange
        var node = new Node(new Address("127.0.0.1", "8080"));
        var card = new PokemonCard("1-Pikachu.jpg", new byte[] { 0x01, 0x02 });
        node.Storage.Add(card);

        // Act
        node.RemovePokemonCard(card.ID);

        // Assert
        Assert.Empty(node.Storage);
    }

    [Fact]
    public void ResetRetrieverBox_ShouldClearRetrieverBox_WhenCalled()
    {
        // Arrange
        var node = new Node(new Address("127.0.0.1", "8080"));
        var card = new PokemonCard("1-Pikachu.jpg", new byte[] { 0x01, 0x02 });
        node.RetrieverBox = card;

        // Act
        node.ResetRetrieverBox();

        // Assert
        Assert.Null(node.RetrieverBox);
    }

    [Fact]
    public void GetFilesMatchPattern_ShouldReturnFiles_WhenFilesMatchPattern()
    {
        // Arrange
        string cardsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cards");
        var file1 = Path.Combine(cardsPath, "1-pikachu.jpg");
        var file2 = Path.Combine(cardsPath, "2-charizard.jpg");
        File.WriteAllBytes(file1, new byte[] { 0x0 });
        File.WriteAllBytes(file2, new byte[] { 0x0 });

        var pattern = "*-*.jpg";

        // Act
        var files = ReadHelper.GetFilesMatchPattern(pattern);

        // Assert
        Assert.NotNull(files);
        Assert.Contains(files, f => f.Contains("1-pikachu.jpg"));
        Assert.Contains(files, f => f.Contains("2-charizard.jpg"));
    }

    [Fact]
    public void GetFileName_ShouldReturnFileNameFromPath()
    {
        // Arrange
        string cardsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cards");
        var filePath = Path.Combine(cardsPath, "1-pikachu.jpg");

        // Act
        var fileName = ReadHelper.GetFileName(filePath);

        // Assert
        Assert.Equal("1-pikachu.jpg", fileName);
    }

    [Fact]
    public void GetFileBytes_ShouldReturnByteArray_WhenFileExists()
    {
        // Arrange
        string cardsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cards");
        var filePath = Path.Combine(cardsPath, "1-pikachu.jpg");
        var content = new byte[] { 0x1, 0x2, 0x3 };
        File.WriteAllBytes(filePath, content);

        // Act
        var bytes = ReadHelper.GetFileBytes(filePath);

        // Assert
        Assert.Equal(content.Length, bytes.Length);
        Assert.Equal(content, bytes);
    }

    [Fact]
    public void GetAllPokemonCardsName_ShouldReturnAllPokemonNames_WhenFilesExist()
    {
        // Arrange
        string cardsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cards");
        var file1 = Path.Combine(cardsPath, "1-pikachu.jpg");
        var file2 = Path.Combine(cardsPath, "2-charizard.jpg");
        File.WriteAllBytes(file1, new byte[] { 0x0 });
        File.WriteAllBytes(file2, new byte[] { 0x0 });

        // Act
        var pokemonNames = ReadHelper.GetAllPokemonCardsName();

        // Assert
        Assert.Contains("1 - pikachu", pokemonNames);
        Assert.Contains("2 - charizard", pokemonNames);
    }

    [Fact]
    public void GetAllPokemonCardsName_ShouldReturnEmptyList_WhenNoFilesExist()
    {
        // Arrange
        string cardsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cards");
        // Ensure no files exist in the directory
        foreach (var file in Directory.GetFiles(cardsPath))
        {
            File.Delete(file);
        }

        // Act
        var pokemonNames = ReadHelper.GetAllPokemonCardsName();

        // Assert
        Assert.NotNull(pokemonNames);
        Assert.Empty(pokemonNames);
    }

    [Fact]
    public void GetKnownAddresses_ShouldReturnListOfAddresses_WhenFileExists()
    {
        // Act
        var addresses = ReadHelper.GetKnownAddresses();

        // Assert
        Assert.NotNull(addresses);
        Assert.Contains(addresses, a => a.IP == "127.0.0.1" && a.Port == "5001");
        Assert.Contains(addresses, a => a.IP == "127.0.0.1" && a.Port == "5005");
    }

    [Fact]
    public void GenerateHashBytes_ShouldReturnCorrectHash_WhenInputIsValidString()
    {
        // Arrange
        string input = "TestString";
        byte[] expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        // Act
        byte[] resultHash = SHA256Hash.GenerateHashBytes(input);

        // Assert
        Assert.NotNull(resultHash);
        Assert.Equal(expectedHash.Length, resultHash.Length);
        Assert.Equal(expectedHash, resultHash);
    }

    [Fact]
    public void GenerateHashBytes_ShouldReturnDifferentHashes_WhenInputIsDifferent()
    {
        // Arrange
        string input1 = "TestString1";
        string input2 = "TestString2";

        // Act
        byte[] resultHash1 = SHA256Hash.GenerateHashBytes(input1);
        byte[] resultHash2 = SHA256Hash.GenerateHashBytes(input2);

        // Assert
        Assert.NotEqual(resultHash1, resultHash2);
    }
}