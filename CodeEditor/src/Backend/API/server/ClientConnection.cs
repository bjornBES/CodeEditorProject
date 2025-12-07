
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using GlobalLibrary;
using lib.debug;

public class ClientConnection
{
    public string ClientId { get; private set; }
    public DebugStreamWriter Writer { get; }
    public DebugStreamReader Reader { get; }
    public NamedPipeServerStream Stream { get; }
    public Queue<string> ReceivedMessages = new Queue<string>();
    public ClientConnection(NamedPipeServerStream stream)
    {
        Stream = stream;
        Writer = new DebugStreamWriter(stream) { AutoFlush = true };
        Reader = new DebugStreamReader(stream, leaveOpen: true);
    }

    public void SetClientID(string clientId)
    {
        ClientId = clientId;
    }

    public void SendLine(string message)
    {
        try
        {
            Writer.WriteLine(message);
        }
        catch (Exception ex)
        {
            DebugWriter.WriteLine("Server", $"Failed to send to {ClientId}: {ex.Message}");
        }
    }

    internal byte[] ReceiveBytes(int size, out int bytesReaded, int timeOut = 1000)
    {
        DateTime dateTime = DateTime.UtcNow;
        byte[] buffer = new byte[size];
        int totalRead = 0;
        bytesReaded = 0;

        while (totalRead < size)
        {
            if (dateTime <= DateTime.UtcNow.AddMilliseconds(timeOut) && timeOut != -1)
            {
                return buffer;
            }
            if (totalRead == size)
            {
                break;
            }

            if (Stream == null)
            {
                bytesReaded = 0;
                return null;
            }

            int bytesRead = Stream.Read(buffer, totalRead, size - totalRead);
            if (bytesRead <= 0)
            {
                bytesReaded = 0;
                return null;
            }

            totalRead += bytesRead;
            bytesReaded += bytesRead;
            if (totalRead == size)
            {
                break;
            }
        }
        return buffer;
    }

    public Package ReceivePackage(string[] parts)
    {
        if (parts.Length != 3)
        {
            DebugWriter.WriteLine("Server", "Usage: package clientid,packageId,size");
            return null;
        }

        string clientId = parts[0];
        string packageType = parts[1];
        if (!int.TryParse(parts[2], out int packageSize) || packageSize <= 0)
        {
            SendLine("error: invalid size");
            DebugWriter.WriteLine("Server", "Package is invakud size"); // i think its invalid or invakud
            return null;
        }

        // Server Ident
        if (string.IsNullOrEmpty(clientId) && clientId != "AS:SERVER")
        {
            DebugWriter.WriteLine("Server", "Package is not send from server");
            return null;
        }

        // Acknowledgment
        SendLine("ACK");

        byte[] bytes = ReceiveBytes(packageSize, out int bytesRead, timeOut: -1);

        if (bytesRead == packageSize)
        {
            DebugWriter.WriteLine("Server", $"Sending Received package for package {packageType}");
            SendLine("Received package");
        }
        else
        {
            DebugWriter.WriteLine("Server", $"Sending error for package {packageType}");
            SendLine("error: incomplete data");
            return null;
        }

        string json = Encoding.UTF8.GetString(bytes);

        DataPackage dataPackage = JsonSerializer.Deserialize<DataPackage>(json);

        Package package = new Package()
        {
            ClientId = dataPackage.ClientId,
            PackageId = dataPackage.PackageId,
            PackageSize = dataPackage.PackageSize,
            PackageData = dataPackage.PackageData,
        };

        return package;
    }

    public void Dispose()
    {

    }
}