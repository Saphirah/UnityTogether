using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class MyHub : Hub
{
    public readonly List<(int, string, string)> packages = new();
    public readonly List<(string, string, string)> files = new();

    public async Task SendPackage(int packageID, string username, string package)
    {
        Console.WriteLine("Package received from " + username + ": " + package);
        packages.Add((packageID, username, package));
        await Clients.Others.SendAsync("ReceivePackage", packageID, username, package);
    }

    public async Task SendFile(string username, string filePath, string fileData)
    {
        Console.WriteLine("File received from " + username + ": " + filePath);
        files.Add((username, filePath, fileData));
        await Clients.Others.SendAsync("ReceiveFile", username, filePath, fileData);
    }
    
    public override async Task OnConnectedAsync()
    {
        foreach (var (username, filePath, fileData) in files)
            await Clients.Caller.SendAsync("ReceiveFile", username, filePath, fileData);
        
        foreach (var (packageID, username, package) in packages)
            await Clients.Caller.SendAsync("ReceivePackage", packageID, username, package);
        
        await base.OnConnectedAsync();
    }
}