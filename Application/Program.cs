using System;
using System.Collections.Generic;
using System.Threading;

using Tsar;

namespace Application;

internal class Program
{
    public static Client TsarClient = new Client(new ClientData
    {
        ApplicationId = "e27cd770-cbb3-425a-b631-8841dbdf1912",
        ClientKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEgavgGalH2ip63mElZtLXpR8QypSVNLSxLzV52kZjXyhHo7Swto2wPZbcChYRbUFB72XNG3zzJDHcm/rjakxXmw==",
        DebugPrint = false
    });

    public static void Main(string[] args)
    {
        Console.Title = "Tsar.Net";
        Console.SetWindowSize(80, 20);

        Console.WriteLine($"\n  Client Successfully Initialized. Host Name Received By Server : {TsarClient.HostName}\n");

        Console.WriteLine("  -- Client Data --");
        Console.WriteLine($"  Application Id: {TsarClient.ApplicationId}");
        Console.WriteLine($"  Client Key: {TsarClient.ClientKey}");
        Console.WriteLine($"  Hardware Id: {TsarClient.HardwareId}");
        Console.WriteLine($"  Debug Mode: {TsarClient.Debug}");
        Console.WriteLine($"  Hash : {TsarClient.Hash}");

        Console.WriteLine("\n -- User Data --");

        User User = TsarClient.Authenticate(new AuthOptions { OpenBrowser = true });
        Console.WriteLine($"  User Id: {User.Id}");
        Console.WriteLine($"  User Name: {User.Name}");
        Console.WriteLine($"  User Avatar: {User.Avatar}");
        Console.WriteLine($"  User Session: {User.Session}");
        Console.WriteLine($"  Session Key: {User.SessionKey}");

        Console.WriteLine("\n -- Subscription Data --");
        Console.WriteLine($"  Subscription Id: {User.Subscription.Id}");
        Console.WriteLine($"  Subscription Tier: {User.Subscription.Tier}");
        Console.WriteLine($"  Subscription Expires: {User.Subscription.Expires}\n");

        while (true)
        {
            try
            {
                User.Heartbeat();
                Console.WriteLine("  Heartbeat Successful");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            catch (Exception Ex) { Console.WriteLine($"  Heartbeat Failed : {Ex.Message}"); }
        }
    }
}
