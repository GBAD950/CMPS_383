using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SP22.P04.Tests.Web.Dtos;
using SP22.P04.Tests.Web.Helpers;

namespace SP22.P04.Tests.Web.Controllers.ProductsController;

internal static class HttpClientExtensions
{
    public static async Task<ProductDto?> CreateProduct(this HttpClient webClient, ProductDto request)
    {
        try
        {
            var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);
            var resultDto = await httpResponse.AssertProductCreateFunctions(request, webClient);

            return resultDto;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<ProductDto?> GetAProduct(this HttpClient webClient)
    {
        try
        {
            var getAllRequest = await webClient.GetAsync("/api/products");
            var getAllResult = await getAllRequest.AssertProductListAllFunctions();
            return getAllResult.OrderByDescending(x => x.Id).First();
        }
        catch (Exception)
        {
            return null;
        }
    }
}