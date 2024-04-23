using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GuerrillaNtp;

namespace Tsar;

public class Client
{
    public string AppId { get; }
    public string ClientKey { get; }
    public string Session { get; }
    public string HardwareId { get; }
    public Subscription Subscription { get; }

    public Client(ClientOptions Options)
    {
        this.AppId = Options.AppId;
        this.ClientKey = Options.ClientKey;
        this.HardwareId = "SampleHWID";

        if (Options.DebugPrint)
            Console.WriteLine($"Client Object Created Successfully : {this.AppId} : {this.ClientKey} : {this.Session} : {this.HardwareId}");

        Data Data = ValidateUser(this.HardwareId);

        if (Data == null)
            Process.Start($"https://tsar.cc/auth/{this.AppId}/{this.HardwareId}").WaitForExit();

        this.Subscription = Data.Subscription;
        this.Session = Data.Session;
    }

    public Data ValidateUser(string HardwareId) => QueryAsync<Data>($"https://tsar.cc/api/client/subscriptions/get?app={this.AppId}&hwid={HardwareId}", this.ClientKey, HardwareId).Result;

    private async Task<T> QueryAsync<T>(string Path, string ClientKey, string HardwareId)
    {        
        byte[] PublicKeyBytes = Convert.FromBase64String(ClientKey);

        using HttpClient Client = new HttpClient { };

        HttpResponseMessage Output = await Client.GetAsync(Path);

        Client.Dispose();

        if (Output.StatusCode != HttpStatusCode.OK)
            return default(T);

        string Json = await Output.Content.ReadAsStringAsync();

        string Data = JsonSerializer.Deserialize<JsonElement>(Json).GetProperty("data").ToString();
        string Signature = JsonSerializer.Deserialize<JsonElement>(Json).GetProperty("signature").ToString();

        byte[] DataBytes = Convert.FromBase64String(Data);

        Data JsonData = JsonSerializer.Deserialize<Data>(Encoding.UTF8.GetString(DataBytes));

        if (JsonData.Hwid != HardwareId)
            throw new Exception("Hardware Id Mismatch.");

        long SystemTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        NtpClient NtpClient = new NtpClient("time.cloudflare.com");
        NtpClock NtpClock = NtpClient.Default.Query();

        long UnixTime = (NtpClock.UtcNow).ToUnixTimeSeconds();

        if (JsonData.Timestamp < SystemTime - 5 || Math.Abs(UnixTime - SystemTime) > 1)
            throw new Exception("Timestamp Invalid.");

        byte[] SignatureBytes = Convert.FromBase64String(Signature);

        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(DataBytes));
    }
}