using System.Text.Json.Serialization;

namespace ComputeNexus.Domain.Dtos.VastApi;

public class VastOffersResponse
{
    [JsonPropertyName("offers")]
    public List<VastOffer> Offers { get; set; } = new();
}