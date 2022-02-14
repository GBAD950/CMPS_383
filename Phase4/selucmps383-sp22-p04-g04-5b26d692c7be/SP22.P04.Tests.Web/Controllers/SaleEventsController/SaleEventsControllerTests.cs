using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP22.P04.Tests.Web.Controllers.AuthenticationController;
using SP22.P04.Tests.Web.Controllers.ProductsController;
using SP22.P04.Tests.Web.Dtos;
using SP22.P04.Tests.Web.Helpers;

namespace SP22.P04.Tests.Web.Controllers.SaleEventsController;

[TestClass]
public class SaleEventsControllerTests
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
    public async Task CreateSaleEvents_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }

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
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
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
        var httpResponse = await webClient.GetAsync("/api/sale-events/active");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/sale-events/active");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<SaleEventDto>();
        resultDto.Should().NotBeNull("we expect an active sale for testing");
        (resultDto!.EndUtc - resultDto.StartUtc).Should().BeGreaterOrEqualTo(TimeSpan.FromHours(23.9), "the seeded active sale should be 1 day in length");
    }

    [TestMethod]
    public async Task CreateSaleEvents_TimeConflicts_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var resultDto = await webClient.GetActiveSaleEvent();
        Assert.IsNotNull(resultDto, "You are not ready for this test - you need an active sale");
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
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var productDto = await webClient.CreateProduct(new ProductDto
        {
            Description = "newly added product",
            Name = "newly added product",
            Price = 10
        });
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        var activeSaleEvent = await webClient.GetActiveSaleEvent();
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
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var productDto = await webClient.CreateProduct(new ProductDto
        {
            Description = "newly added product",
            Name = "newly added product",
            Price = 10
        });
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        var activeSaleEvent = await webClient.GetActiveSaleEvent();
        if (activeSaleEvent == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/sale-events/{activeSaleEvent.Id + 1000}/add-product/{productDto.Id}", new SaleEventProductDto
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
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }

        var productDto = await webClient.CreateProduct(new ProductDto
        {
            Description = "newly added product",
            Name = "newly added product",
            Price = 10
        });
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        var activeSaleEvent = await webClient.GetActiveSaleEvent();
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
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var productDto = await webClient.CreateProduct(new ProductDto
        {
            Description = "newly added product",
            Name = "newly added product",
            Price = 10
        });
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        var activeSaleEvent = await webClient.GetActiveSaleEvent();
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
        var sales = await salesResponse.AssertProductListSalesFunctions();

        productDto.SaleEndUtc = activeSaleEvent.EndUtc;
        productDto.SalePrice = 1.23m;
        sales.Should().ContainEquivalentOf(productDto, "we expected adding a product to a sales event marks that item as 'on sale' according to the list product sales endpoint");
    }

    [TestMethod]
    public async Task CreateSaleEvents_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var request = new SaleEventDto
        {
            Name = "name",
            EndUtc = DateTimeOffset.UtcNow.AddDays(2).AddHours(1),
            StartUtc = DateTimeOffset.UtcNow.AddDays(2)
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/sale-events", request);

        //assert
        await httpResponse.AssertSaleEventCreateFunctions(request, webClient);
    }
}