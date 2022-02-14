using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP22.P04.Tests.Web.Dtos;
using SP22.P04.Tests.Web.Helpers;

namespace SP22.P04.Tests.Web.Controllers.SaleEventsController;

internal static class HttpResponseMessageAssertExtensions
{
    public static async Task AssertSaleEventCreateFunctions(this HttpResponseMessage httpResponse, SaleEventDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created, "we expect an HTTP 201 when calling POST /api/sale-events with valid data to create a new sale event");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<SaleEventDto>();
        resultDto.Should().NotBeNull("we expect calling POST /api/sale-events with valid to return a SaleEventDto");
        Assert.IsNotNull(resultDto);
        resultDto.Id.Should().BeGreaterOrEqualTo(1, "we expect a newly created sale event to return with a positive Id");
        resultDto.Should().BeEquivalentTo(request, x => x.Excluding(y => y.Id), "We expect the create sale event endpoint to return the result");

        httpResponse.Headers.Location.Should().NotBeNull("we expect the 'location' header to be set as part of a HTTP 201");
        httpResponse.Headers.Location.Should().Be($"http://localhost/api/sale-events/{resultDto.Id}", "we expect the location header to point to the get sale event by id endpoint");

        var getByIdResult = await webClient.GetAsync($"/api/sale-events/{resultDto.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the newly created sale event by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<SaleEventDto>();
        dtoById.Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a create sale event as what you'd get from get sale event by id");
    }
}