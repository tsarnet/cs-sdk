using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using GuerrillaNtp;
using Microsoft.Win32;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;

namespace Tsar;

public class Client : IDisposable
{
    #region Properties
    private static HttpClient HttpClient { get; } = new HttpClient { };

    /// <summary> The Application Id. </summary>
    public string ApplicationId { get; }

    /// <summary> The Client's Key. </summary>
    public string ClientKey { get; }

    /// <summary> The Client's Session. </summary>
    public string Session { get; }

    /// <summary> The User's Hardware Id. </summary>
    public string HardwareId { get; }

    /// <summary> The User's Subscription. </summary>
    public Subscription Subscription { get; }
    #endregion

    #region Constructors
    /// <summary> Initializes A New Instance Of The <see cref="Client"/> Class. </summary>
    public Client(ClientOptions Options)
    {
        this.ApplicationId = Options.ApplicationId;
        this.ClientKey = Options.ClientKey;
        this.HardwareId = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid", null);

        if (Options.DebugPrint)
            Console.WriteLine($"Client Object Created Successfully : {this.ApplicationId} : {this.ClientKey} : {this.Session} : {this.HardwareId}");

        Data UserData = this.ValidateUser(this.HardwareId);

        if (UserData != null)
        {
            this.Session = UserData.Session;
            this.Subscription = UserData.Subscription;
        }
        else Process.Start($"https://tsar.cc/auth/{this.ApplicationId}/{this.HardwareId}").WaitForExit();
    }
    #endregion

    #region Methods
    /// <summary> Validates The User With <paramref name="Id"/> Specified. </summary>
    /// <param name="Id"> The User's Hardware Id. </param>
    /// <returns> <see cref="Data"/> Struct Which Contains Information About The User And Subscription. </returns>
    public Data ValidateUser(string Id) => Query<Data>($"https://tsar.cc/api/client/subscriptions/get?app={this.ApplicationId}&hwid={Id}").Result;

    /// <summary> Validates The User With <paramref name="Id"/> Specified. </summary>
    /// <param name="Id"> The User's Hardware Id. </param>
    /// <returns> <see cref="Data"/> Struct Which Contains Information About The User And Subscription. </returns>
    public async Task<Data> ValidateUserAsync(string Id) => await Query<Data>($"https://tsar.cc/api/client/subscriptions/get?app={this.ApplicationId}&hwid={Id}");

    private async Task<T> Query<T>(string Path)
    {
        HttpResponseMessage Output = await HttpClient.GetAsync(Path);

        if (Output.StatusCode != HttpStatusCode.OK)
            return default;

        string JsonText = await Output.Content.ReadAsStringAsync();

        byte[] DataBytes = Convert.FromBase64String(JsonSerializer.Deserialize<JsonElement>(JsonText).GetProperty("data").ToString());
        Data DataObject = JsonSerializer.Deserialize<Data>(Encoding.UTF8.GetString(DataBytes));

        if (DataObject.HardwareId != HardwareId)
            throw new Exception("Hardware Id Mismatch.");

        long SystemTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        NtpClient NtpClient = new NtpClient("time.cloudflare.com");
        long UnixTime = (NtpClient.Query().UtcNow).ToUnixTimeSeconds();

        if (DataObject.Timestamp < SystemTime - 5 || Math.Abs(UnixTime - SystemTime) > 1)
            throw new Exception("Timestamp Invalid.");

        byte[] PublicKeyBytes = Convert.FromBase64String(this.ClientKey);
        byte[] SignatureBytes = Convert.FromBase64String(JsonSerializer.Deserialize<JsonElement>(JsonText).GetProperty("signature").ToString());

        return JsonSerializer.Deserialize<T>(DataBytes);
    }
    #endregion

    #region Events
    /// <summary> Occurs When The <see cref="Client"/> Instance Is Disposed. </summary>
    public event EventHandler OnDispose;
    #endregion

    #region Dispose
    /// <summary> Disposes Of The <see cref="Client"/> Object. </summary>
    public void Dispose()
    {
        OnDispose?.Invoke(this, EventArgs.Empty);

        if (HttpClient != null)
            HttpClient.Dispose();
    }
    #endregion
}

public class ClientException : Exception
{
    public ClientException(InitError Error, string Message) : base(Message) { }
    public ClientException(AuthError Error, string Message) : base(Message) { }
    public ClientException(ValidateError Error, string Message) : base(Message) { }
}

public class ClientOptions
{
    public string ApplicationId { get; set; }
    public string ClientKey { get; set; }
    public bool DebugPrint { get; set; }
}