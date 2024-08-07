using System;
using System.Threading;

using Tsar;

namespace Application;

internal class Program
{
    public static Client TsarClient = new Client(new ClientOptions
    {
        ApplicationId = "e27cd770-cbb3-425a-b631-8841dbdf1912",
        ClientKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEgavgGalH2ip63mElZtLXpR8QypSVNLSxLzV52kZjXyhHo7Swto2wPZbcChYRbUFB72XNG3zzJDHcm/rjakxXmw==",
        DebugPrint = true
    });

    public static void Main(string[] args)
    {
        Console.Title = "Tsar.Net";
        Console.SetWindowSize(80, 20);

        Console.WriteLine("\n  -- Subscription Information --\n");
        Console.WriteLine($"  Subscription Id: {TsarClient.Subscription.Id}");
        Console.WriteLine($"  Subscription Expires: {TsarClient.Subscription.Expires}");
        Console.WriteLine($"  User Id: {TsarClient.Subscription.User.Id}");
        Console.WriteLine($"  Username: {TsarClient.Subscription.User.Username}");
        Console.WriteLine($"  Avatar: {TsarClient.Subscription.User.Avatar}");

        Console.WriteLine("\n  -- Validate Information --\n");

        ValidateData ValidateData = TsarClient.Validate();
        Console.WriteLine($"  Is Valid: {ValidateData.Valid}");
        Console.WriteLine($"  Hardware Id: {ValidateData.HardwareId}");
        Console.WriteLine($"  TimeStamp: {ValidateData.Timestamp}");

        Thread.Sleep(-1);
    }
}
