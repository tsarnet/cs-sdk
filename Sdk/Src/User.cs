using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tsar;

public class Subscription
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("tier")]
    public int Tier { get; set; }

    [JsonPropertyName("expires")]
    public object Expires { get; set; }
}

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("subscription")]
    public Subscription Subscription { get; set; }

    [JsonPropertyName("session")]
    public string Session { get; set; }

    [JsonPropertyName("session_key")]
    public string SessionKey { get; set; }

    /// <summary> Sends A Heartbeat To The Server. </summary>
    /// <remarks> This Method Is Used To If The User Session Is Alive. </remarks>
    public void Heartbeat() => this.UserCall<object>("heartbeat");

    /// <summary> Calls The Server With The User's Session. </summary>
    /// <remarks> This Method Is Used To Call The Server With The User's Session. </remarks>
    /// <returns> Returns The Server Response. </returns>
    public T UserCall<T>(string Path) => (T)OnClientCallRequested?.Invoke(Path, this.SessionKey, new Dictionary<string, string> { { "session", this.Session } });

    internal event Func<string, string, Dictionary<string, string>, object> OnClientCallRequested;
}
