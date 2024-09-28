using App.Domain;
using Microsoft.Extensions.DependencyInjection;
using src.Application;
using src.Interfaces;

// Classe que define o entry point do programa
internal class Startup
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddScoped<IDHT, DHT>();

        services.AddTransient<AppPokemon>();

        var serviceProvider = services.BuildServiceProvider();

        var dht = serviceProvider.GetService<IDHT>();

        if (dht == null)
        {
            throw new InvalidOperationException("Failed to retrieve the IDHT implementation");
        }

        AppPokemon.Configure(dht);

        var app = serviceProvider.GetService<AppPokemon>();
        await app?.Run();
    }
}