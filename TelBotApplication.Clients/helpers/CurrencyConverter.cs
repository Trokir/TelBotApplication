using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TelBotApplication.Domain.Helpers;

namespace TelBotApplication.Clients.helpers
{
    public class CurrencyConverter : ICurrencyConverter
    {
        private const string rubUrl = @"http://www.floatrates.com/daily/rub.json";
        private const string usdUrl = @"http://www.floatrates.com/daily/usd.json";
        private const string euroUrl = @"http://www.floatrates.com/daily/eur.json";
        private const string brlUrl = @"http://www.floatrates.com/daily/brl.json";
        private readonly IServiceProvider _factory;
        private IMemoryCache _cache;
        public CurrencyConverter(IServiceProvider factory)
        {
            _factory = factory;

        }


        public async Task<decimal> GetCurrencyFromRubles(float amount)
        {
            using var scope = _factory.CreateScope();
            _cache = scope.ServiceProvider
                     .GetService<IMemoryCache>();

            if (GetCache("CurrencyModelRub") is CurrencyModel model)
            {
                return Convert.ToDecimal(model.usd.rate * amount);
            }
            else
            {
                var httpClient =
                   scope.ServiceProvider
                       .GetRequiredService<IHttpClientFactory>();
                var client = httpClient.CreateClient();
                var responce = await client.GetAsync(rubUrl);
                var str = await responce.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CurrencyModel>(str);

                SetCache("CurrencyModelRub", result);

                return Convert.ToDecimal(result.usd.rate * amount);
            }
        }



        public async Task<decimal> GetCurrencyFromUSD(float amount)
        {
            using var scope = _factory.CreateScope();
            _cache = scope.ServiceProvider
                     .GetRequiredService<IMemoryCache>();

            if (GetCache("CurrencyModelUsd") is CurrencyModel model)
            {
                return Convert.ToDecimal(model.rub.rate * amount);
            }
            else
            {
                var httpClient =
                                 scope.ServiceProvider
                                     .GetRequiredService<IHttpClientFactory>();
                var client = httpClient.CreateClient();
                var responce = await client.GetAsync(usdUrl);
                var str = await responce.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CurrencyModel>(str);
                SetCache("CurrencyModelUsd", result);
                return Convert.ToDecimal(result.rub.rate * amount);
            }

        }
        public async Task<decimal> GetCurrencyFromEUR(float amount)
        {
            using var scope = _factory.CreateScope();
            _cache = scope.ServiceProvider
                     .GetRequiredService<IMemoryCache>();
            if (GetCache("CurrencyModelEur") is CurrencyModel model)
            {
                return Convert.ToDecimal(model.rub.rate * amount);
            }
            else
            {
                var httpClient =
                  scope.ServiceProvider
                      .GetRequiredService<IHttpClientFactory>();
                var client = httpClient.CreateClient();
                var responce = await client.GetAsync(euroUrl);
                var str = await responce.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CurrencyModel>(str);
                SetCache("CurrencyModelEur", result);
                return Convert.ToDecimal(result.rub.rate * amount);
            }

        }

        public async Task<decimal> GetCurrencyFromReal(float amount)
        {
            using var scope = _factory.CreateScope();
            _cache = scope.ServiceProvider
                     .GetRequiredService<IMemoryCache>();
            if (GetCache("CurrencyModelReal") is CurrencyModel model)
            {
                return Convert.ToDecimal(model.rub.rate * amount);
            }
            else
            {
                var httpClient =
                  scope.ServiceProvider
                      .GetRequiredService<IHttpClientFactory>();
                var client = httpClient.CreateClient();
                var responce = await client.GetAsync(brlUrl);
                var str = await responce.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CurrencyModel>(str);
                SetCache("CurrencyModelReal", result);
                return Convert.ToDecimal(result.rub.rate * amount);
            }

        }

        private CurrencyModel GetCache(string cacheName)
        {

            if (_cache.TryGetValue(cacheName, out CurrencyModel model))
            {
                return model;
            }
            else
                return default;
        }

        private void SetCache(string cacheName, CurrencyModel result)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
           .SetSlidingExpiration(TimeSpan.FromHours(3));
            _cache.Set(cacheName, result, cacheEntryOptions);
        }
    }
}
