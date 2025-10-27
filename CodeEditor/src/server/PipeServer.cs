
using System.Collections.Concurrent;
using System.IO.Pipes;
using GlobalLibrary;
using lib.debug;

public class PipeServer
{
    public delegate void PackageReceivedHandler(Package dataPackage);
    public string PipeName { get; }
#nullable enable
    public event PackageReceivedHandler? OnPackageReceived;
    public readonly ConcurrentDictionary<string, ClientConnection> _clients = new();
    public PipeServer(string pipeName)
    {
        DebugWriter.AddModule("Server", "server", "server");
        DebugWriter.AddModule("Client", "clients", "clients");
        PipeName = pipeName;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        DebugWriter.WriteLine("Server", $"Listening on pipe '{PipeName}'...");

        while (!cancellationToken.IsCancellationRequested)
        {
            NamedPipeServerStream pipeStream = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous
            );

            try
            {
                // Wait for a client to connect before spawning handler
                await pipeStream.WaitForConnectionAsync(cancellationToken);
                DebugWriter.WriteLine("Server", "Client connected.");

                // Handle the connected client in a background task
                _ = Task.Run(() => HandleClientAsync(pipeStream, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await pipeStream.DisposeAsync(); // Clean up
            }
            catch (Exception ex)
            {
                DebugWriter.WriteLine("Server", $"Error accepting connection: {ex.Message}");
                await pipeStream.DisposeAsync();
            }
        }
    }

    private async Task HandleClientAsync(NamedPipeServerStream stream, CancellationToken cancellationToken)
    {
        try
        {
            ClientConnection clientConnection = new ClientConnection(stream);

            string? clientId = await clientConnection.Reader.ReadLineAsync();
            clientConnection.Reader.SetFile($"{clientId}_server" ?? "unknown");
            clientConnection.Writer.SetFile($"{clientId}_server" ?? "unknown");
            if (string.IsNullOrWhiteSpace(clientId))
            {
                DebugWriter.WriteLine("Server", "Client did not send a clientId. Disconnecting.");
                return;
            }
            clientConnection.SetClientID(clientId);

            DebugWriter.WriteLine("Server", $"Client connected with ID: {clientId}");

            _clients[clientId] = clientConnection;

            DebugWriter.WriteLine("Server", $"Client connected with ID: {clientId}");


            clientConnection.SendLine("Server:READY");
            await HandleClientMessageAsync(clientConnection, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }

    private async Task HandleClientMessageAsync(ClientConnection clientConnection, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && clientConnection.Stream.IsConnected)
        {
            string? header = await clientConnection.Reader.ReadLineAsync();
            if (header == null)
            {
                break;
            }

            string[] parts = header.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                DebugWriter.WriteLine("Server", "Usage: package clientid,packageId,size");
                continue;
            }

            string[] packageParts = parts.Last().Split(',', 3, StringSplitOptions.RemoveEmptyEntries);

            DebugWriter.WriteLine($"Server", $"from {clientConnection.ClientId} Header: {header}");

            Package dataPackage = clientConnection.ReceivePackage(packageParts);
            OnPackageReceived?.Invoke(dataPackage);
        }
    }

}