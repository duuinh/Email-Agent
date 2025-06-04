using System.Text.Json.Serialization;

public sealed class Email
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}