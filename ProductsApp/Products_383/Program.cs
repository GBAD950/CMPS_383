using Products_383.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

var Products = new List<ProductDto>();

var add = new ProductDto
{
    Id = 1,
    Name = "Something Funny",
    Description = "This is a funny product",
    Price = 5,
    SalePrice = 2,
};
var add2 = new ProductDto
{
    Id = 2,
    Name = "Something Kinda Funny",
    Description = "This is a slightly funny product",
    Price = 15,
    SalePrice = 12,
};
var add3 = new ProductDto
{
    Id = 3,
    Name = "Something LAME",
    Description = "This is a really LAME product, Please dont buy",
    Price = 115,
    SalePrice = 0,
};
Products.Add(add);
Products.Add(add2);
Products.Add(add3);


//CREATE
app.MapPost("/api/products", (ProductDto product) =>
{
    var message = "Please put in a name and description";

    if (CheckNull(product) != false)
    {
        var isVerified = Verification(product);
        message = "Please Enter Name Less than 120 Characters and a Price greater than 0";

        if (isVerified != false)
        {
            product.Id = UniqueId(Products);
            Products.Add(product);
            return Results.CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        }
    }



    return Results.BadRequest(message);
});


// EDIT
app.MapPut("/api/products/{id}", (int id, ProductDto updatedProduct) =>
{

    if (Products.FirstOrDefault(x => x.Id == id) != null)
    {
        var message = "Please put in a name and description";

        if (CheckNull(updatedProduct) != false)
        {
            message = "Please Enter Name Less than 120 Characters and a Price greater than 0";

            if (Verification(updatedProduct) != false)
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

                return Results.Ok(newProduct);

            }
        }
        return Results.BadRequest(message);
    }

    return Results.NotFound();

});


//GET ALL
app.MapGet("/api/products", () =>
{
    return Results.Ok(Products);
});



//GET SALES
app.MapGet("/api/products/sales", () =>
{
    var Sales = new List<ProductDto>();
    foreach (var product in Products)
    {
        if (product.SalePrice != 0)
        {
            Sales.Add(product);
        }
    }
    return Sales;
});


//GET BY ID
app.MapGet("/api/products/{id}", (int id) =>
{

    var item = Products.FirstOrDefault((x) => x.Id == id);
    if (item == null)
    {
        return Results.NotFound(id);
    }
    else
    {
        return Results.Ok(item);
    }

})
    .WithName("GetProduct");


// DELETE
app.MapDelete("/api/products/{id}", (int id) =>
{
    //var item = Products.FirstOrDefault((x) => x.Id == id);
    //    if (item != null)
    //    {
    //        Products.Remove(item);
    //        return Results.Ok(id);
    //    }
    //    else return Results.NotFound(id);

    if (Products.FirstOrDefault(x => x.Id == id) is null)
    {
        return Results.NotFound();
    }
    else
        Products.RemoveAll(x => x.Id == id);
    return Results.Ok("Product Deleted..... Hopefully");
});


app.Run();


static int UniqueId(List<ProductDto> products)
{
    var id = 1;
    foreach (var product in products)
    {
        if (products.Count == 0)
        {
            return id;
        }
        else
        {
            id++;
        }
    }

    return id;
}

static bool Verification(ProductDto product)
{
    var isVerified = false;

    if (product.Name.Length > 120 || product.Name == "")
    {
        return isVerified;
    }
    else if (product.Price <= 0 || product.SalePrice < 0)
    {
        return isVerified;
    }
    else
    {
        isVerified = true;
    }

    return isVerified;
}

static bool CheckNull(ProductDto product)
{
    var isNulled = false;

    if (product.Name == null || product.Description == null)
    {
        return isNulled;
    }
    else
    {
        isNulled = true;
    }

    return isNulled;
}

public partial class Program { }