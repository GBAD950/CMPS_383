using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using SP22.P04.Tests.Web.Helpers;
using SP22.P04.Web.Features.Sales;

namespace SP22.P04.Tests.Web.Controllers.SaleEventsController;

internal static class HttpClientExtensions
{
    public static async Task<SaleEventDto?> GetActiveSaleEvent(this HttpClient webClient)
    {
        var activeSale = await webClient.GetAsync("/api/sale-events/active");
        var resultDto = await activeSale.Content.ReadAsJsonAsync<SaleEventDto>();
        if (resultDto == null)
        {
            return null;
        }

        (resultDto.EndUtc - resultDto.StartUtc).Should().BeGreaterOrEqualTo(TimeSpan.FromHours(23.9), "the seeded active sale should be 1 day in length");
        return resultDto;
    }
}