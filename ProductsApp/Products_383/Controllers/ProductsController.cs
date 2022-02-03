using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Products_383.Features;

namespace Products_383.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public static List<ProductDto> Products = new List<ProductDto>()
        {
                new ProductDto
                {
                    Id = 1,
                    Name = "Something Funny",
                    Description = "This is a funny product",
                    Price = 5,
                    SalePrice = 2,
                },
                new ProductDto
                {
                    Id = 2,
                    Name = "Something Kinda Funny",
                    Description = "This is a slightly funny product",
                    Price = 15,
                    SalePrice = 12,
                },
                new ProductDto
                {
                    Id = 3,
                    Name = "Something LAME",
                    Description = "This is a really LAME product, Please dont buy",
                    Price = 115,
                    SalePrice = 0,
                }

        };

        [HttpGet]
        public ActionResult<ProductDto[]> GetAllProducts()
        {
            return Ok(Products.ToArray());
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<ProductDto> GetProduct(int id)
        {
            var item = Products.FirstOrDefault((x) => x.Id == id);
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
        public ActionResult CreateProduct(ProductDto product)
        {
            var message = "Please put in a name and description";

            if (ProductCheck.CheckNull(product) != false)
            {
                var isVerified = ProductCheck.Verification(product);
                message = "Please Enter Name Less than 120 Characters and a Price greater than 0";

                if (isVerified != false)
                {
                    product.Id = ProductCheck.UniqueId(Products);
                    Products.Add(product);
                    return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
                }
            }



            return BadRequest(message);

        }

        [HttpPut]
        [Route("{id}")]
        public ActionResult UpdateProduct(int id, ProductDto updatedProduct)
        {

            if (Products.FirstOrDefault(x => x.Id == id) != null)
            {
                var message = "Please put in a name and description";

                if (ProductCheck.CheckNull(updatedProduct) != false)
                {
                    message = "Please Enter Name Less than 120 Characters and a Price greater than 0";

                    if (ProductCheck.Verification(updatedProduct) != false)
                    {
                        foreach (ProductDto product in Products)
                        {
                            if (product.Id == id)
                            {
                                product.Id = id;
                                product.Name = updatedProduct.Name;
                                product.Description = updatedProduct.Description;
                                product.Price = updatedProduct.Price;
                                product.SalePrice = updatedProduct.SalePrice;


                            }
                        }
                        var newProduct = Products.FirstOrDefault(x => x.Id == id);

                        return Ok(newProduct);

                    }
                }
                return BadRequest(message);
            }

            return NotFound();
        }

    }
}
