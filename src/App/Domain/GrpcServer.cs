using System.Numerics;
using App.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Domain;

// Classe responsável por inicializar e encerrar o Server do Nó, dado que é
// uma rede P2P, o Nó atua como Client e Server simultaneamente
public class GrpcServer
{
    private readonly string _ip;
    private readonly string _port;
    private WebApplication _app;

    public GrpcServer(string ip, string port)
    {
        _ip = ip;
        _port = port;
    }

    public async Task StartAsync(Address address)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        // builder.Logging.SetMinimumLevel(LogLevel.Debug);

        builder.Services.AddGrpc();

        builder.Services.AddSingleton(e => 
        {
            return new Node(address);
        });

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(int.Parse(_port), listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            });
        });

        _app = builder.Build();

        _app.MapGrpcService<DHTServices>();
        _app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
        
        await _app.StartAsync();   
    }

    public async Task StopAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
        }
    }

    public Node GetNode()
    {
        if (_app != null)
        {
            Node node = _app.Services.GetRequiredService<Node>();
            return node;
        }
        return null;
    }

}
