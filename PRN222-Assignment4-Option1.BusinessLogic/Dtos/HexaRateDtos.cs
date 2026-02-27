using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PRN222_Assignment4_Option1.BusinessLogic.Dtos;

public sealed class HexaRateResponse
{
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("data")]
    public HexaRateData? Data { get; set; }
}

public sealed class HexaRateData
{
    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("mid")]
    public decimal Mid { get; set; }

    [JsonPropertyName("unit")]
    public int Unit { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}


public sealed class FrankfurterLatestResponse
{
    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = new();
}

public sealed class FrankfurterHistoricalResponse
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = new();
}


