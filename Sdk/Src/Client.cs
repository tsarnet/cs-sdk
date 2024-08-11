using System;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GuerrillaNtp;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

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
    public string HardwareId
    {
        get
        {
            ManagementObjectSearcher Searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystemProduct");
            ManagementObjectCollection Collection = Searcher.Get();

            foreach (ManagementObject ManagementObject in Collection)
                return ManagementObject["UUID"].ToString();

            return null;
        }
    }

    /// <summary> The User's Subscription. </summary>
    public Subscription Subscription { get; }
    #endregion

    #region Constructors
    /// <summary> Initializes A New Instance Of The <see cref="Client"/> Class. </summary>
    /// <param name="Options"> The Client Options To Use When Constructing <see cref="Client"/>. </param>
    public Client(ClientOptions Options)
    {
        this.ApplicationId = Options.ApplicationId;
        this.ClientKey = Options.ClientKey;

        Data UserData = this.ValidateUser(this.HardwareId);

        if (UserData != null)
        {
            this.Session = UserData.Session;
            this.Subscription = UserData.Subscription;
        }
        else
        {
            Process.Start($"https://tsar.cc/auth/{this.ApplicationId}/{this.HardwareId}").WaitForExit();
            Environment.Exit(0);
        }

        if (Options.DebugPrint)
            Console.WriteLine($"Client Object Created Successfully :\nApplication Id - {this.ApplicationId}\nClient Key - {this.ClientKey}\nSession - {this.Session}\nHardware Id - {this.HardwareId}\n");
    }
    #endregion

    #region Methods
    /// <summary> Validates The User. </summary>
    /// <returns> <see cref="ValidateData"/> Struct Which Contains Information About The Current User Status. </returns>
    public ValidateData Validate() => this.Query<ValidateData>($"https://tsar.cc/api/client/validate?app={this.ApplicationId}&hwid={this.HardwareId}&session={this.Session}").Result;

    /// <summary> Validates The User With <paramref name="Id"/> Specified. </summary>
    /// <param name="Id"> The User's Hardware Id. </param>
    /// <returns> <see cref="Data"/> Struct Which Contains Information About The User And Subscription. </returns>
    internal Data ValidateUser(string Id) => this.Query<Data>($"https://tsar.cc/api/client/initialize?app={this.ApplicationId}&hwid={Id}").Result;

    /// <summary> Validates The User With <paramref name="Id"/> Specified. </summary>
    /// <param name="Id"> The User's Hardware Id. </param>
    /// <returns> <see cref="Data"/> Struct Which Contains Information About The User And Subscription. </returns>
    internal async Task<Data> ValidateUserAsync(string Id) => await this.Query<Data>($"https://tsar.cc/api/client/initialize?app={this.ApplicationId}&hwid={Id}");

    private async Task<T> Query<T>(string Path)
    {
        HttpResponseMessage Output = await HttpClient.GetAsync(Path);

        if (Output.StatusCode != HttpStatusCode.OK)
            return default;

        string JsonText = await Output.Content.ReadAsStringAsync();

        byte[] DataBytes = Convert.FromBase64String(JsonSerializer.Deserialize<JsonElement>(JsonText).GetProperty("data").ToString());
        Data DataObject = JsonSerializer.Deserialize<Data>(Encoding.UTF8.GetString(DataBytes));

        if (DataObject.HardwareId != this.HardwareId)
            throw new Exception("Hardware Id Mismatch.");

        long SystemTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        NtpClient NtpClient = new NtpClient("time.cloudflare.com");
        long UnixTime = NtpClient.Query().UtcNow.ToUnixTimeSeconds();

        if (DataObject.Timestamp < SystemTime - 30 || Math.Abs(UnixTime - SystemTime) > 30)
            throw new Exception("Timestamp Invalid.");

        byte[] PublicKeyBytes = Convert.FromBase64String(this.ClientKey);
        byte[] SignatureBytes = Convert.FromBase64String(JsonSerializer.Deserialize<JsonElement>(JsonText).GetProperty("signature").ToString());

        Asn1Object Object = Asn1Object.FromByteArray(PublicKeyBytes);
        byte[] EncodedPublicKey = Object.GetEncoded();

        SubjectPublicKeyInfo PublicKeyInfo = SubjectPublicKeyInfo.GetInstance(EncodedPublicKey);
        ECPublicKeyParameters ecPublicKeyParameters = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(PublicKeyInfo);

        using (ECDsa Ecdsa = ECDsa.Create())
        {
            ECParameters ecParams = new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = ecPublicKeyParameters.Q.AffineXCoord.GetEncoded(),
                    Y = ecPublicKeyParameters.Q.AffineYCoord.GetEncoded()
                }
            };

            Ecdsa.ImportParameters(ecParams);

            if (!Ecdsa.VerifyData(DataBytes, SignatureBytes, HashAlgorithmName.SHA256))
                throw new Exception("Signature Invalid.");
        }

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

public class ClientOptions
{
    public string ApplicationId { get; set; }
    public string ClientKey { get; set; }
    public bool DebugPrint { get; set; }
}