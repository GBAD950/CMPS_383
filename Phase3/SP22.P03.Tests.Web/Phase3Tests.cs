using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP22.P03.Tests.Web.Helpers;

namespace SP22.P03.Tests.Web;

[TestClass]
public class Phase3Tests
{
    private WebTestContext context = new();

    [TestInitialize]
    public void Init()
    {
        context = new WebTestContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task ListAllProducts_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/products");

        //assert
        await AssertProductListAllFunctions(httpResponse);
    }

    [TestMethod]
    public async Task ListAllProductsSales_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/products/sales");

        //assert
        await AssertProductListSalesFunctions(httpResponse);
    }

    [TestMethod]
    public async Task GetProductById_Returns200AndDatawebClient()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await GetItem(webClient);
        if (target == null)
        {
            Assert.Fail("Make List All products work first");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync($"/api/products/{target.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/products/{id} ");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<ProductDto>();
        resultDto.Should().BeEquivalentTo(target, "we expect get product by id to return the same data as the list all product endpoint");
    }

    [TestMethod]
    public async Task GetProductById_NoSuchId_Returns404webClient()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/products/999999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/products/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateSaleEvents_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new SaleEventDto
        {
            Name = null,
            EndUtc = DateTimeOffset.UtcNow.AddDays(2).AddHours(1),
            StartUtc = DateTimeOffset.UtcNow.AddDays(2)
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/sale-events", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when getting POST /api/sale-events with no name");
    }

    [TestMethod]
    public async Task CreateSaleEvents_EndBeforeStart_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new SaleEventDto
        {
            Name = "name",
            EndUtc = DateTimeOffset.UtcNow.AddDays(2),
            StartUtc = DateTimeOffset.UtcNow.AddDays(2).AddSeconds(1)
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/sale-events", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/sale-events with no name");
    }

    [TestMethod]
    public async Task GetActiveSale_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await  webClient.GetAsync("/api/sale-events/active");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/sale-events/active");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<SaleEventDto>();
        resultDto.Should().NotBeNull("we expect an active sale for testing");
        (resultDto.EndUtc - resultDto.StartUtc).Should().BeGreaterOrEqualTo(TimeSpan.FromHours(23.9), "the seeded active sale should be 1 day in length");
    }

    [TestMethod]
    public async Task CreateSaleEvents_TimeConflicts_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var resultDto = await GetActiveSaleEvent(webClient);
        var existingStart = resultDto.StartUtc;
        var existingEnd = resultDto.EndUtc;
        var fudge = new Random();
        var offset = TimeSpan.FromMilliseconds(fudge.NextDouble() * 5000 + 5000);
        var invalidScheduleExamples = new List<SaleEventDto>
        {
            new SaleEventDto
            {
                Name = "completely contains existing",
                EndUtc = existingEnd.Add(offset),
                StartUtc = existingStart.Subtract(offset)
            },

            new SaleEventDto
            {
                Name = "completely contained by existing",
                EndUtc = existingEnd.Subtract(offset),
                StartUtc = existingStart.Add(offset)
            },
            new SaleEventDto
            {
                Name = "starts in the middle of existing",
                EndUtc = existingEnd.Add(offset),
                StartUtc = existingStart.Add(offset)
            },
            new SaleEventDto
            {
                Name = "ends in the middle of existing",
                EndUtc = existingEnd.Subtract(offset),
                StartUtc = existingStart.Subtract(offset)
            },
        };

        foreach (var data in invalidScheduleExamples)
        {
            //act
            var httpResponse = await webClient.PostAsJsonAsync("/api/sale-events", data);

            //assert
            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when getting POST /api/sale-events with a conflicting schedule that " + data.Name + " sale event");
        }
    }

    [TestMethod]
    public async Task AddProductToSale_NoSuchSaleEventId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = new ProductDto
        {
            Description = "newly added product",
            Name = "newly added product",
            Price = 10
        };

        using var itemHandle = await CreateProduct(webClient, productDto);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        var activeSaleEvent = await GetActiveSaleEvent(webClient);
        if (activeSaleEvent == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/sale-events/{activeSaleEvent.Id}/add-product/{productDto.Id + 1000}", new SaleEventProductDto
        {
            SaleEventPrice = 1
        });

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling POST /api/sale-events/{saleEventId}/add-product/{productId} with a invalid product id");
    }

    [TestMethod]
    public async Task AddProductToSale_NoSuchProductId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = new ProductDto
        {
            Description = "newly added product",
            Name = "newly added product",
            Price = 10
        };

        using var itemHandle = await CreateProduct(webClient, productDto);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        var activeSaleEvent = await GetActiveSaleEvent(webClient);
        if (activeSaleEvent == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/sale-events/{activeSaleEvent.Id+1000}/add-product/{productDto.Id}", new SaleEventProductDto
        {
            SaleEventPrice = 1
        });

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling POST /api/sale-events/{saleEventId}/add-product/{productId} with a invalid sale event id");
    }

    [TestMethod]
    public async Task AddProductToSale_InvalidPrice_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = new ProductDto
        {
            Description = "newly added product",
            Name = "newly added product",
            Price = 10
        };

        using var itemHandle = await CreateProduct(webClient, productDto);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        var activeSaleEvent = await GetActiveSaleEvent(webClient);
        if (activeSaleEvent == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/sale-events/{activeSaleEvent.Id}/add-product/{productDto.Id}", new SaleEventProductDto
        {
            SaleEventPrice = -1
        });

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/sale-events/{saleEventId}/add-product/{productId} with a negative sale price");
    }

    [TestMethod]
    public async Task AddProductToSale_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var productDto = new ProductDto
        {
            Description = "newly added product",
            Name = "newly added product",
            Price = 10
        };

        using var itemHandle = await CreateProduct(webClient, productDto);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        var activeSaleEvent = await GetActiveSaleEvent(webClient);
        if (activeSaleEvent == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/sale-events/{activeSaleEvent.Id}/add-product/{productDto.Id}", new SaleEventProductDto
        {
            SaleEventPrice = 1.23m
        });

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling POST /api/sale-events/{saleEventId}/add-product/{productId} with a valid SaleEventProductDto");

        var salesResponse = await webClient.GetAsync("/api/products/sales");
        var sales = await AssertProductListSalesFunctions(salesResponse);

        productDto.SaleEndUtc = activeSaleEvent.EndUtc;
        productDto.SalePrice = 1.23m;
        sales.Should().ContainEquivalentOf(productDto, "we expected adding a product to a sales event marks that item as 'on sale' according to the list product sales endpoint");
    }

    [TestMethod]
    public async Task CreateSaleEvents_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new SaleEventDto
        {
            Name = "name",
            EndUtc = DateTimeOffset.UtcNow.AddDays(2).AddHours(1),
            StartUtc = DateTimeOffset.UtcNow.AddDays(2)
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/sale-events", request);

        //assert
        await AssertSaleEventCreateFunctions(httpResponse, request, webClient);
    }

    [TestMethod]
    public async Task CreateProduct_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Price = 1
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when getting POST /api/products with no name");
    }

    [TestMethod]
    public async Task CreateProduct_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a".PadLeft(121, '0'),
            Description = "asd",
            Price = 1
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when getting POST /api/products with no name");
    }

    [TestMethod]
    public async Task CreateProduct_NoDescription_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "asd",
            Price = 1
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/products with no description");
    }

    [TestMethod]
    public async Task CreateProduct_NoPrice_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd"
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/products with no price");
    }

    [TestMethod]
    public async Task CreateProduct_NegativePrice_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd"
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/products with negative price");
    }

    [TestMethod]
    public async Task CreateProduct_Returns201AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Name = "a",
            Description = "asd",
            Price = 1
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        await AssertProductCreateFunctions(httpResponse, request, webClient);
    }

    [TestMethod]
    public async Task UpdateProduct_InvalidId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await GetItem(webClient);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{9999999}", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when getting PUT /api/products/{id} with an invalid id");
    }

    [TestMethod]
    public async Task UpdateProduct_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await GetItem(webClient);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        target.Name = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when getting PUT /api/products/{id} with no name");
    }

    [TestMethod]
    public async Task UpdateProduct_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await GetItem(webClient);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        target.Name = "0".PadRight(121, '0');

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when getting PUT /api/products/{id} with no name");
    }

    [TestMethod]
    public async Task UpdateProduct_NoDescription_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await GetItem(webClient);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        target.Description = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/products/{id} with no description");
    }

    [TestMethod]
    public async Task UpdateProduct_NoPrice_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await GetItem(webClient);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        target.Price = 0;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/products/{id} with no price");
    }

    [TestMethod]
    public async Task UpdateProduct_NegativePrice_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await GetItem(webClient);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        target.Price = -1m;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/products/{id} with negative price");
    }

    [TestMethod]
    public async Task UpdateProduct_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await GetItem(webClient);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        target.Name = Guid.NewGuid().ToString("N");

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        await AssertProductUpdateFunctions(httpResponse, target, webClient);
    }

    [TestMethod]
    public async Task DeleteProduct_NoSuchItem_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd",
            Price = 1
        };
        using var itemHandle = await CreateProduct(webClient, request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        //act
        var httpResponse = await webClient.DeleteAsync($"/api/products/{request.Id + 21}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/products/{id} with an invalid Id");
    }

    [TestMethod]
    public async Task DeleteProduct_ValidItem_ReturnsOk()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd",
            Price = 1
        };
        using var itemHandle = await CreateProduct(webClient, request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        //act
        var httpResponse = await webClient.DeleteAsync($"/api/products/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/products/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteProduct_SameItemTwice_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new ProductDto
        {
            Description = "asd",
            Name = "asd",
            Price = 1
        };
        using var itemHandle = await CreateProduct(webClient, request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        //act
        await webClient.DeleteAsync($"/api/products/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/products/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/products/{id} on the same item twice");
    }

    private async Task<IDisposable?> CreateProduct(HttpClient webClient, ProductDto request)
    {
        try
        {
            var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);
            var resultDto = await AssertProductCreateFunctions(httpResponse, request, webClient);
            request.Id = resultDto.Id;
            return new DeleteProductItem(resultDto, webClient);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<ProductDto?> GetItem(HttpClient webClient)
    {
        try
        {
            var getAllRequest = await webClient.GetAsync("/api/products");
            var getAllResult = await AssertProductListAllFunctions(getAllRequest);
            return getAllResult.OrderByDescending(x => x.Id).First();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<SaleEventDto?> GetActiveSaleEvent(HttpClient webClient)
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

    private static async Task<SaleEventDto> AssertSaleEventCreateFunctions(HttpResponseMessage httpResponse, SaleEventDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created, "we expect an HTTP 201 when calling POST /api/sale-events with valid data to create a new sale event");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<SaleEventDto>();
        resultDto.Id.Should().BeGreaterOrEqualTo(1, "we expect a newly created sale event to return with a positive Id");
        resultDto.Should().BeEquivalentTo(request, x => x.Excluding(y => y.Id), "We expect the create sale event endpoint to return the result");

        httpResponse.Headers.Location.Should().NotBeNull("we expect the 'location' header to be set as part of a HTTP 201");
        httpResponse.Headers.Location.Should().Be($"http://localhost/api/sale-events/{resultDto.Id}", "we expect the location header to point to the get sale event by id endpoint");

        var getByIdResult = await webClient.GetAsync($"/api/sale-events/{resultDto.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the newly created sale event by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<SaleEventDto>();
        dtoById.Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a create sale event as what you'd get from get sale event by id");

        return resultDto;
    }

    private static async Task<List<ProductDto>> AssertProductListAllFunctions(HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when getting calling GET /api/products");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<List<ProductDto>>();
        resultDto.Should().HaveCountGreaterThan(2, "we expect at least 3 records");
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Name)).Should().BeTrue("we expect all products to have names");
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Description)).Should().BeTrue("we expect all products to have descriptions");
        resultDto.All(x => x.Price > 0).Should().BeTrue("we expect all products to have non zero/non negative prices");
        resultDto.All(x => x.Id > 0).Should().BeTrue("we expect all products to have an id");
        var ids = resultDto.Select(x => x.Id).ToArray();
        ids.Should().HaveSameCount(ids.Distinct(), "we expect Id values to be unique for every product");
        return resultDto;
    }

    private static async Task<List<ProductDto>> AssertProductListSalesFunctions(HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when getting calling GET /api/products/sales");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<List<ProductDto>>();
        resultDto.Should().HaveCountGreaterThan(0, "we expect at least 1 record on sale");
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

    private static async Task<ProductDto> AssertProductCreateFunctions(HttpResponseMessage httpResponse, ProductDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created, "we expect an HTTP 201 when calling POST /api/products with valid data to create a new product");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<ProductDto>();
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
        var matchingItem = listAllData.Where(x => x.Id == resultDto.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created product by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a created product as what you'd get from get getting all products");

        return resultDto;
    }

    private async Task AssertProductUpdateFunctions(HttpResponseMessage httpResponse, ProductDto request, HttpClient webClient)
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
        var matchingItem = listAllData.Where(x => x.Id == request.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created product by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(request, "we expect the same result to be returned by a updated product as what you'd get from get getting all products");
    }

    internal class ProductDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public decimal? SalePrice { get; set; }
        public DateTimeOffset? SaleEndUtc { get; set; }
    }

    internal class SaleEventDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTimeOffset StartUtc { get; set; }
        public DateTimeOffset EndUtc { get; set; }
    }

    internal class SaleEventProductDto
    {
        public decimal SaleEventPrice { get; set; }
    }

    internal sealed class DeleteProductItem : IDisposable
    {
        private readonly ProductDto request;
        private readonly HttpClient webClient;

        public DeleteProductItem(ProductDto request, HttpClient webClient)
        {
            this.request = request;
            this.webClient = webClient;
        }

        public void Dispose()
        {
            try
            {
                webClient.DeleteAsync($"/api/products/{request.Id}").Wait();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}