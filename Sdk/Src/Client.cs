using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
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
    public string ApplicationId { get; internal set; }

    /// <summary> The Client's Key. </summary>
    public string ClientKey { get; internal set; }

    /// <summary> The Client's Debug Mode. </summary>
    public bool Debug { get; internal set; }

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

    /// <summary> The Dashboards Host Name. </summary>
    public string HostName { get; internal set; }

    /// <summary> The Binaries Hash. </summary>
    public string Hash
    {
        get
        {
            try
            {
                string BinaryPath = Process.GetCurrentProcess().MainModule.FileName ?? throw new Exception("Tsar Client - Hash Unauthorized");

                using (FileStream Stream = File.OpenRead(BinaryPath))
                using (SHA256 Sha256 = SHA256.Create())
                {
                    byte[] Buffer = new byte[1024];
                    int BytesRead;

                    while ((BytesRead = Stream.Read(Buffer, 0, Buffer.Length)) > 0)
                        Sha256.TransformBlock(Buffer, 0, BytesRead, Buffer, 0);

                    Sha256.TransformFinalBlock(Buffer, 0, 0);
                    return BitConverter.ToString(Sha256.Hash).Replace("-", "").ToLower();
                }
            }
            catch { throw new Exception("Tsar Client - Hash Unauthorized"); }
        }
    }
    #endregion

        #region Constructors
        /// <summary> Initializes A New Instance Of The <see cref="Client"/> Class. </summary>
        /// <param name="Options"> The Client Options To Use When Constructing <see cref="Client"/>. </param>
    public Client(ClientData Options)
    {
        this.ApplicationId = Options.ApplicationId;
        this.ClientKey = Options.ClientKey;
        this.Debug = Options.DebugPrint;

        if (Options.ApplicationId.Length != 36)
            throw new Exception("Invalid Application Id");

        if (Options.ClientKey.Length != 124)
            throw new Exception("Invalid Client Key");

        Init Data = this.ClientCall<Init>("initialize", Options.ClientKey, new() { { "app_id", Options.ApplicationId }, });

        if (Data == null)
            throw new Exception("Tsar Client - Failed To Initialize Client.");

        this.HostName = Data.HostName;

        if (Options.DebugPrint)
            Console.WriteLine($"Tsar Client - Initialized Client With Host Name: {this.HostName}");

        if (OnInitialize != null)
            OnInitialize?.Invoke(Data);
    }
    #endregion

    #region Methods    
    /// <summary> Authenticates The User. </summary>
    /// <remarks> This Method Is Synchronous. </remarks>
    /// <returns> Returns A <see cref="User"/> Object With Contains Info About The Authenticated User. </returns>
    public User Authenticate(AuthOptions Options = default) => this.AuthenticateAsync(Options).Result;

    /// <summary> Authenticates The User. </summary>
    /// <remarks> This Method Is Asynchronous. </remarks>
    /// <returns> Returns A <see cref="User"/> Object With Contains Info About The Authenticated User. </returns>
    internal async Task<User> AuthenticateAsync(AuthOptions Options = default)
    {
        User UserData = null;

        try
        {
            UserData = await this.ClientCallAsync<User>("authenticate", this.ClientKey, new() { { "app_id", this.ApplicationId }, });
            UserData.OnClientCallRequested += ClientCallAsync<object>;
        }
        catch (Exception Exception)
        {
            switch (Exception.Message)
            {
                case "Tsar Client - Unauthorized":
                    if (Options.OpenBrowser)
                        Process.Start($"https://{this.HostName}/auth/{this.HardwareId}");
                    break;
                case "Tsar Client - Hash Unauthorized":
                    if (Options.OpenBrowser)
                        Process.Start($"https://{this.HostName}/assets?outdated=true");
                    break;
                default:
                    throw new Exception(Exception.Message);
            }
            return null;
        }

        if (OnAuthenticate != null)
            OnAuthenticate?.Invoke(UserData);

        return UserData;
    }

    /// <summary> Queries The Tsar API. </summary>
    /// <remarks> This Method Is Synchronous. </remarks>
    /// <returns> Returns The Data Object As <typeparamref name="T"/>. </returns>
    public T ClientCall<T>(string Path, string PublicKey, Dictionary<string, string> Params) => this.ClientCallAsync<T>(Path, PublicKey, Params).Result;

    /// <summary> Queries The Tsar API. </summary>
    /// <remarks> This Method Is Asynchronous. </remarks>
    /// <returns> Returns The Data Object As <typeparamref name="T"/>. </returns>
    internal async Task<T> ClientCallAsync<T>(string Path, string PublicKey, Dictionary<string, string> Params)
    {
        if (!Path.StartsWith("/"))
            Path = "/" + Path;

        Params.Add("hwid", this.HardwareId);
        Params.Add("hash", this.Hash);

        string QueryString = string.Join("&", Params.Select(x => $"{x.Key}={x.Value}"));
        HttpResponseMessage Response = await HttpClient.GetAsync($"https://tsar.cc/api/client{Path}?{QueryString}");

        if (Response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Tsar Client - {Response.StatusCode switch
            {
                HttpStatusCode.BadRequest => "Bad Request",
                HttpStatusCode.NotFound => "Not Found",
                HttpStatusCode.Unauthorized => "Unauthorized",
                HttpStatusCode.ServiceUnavailable => "App Paused",
                HttpStatusCode.Forbidden => "Hash Unauthorized",
                (HttpStatusCode)429 => "Too Many Requests",
                _ => Enum.GetName(typeof(HttpStatusCode), Response.StatusCode)
            }}");

        string Data = await Response.Content.ReadAsStringAsync();
        byte[] DataBytes = Convert.FromBase64String(JsonSerializer.Deserialize<JsonElement>(Data).GetProperty("data").ToString());

        Data<T> DataObject = new Data<T>
        {
            DataObject = JsonSerializer.Deserialize<DataObject<T>>(Encoding.UTF8.GetString(DataBytes)),
            Signature = JsonSerializer.Deserialize<JsonElement>(Data).GetProperty("signature").ToString()
        };

        if (DataBytes == null || DataObject.DataObject == null)
            throw new Exception("Tsar Client - Failed To Deserialize Data.");

        if (DataObject.DataObject.HardwareId != this.HardwareId)
            throw new Exception("Tsar Client - Hardware Id Mismatch.");

        long SystemTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        NtpClient NtpClient = new NtpClient("time.cloudflare.com");
        long UnixTime = NtpClient.Query().UtcNow.ToUnixTimeSeconds();

        if (DataObject.DataObject.Timestamp < SystemTime - 30 || Math.Abs(UnixTime - SystemTime) > 30)
            throw new Exception("Tsar Client - Timestamp Invalid Response Tampered.");

        byte[] PublicKeyBytes = Convert.FromBase64String(PublicKey);
        byte[] SignatureBytes = Convert.FromBase64String(DataObject.Signature);

        if (PublicKeyBytes == null || SignatureBytes == null)
            throw new Exception("Tsar Client - Failed To Deserialize Public Key Or Signature.");

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
                throw new Exception("Tsar Client - Signature Invalid.");
        }

        return DataObject.DataObject.Object;
    }
    #endregion

    #region Events
    /// <summary> Occurs When The <see cref="Client"/> Instance Is Disposed. </summary>
    public event EventHandler OnDispose;

    /* Not Included Or Unused Possible Addition Later On. */
    /// <summary> Occurs When The <see cref="Client"/> Instance Is Initialized. </summary>
    internal delegate void OnInitalizeDelegate(Init Object);
    internal event OnInitalizeDelegate OnInitialize;

    /// <summary> Occurs When The <see cref="Client"/> Authentication Call Is Successful And Has A Valid <see cref="User"/>. </summary>
    internal delegate void OnAuthenticateDelegate(User Object);
    internal event OnAuthenticateDelegate OnAuthenticate;
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

public class AuthOptions
{
    public bool OpenBrowser { get; set; } = true;
}

public class ClientData
{
    public string ApplicationId { get; set; }
    public string ClientKey { get; set; }
    public bool DebugPrint { get; set; }
    public string HostName { get; }
}