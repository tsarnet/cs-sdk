using System.Text.Json.Serialization;

namespace Tsar;

public class Data
{
    [JsonPropertyName("subscription")]
    public Subscription Subscription { get; set; }

    [JsonPropertyName("timestamp")]
    public double Timestamp { get; set; }

    [JsonPropertyName("hwid")]
    public string HardwareId { get; set; }

    [JsonPropertyName("session")]
    public string Session { get; set; }
}

public class Subscription
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("expires")]
    public object Expires { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }

    [JsonPropertyName("tier")]
    public int Tier { get; set; }
}

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}

public class ValidateData
{
    [JsonPropertyName("hwid")]
    public string HardwareId { get; set; }

    [JsonPropertyName("timestamp")]
    public double Timestamp { get; set; }
}

public enum Status
{
    Success,

    ApplicationNotFound,
    UserNotFound,

    RequestFailed,
    ServerError,
    Unauthorized,
    ValidateError,

    FailedToParseBody,
    FailedToGetData,
    FailedToGetSignature,
    FailedToDecodeData,
    FailedToDecodeSignature,
    FailedToDecodePubKey,
    FailedToParseData,
    FailedToGetTimestamp,
    FailedToParseTimestamp,
    FailedToBuildKey,
    FailedToBuildSignature,

    OldResponse,

    InvalidSignature,

    FailedToOpenBrowser,
    FailedToGetHardwareId,
}