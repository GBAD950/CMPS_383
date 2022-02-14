using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP22.P04.Tests.Web.Controllers.AuthenticationController;
using SP22.P04.Tests.Web.Dtos;
using SP22.P04.Tests.Web.Helpers;

namespace SP22.P04.Tests.Web.Controllers.ProductsController;

[TestClass]
public class ProductsControllerTests
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
    public async Task ListAllProducts_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/products");

        //assert
        await httpResponse.AssertProductListAllFunctions();
    }

    [TestMethod]
    public async Task ListAllProductsSales_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/products/sales");

        //assert
        await httpResponse.AssertProductListSalesFunctions();
    }

    [TestMethod]
    public async Task GetProductById_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetAProduct();
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
    public async Task GetProductById_NoSuchId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/products/999999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/products/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateProduct_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
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
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
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
    public async Task CreateProduct_NoDescription_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
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
    public async Task CreateProduct_NoPrice_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
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
    public async Task CreateProduct_NegativePrice_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
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
    public async Task CreateProduct_NotLoggedIn_Returns401()
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
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling POST /api/products while not logged in");
    }

    [TestMethod]
    public async Task CreateProduct_LoginAsUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsBobAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - user login should work first");
        }
        var request = new ProductDto
        {
            Name = "a",
            Description = "asd",
            Price = 1
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling POST /api/products as user");
    }

    [TestMethod]
    public async Task CreateProduct_Returns201()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var request = new ProductDto
        {
            Name = "a",
            Description = "asd",
            Price = 1
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/products", request);

        //assert
        await httpResponse.AssertProductCreateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateProduct_InvalidId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var target = await webClient.GetAProduct();
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
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var target = await webClient.GetAProduct();
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
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var target = await webClient.GetAProduct();
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
    public async Task UpdateProduct_NoDescription_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var target = await webClient.GetAProduct();
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
    public async Task UpdateProduct_NoPrice_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var target = await webClient.GetAProduct();
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
    public async Task UpdateProduct_NegativePrice_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var target = await webClient.GetAProduct();
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
    public async Task UpdateProduct_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetAProduct();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling PUT /api/products while not logged in");
    }

    [TestMethod]
    public async Task UpdateProduct_LoginAsUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsBobAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - user login should work first");
        }
        var target = await webClient.GetAProduct();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling PUT /api/products as non admin user");
    }

    [TestMethod]
    public async Task UpdateProduct_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var admin = await webClient.LoginAsAdminAsync();
        if (admin == null)
        {
            Assert.Fail("You are not ready for this test - admin login should work first");
        }
        var target = await webClient.GetAProduct();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        target.Name = Guid.NewGuid().ToString("N");

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/products/{target.Id}", target);

        //assert
        await httpResponse.AssertProductUpdateFunctions(target, webClient);
    }

    [TestMethod]
    public async Task DeleteProduct_NoSuchItem_Returns401()
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
            Description = "asd",
            Name = "asd",
            Price = 1
        });
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        //act
        var httpResponse = await webClient.DeleteAsync($"/api/products/{productDto.Id + 21}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/products/{id} with an invalid Id");
    }

    [TestMethod]
    public async Task DeleteProduct_ValidItem_Returns200()
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
            Description = "asd",
            Name = "asd",
            Price = 1
        });
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        //act
        var httpResponse = await webClient.DeleteAsync($"/api/products/{productDto.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/products/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteProduct_SameItemTwice_Returns401()
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
            Description = "asd",
            Name = "asd",
            Price = 1
        });
        if (productDto == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        //act
        await webClient.DeleteAsync($"/api/products/{productDto.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/products/{productDto.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/products/{id} on the same item twice");
    }
}