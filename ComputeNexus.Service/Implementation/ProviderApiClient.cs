using System.Net.Http.Json;
using ComputeNexus.Domain.Configuration;
using ComputeNexus.Domain.Dtos;
using ComputeNexus.Domain.Dtos.VastApi;
using ComputeNexus.Service.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ProviderApiClient : IProviderApiClient
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ProviderApiSettings _settings;
    private readonly ILogger<ProviderApiClient> _logger;

    public ProviderApiClient(HttpClient http, IMemoryCache cache,
        IOptions<ProviderApiSettings> settings, ILogger<ProviderApiClient> logger)
    {
        _http = http;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }
    
    public async Task<List<ProviderHardwareDto>> GetAvailableHardwareAsync()
    {
        var cacheKey = "provider_hardware";
        if (_cache.TryGetValue(cacheKey, out List<ProviderHardwareDto>? cached))
            return cached ?? new List<ProviderHardwareDto>();

        try
        {
            var requestBody = new
            {
                limit = 500, // max 500 offers
                type = "on-demand",
                verified = new { eq = true },
                rentable = new { eq = true },
                rented = new { eq = false }
            };

            var response = await _http.PostAsJsonAsync(string.Empty, requestBody);
            response.EnsureSuccessStatusCode();

            var vastResponse = await response.Content.ReadFromJsonAsync<VastOffersResponse>();
            if (vastResponse?.Offers == null || vastResponse.Offers.Count == 0)
            {
                _logger.LogWarning("No offers returned from Vast.ai API");
                return new List<ProviderHardwareDto>();
            }

            var hardwareList = new List<ProviderHardwareDto>();
            foreach (var offer in vastResponse.Offers)
            {
                var dto = new ProviderHardwareDto
                {
                    ProviderId = $"vast-{offer.Id}",
                    GpuModel = offer.GpuName,
                    AvailableGpuCount = offer.NumGpus,
                    PricePerGpuHour = offer.DphTotal,
                    Region = offer.Geolocation,
                    LastUpdated = DateTime.UtcNow,
                    CpuCores = offer.CpuCores,
                    CpuMemoryGb = offer.CpuRamMb / 1024, // MB to GB
                    StorageGb = (int)offer.DiskSpaceGb,
                    GpuRamGb = (int)Math.Ceiling((offer.GpuRamMb) / 1024.0) // MB to GB
                };
                hardwareList.Add(dto);
            }
            
            _cache.Set(cacheKey, hardwareList, TimeSpan.FromMinutes(_settings.CacheExpirationMinutes));
            return hardwareList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch hardware from Vast.ai API");
            return new List<ProviderHardwareDto>();
        }
    }
}