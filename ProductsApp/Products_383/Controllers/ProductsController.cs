using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Products_383.Data;
using Products_383.Data.Models;
using Products_383.Features;
using Products_383.Features.Products;
using System.Linq.Expressions;

namespace Products_383.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext db;
        private readonly string url = "http://localhost";

        public ProductsController(DataContext db)
        {
            this.db = db;
        }

        public static Expression<Func<Product, ProductDto>> MapperMethod()
        {
            return product => new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price

            };
        }

        [HttpGet]
        public IEnumerable<ProductDto> GetAllProducts()
        {
            return db.Set<Product>().Select(MapperMethod()).ToList();
        }

        [HttpGet]
        [Route("sales")]
        public IEnumerable<ProductDto> GetAllSaleProducts()
        {
            // Not Quite Working yet


            //var saleList = ( from Saleproducts in db.SaleEventsProduct
            //                 join products in db.Products on Saleproducts.ProductId equals products.Id
            //                 join Saleevents in db.SaleEvents on Saleproducts.SaleEventID equals Saleevents.Id
            //                 where Saleevents.StartUtc  >= DateTime.UtcNow
            //                 select new
            //                 {
            //                     Name = products.Name,
            //                     Description = products.Description,
            //                     SalePrice = Saleproducts.SaleEventPrice,
            //                 }


            //    ).ToList();

            return db.Set<Product>().Select(MapperMethod()).ToList();
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<ProductDto> GetProduct(int id)
        {
            var item = db.Set<Product>().Where(x => x.Id == id).Select(MapperMethod()).FirstOrDefault();
            if (item == null)
            {
                return NotFound(id);
            }
            else
            {
                return Ok(item);
            }
        }

        [HttpPost]
        public ActionResult<ProductDto> CreateProduct(ProductDto product)
        {
            var item = db.Set<Product>().Add(new Product()
            {

                Name = product.Name,
                Description = product.Description,
                Price = product.Price

            });
            db.SaveChanges();
            product.Id = item.Entity.Id;

            return Created($"{url}/api/products/{item.Entity.Id}", product);

        }

        [HttpPut("{id}")]
        public ActionResult<ProductDto> Update(int id, ProductDto product)
        {
            var item = db.Set<Product>().Where(x => x.Id == id).FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }
            item.Name = product.Name;
            item.Description = product.Description;
            item.Price = product.Price;
            db.SaveChanges();
            return Ok();

        }

        [HttpDelete]
        public ActionResult<ProductDto> Delete(int id)
        {
            var item = db.Set<Product>().Where(x => x.Id == id).FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            db.Set<Product>().Remove(item);
            db.SaveChanges();

            return Ok();
        }

    }
}
