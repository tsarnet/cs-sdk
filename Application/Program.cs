using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Tsar;

namespace Application;

internal class Program
{
    public static Client TsarClient = new Client(new ClientOptions
    {
        ApplicationId = "5cce113e-b84f-43d3-a9c8-119579a4df0f",
        ClientKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEnecUBQwWprvrTapicgGus20/LBPxIF5zKMvciHuniPg/T4/jBeyum36bNVvjFVtGKgQKbHChB8wsSWpVLghwag==",
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

        Thread.Sleep(-1);
    }
}
