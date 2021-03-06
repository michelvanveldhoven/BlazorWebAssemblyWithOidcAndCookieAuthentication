using BlazorWasmCookieAuth.Shared;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BlazorWasmCookieAuth.Client.Services
{
    public class FetchWeatherForecastService
    {
        private readonly HttpClient _publicApiClient;
        private readonly HttpClient _protectedApiClient;

        public FetchWeatherForecastService(IHttpClientFactory clientFactory, NavigationManager navigationManager)
        {
            _publicApiClient = clientFactory.CreateClient();
            _protectedApiClient = clientFactory.CreateClient("authorizedClient");
            _publicApiClient.BaseAddress = _protectedApiClient.BaseAddress = new Uri(navigationManager.BaseUri);
        }

        public async Task<WeatherForecast[]> GetPublicWeatherForeacast()
        {
            return await _publicApiClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
        }

        public async Task<WeatherForecast[]> GetProtectedWeatherForeacast()
        {
            return await _protectedApiClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast/protected");
        }
    }
}
