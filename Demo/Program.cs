using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tsar;

namespace Demo;

internal class Program
{
    public static void Main(string[] args)
    {
        Client TsarClient = new Client(new ClientOptions
        {
            AppId = "5cce113e-b84f-43d3-a9c8-119579a4df0f",
            ClientKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEnecUBQwWprvrTapicgGus20/LBPxIF5zKMvciHuniPg/T4/jBeyum36bNVvjFVtGKgQKbHChB8wsSWpVLghwag==",
            DebugPrint = true
        });

        Console.WriteLine($"\n  -- Subscription Information -- \n  Subscription ID : {TsarClient.Subscription.Id}");
        Console.WriteLine($"  User ID : {TsarClient.Subscription.User.Id}");
        Console.WriteLine($"  Username : {TsarClient.Subscription.User.Username}");
        Console.WriteLine($"  Session : {TsarClient.Session}");

        Thread.Sleep(-1);
    }
}
