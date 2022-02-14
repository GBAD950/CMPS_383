using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP22.P04.Tests.Web.Dtos;
using SP22.P04.Tests.Web.Helpers;

namespace SP22.P04.Tests.Web.Controllers.ProductsController;

internal static class HttpResponseMessageAssertExtensions
{
    public static async Task<List<ProductDto>> AssertProductListAllFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when getting calling GET /api/products");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<List<ProductDto>>();
        resultDto.Should().HaveCountGreaterThan(2, "we expect at least 3 records");
        Assert.IsNotNull(resultDto);
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Name)).Should().BeTrue("we expect all products to have names");
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Description)).Should().BeTrue("we expect all products to have descriptions");
        resultDto.All(x => x.Price > 0).Should().BeTrue("we expect all products to have non zero/non negative prices");
        resultDto.All(x => x.Id > 0).Should().BeTrue("we expect all products to have an id");
        var ids = resultDto.Select(x => x.Id).ToArray();
        ids.Should().HaveSameCount(ids.Distinct(), "we expect Id values to be unique for every product");
        return resultDto;
    }

    public static async Task<List<ProductDto>> AssertProductListSalesFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when getting calling GET /api/products/sales");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<List<ProductDto>>();
        resultDto.Should().HaveCountGreaterThan(0, "we expect at least 1 record on sale");
        Assert.IsNotNull(resultDto);
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Name)).Should().BeTrue("we expect all products to have names");
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Description)).Should().BeTrue("we expect all products to have descriptions");
        resultDto.All(x => x.Price > 0).Should().BeTrue("we expect all products to have non zero/non negative prices");
        resultDto.All(x => x.SalePrice != null).Should().BeTrue("we expect all products on sale to have a non-null sale price");
        resultDto.All(x => x.SalePrice >= 0).Should().BeTrue("we expect all products on sale to have a sale price greater than or equal to zero");
        resultDto.All(x => x.SaleEndUtc != null).Should().BeTrue("we expect all products on sale to have a 'SaleEndUtc' to not be null");
        resultDto.All(x => x.SaleEndUtc >= DateTimeOffset.UtcNow).Should().BeTrue("we expect all products on sale to have a 'SaleEndUtc' to be on sale at the moment. e.g. SaleEndUtc > now");
        resultDto.All(x => x.Id > 0).Should().BeTrue("we expect all products to have an id");
        var ids = resultDto.Select(x => x.Id).ToArray();
        ids.Should().HaveSameCount(ids.Distinct(), "we expect Id values to be unique for every product");

        return resultDto;
    }

    public static async Task<ProductDto> AssertProductCreateFunctions(this HttpResponseMessage httpResponse, ProductDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created, "we expect an HTTP 201 when calling POST /api/products with valid data to create a new product");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<ProductDto>();
        resultDto.Should().NotBeNull("we expect calling POST /api/products with valid data returns a ProductDto");
        Assert.IsNotNull(resultDto);
        resultDto.Id.Should().BeGreaterOrEqualTo(1, "we expect a newly created product to return with a positive Id");
        resultDto.Should().BeEquivalentTo(request, x => x.Excluding(y => y.Id), "We expect the create product endpoint to return the result");

        httpResponse.Headers.Location.Should().NotBeNull("we expect the 'location' header to be set as part of a HTTP 201");
        httpResponse.Headers.Location.Should().Be($"http://localhost/api/products/{resultDto.Id}", "we expect the location header to point to the get product by id endpoint");

        var getByIdResult = await webClient.GetAsync($"/api/products/{resultDto.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the newly created product by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<ProductDto>();
        dtoById.Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a create product as what you'd get from get product by id");

        var getAllRequest = await webClient.GetAsync("/api/products");
        await AssertProductListAllFunctions(getAllRequest);

        var listAllData = await getAllRequest.Content.ReadAsJsonAsync<List<ProductDto>>();
        listAllData.Should().NotBeEmpty("list all should have something if we just created a product");
        Assert.IsNotNull(listAllData);
        var matchingItem = listAllData.Where(x => x.Id == resultDto.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created product by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a created product as what you'd get from get getting all products");

        return resultDto;
    }

    public static async Task AssertProductUpdateFunctions(this HttpResponseMessage httpResponse, ProductDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling PUT /api/products/{id} with valid data to update a product");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<ProductDto>();
        resultDto.Should().BeEquivalentTo(request, "We expect the update product endpoint to return the result");

        var getByIdResult = await webClient.GetAsync($"/api/products/{request.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the updated product by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<ProductDto>();
        dtoById.Should().BeEquivalentTo(request, "we expect the same result to be returned by a update product as what you'd get from get product by id");

        var getAllRequest = await webClient.GetAsync("/api/products");
        await AssertProductListAllFunctions(getAllRequest);

        var listAllData = await getAllRequest.Content.ReadAsJsonAsync<List<ProductDto>>();
        listAllData.Should().NotBeEmpty("list all should have something if we just updated a product");
        Assert.IsNotNull(listAllData);
        var matchingItem = listAllData.Where(x => x.Id == request.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created product by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(request, "we expect the same result to be returned by a updated product as what you'd get from get getting all products");
    }
}