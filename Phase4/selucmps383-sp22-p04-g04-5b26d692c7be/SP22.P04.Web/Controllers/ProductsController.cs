using Microsoft.AspNetCore.Mvc;
using SP22.P04.Web.Data;
using SP22.P04.Web.Features.Products;

namespace SP22.P04.Web.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly DataContext dataContext;

    public ProductsController(DataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    [HttpGet]
    public ProductDto[] GetAllProducts()
    {
        var products = dataContext.Set<Product>();
        return GetProductDtos(products).ToArray();
    }

    [HttpGet]
    [Route("{id}")]
    public ActionResult<ProductDto> GetProductById(int id)
    {
        var products = dataContext.Set<Product>();
        var result = GetProductDtos(products).FirstOrDefault(x => x.Id == id);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    [Route("sales")]
    public ProductDto[] GetProductsOnSale()
    {
        var products = dataContext.Set<Product>();
        return GetProductDtos(products).Where(x => x.SalePrice != null).ToArray();
    }

    [HttpPost]
    public ActionResult<ProductDto> CreateProduct(ProductDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price
        };

        dataContext.Add(product);
        dataContext.SaveChanges();
        productDto.Id = product.Id;

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, productDto);
    }

    [HttpPut("{id}")]
    public ActionResult<ProductDto> UpdateProduct(int id, ProductDto productDto)
    {
        var products = dataContext.Set<Product>();
        var current = products.FirstOrDefault(x => x.Id == id);
        if (current == null)
        {
            return NotFound();
        }

        current.Name = productDto.Name;
        current.Price = productDto.Price;
        current.Description = productDto.Description;
        dataContext.SaveChanges();

        return Ok(productDto);
    }

    [HttpDelete("{id}")]
    public ActionResult<ProductDto> DeleteProduct(int id)
    {
        var products = dataContext.Set<Product>();
        var current = products.FirstOrDefault(x => x.Id == id);
        if (current == null)
        {
            return NotFound();
        }

        products.Remove(current);
        dataContext.SaveChanges();

        return Ok();
    }

    private static IQueryable<ProductDto> GetProductDtos(IQueryable<Product> products)
    {
        var now = DateTimeOffset.UtcNow;
        return products
            .Select(x => new
            {
                Product = x,
                CurrentSale = x.SaleEventProducts.FirstOrDefault(y => y.SaleEvent!.StartUtc <= now && now <= y.SaleEvent.EndUtc)
            })
            .Select(x => new ProductDto
            {
                Id = x.Product.Id,
                Name = x.Product.Name,
                Description = x.Product.Description,
                Price = x.Product.Price,
                SalePrice = x.CurrentSale == null ? null : x.CurrentSale.SaleEventPrice,
                SaleEndUtc = x.CurrentSale == null ? null : x.CurrentSale.SaleEvent!.EndUtc,
            });
    }
}