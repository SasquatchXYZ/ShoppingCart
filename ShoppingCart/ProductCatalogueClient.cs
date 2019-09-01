using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using ShoppingCart;

namespace ShoppingCart
{
    public class ProductCatalogueClient : IProductCatalogueClient
    {
        private static AsyncRetryPolicy exponentialRetryPolicy =
            Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3, attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt))
                );

        private static string productCatalogBaseUrl =
            @"http://private-05cc8-chapter2productcatalogmicroservice.apiary-mock.com";

        private static string getProductPathTemplate = "/products?productIds=[{0}]";

        public async Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productCatalogIds) =>
            exponentialRetryPolicy.ExecuteAsync(async () =>
                await GetItemsFromCatalogueService(productCatalogIds)
                    .ConfigureAwait(false));

        private static async Task<IEnumerable<ShoppingCartItem>> GetItemsFromCatalogueService(int[] productCatalogueIds)
        {
            var response = await RequestProductFromProductCatalogue(productCatalogueIds).ConfigureAwait(false);
            return await ConvertToShoppingCartItems(response).ConfigureAwait(false);
        }

        private static async Task<HttpResponseMessage> RequestProductFromProductCatalogue(int[] productCatalogueIds)
        {
            var productsResource = string.Format(getProductPathTemplate, string.Join(",", productCatalogueIds));
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(productCatalogBaseUrl);
                return await httpClient.GetAsync(productsResource).ConfigureAwait(false);
            }
        }

        private static async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(
            HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var products =
                JsonConvert.DeserializeObject<List<ProductCatalogueProduct>>(await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false));
            return products
                .Select(p => new ShoppingCartItem(
                    int.Parse(p.ProductId),
                    p.ProductName,
                    p.ProductDescription,
                    p.Price
                ));
        }

        private class ProductCatalogProduct
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public Money Price { get; set; }
        }
    }
}