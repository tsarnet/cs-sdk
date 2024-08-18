using System.Text.Json.Serialization;

namespace Tsar;

public class Init
{
    [JsonPropertyName("dashboard_hostname")]
    public string HostName { get; set; }
}

public class Data<T>
{
    [JsonPropertyName("data")]
    public DataObject<T> DataObject { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }
}

public class DataObject<T>
{
    [JsonPropertyName("data")]
    public T Object { get; set; }

    [JsonPropertyName("timestamp")]
    public int Timestamp { get; set; }

    [JsonPropertyName("hwid")]
    public string HardwareId { get; set; }
}