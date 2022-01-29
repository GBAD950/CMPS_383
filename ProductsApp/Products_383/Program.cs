using Products_383.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

var Products = new List<ProductDto>();

//CREATE
app.MapPost("/api/Products/New", (ProductDto product) =>
{
    var message = "Please put in a name and description";

    if (CheckNull(product) != false)
    {
        var isVerified = Verification(product);
        message = "Please Enter Name Less than 120 Characters and a Price greater than 0";

        if (isVerified != false)
        {
            product.ProductId = UniqueId(Products);
            Products.Add(product);
            return Results.Ok(product);
        }
    }



    return Results.Ok(message);
});

//GET ALL
app.MapGet("/api/Products/List", () =>
{
    var OnSale = Products.Where(x => x.SalePrice != 0 && x.SalePrice < x.Price).ToList();
    return OnSale;
})
.WithName("GetProductList");

// GET ONE
app.MapGet("/api/Products/Item/{id}", (int id) =>
{
    var product = Products.FirstOrDefault(x => x.ProductId == id);
    if (Products.FirstOrDefault(x => x.ProductId == id) is null)
    {
        return Results.NotFound();
    }
    else
        return Results.Ok(product);
});

// EDIT
app.MapPut("/api/Producsts/Edit/{id}", (int id, ProductDto updatedProduct) =>
{
    var message = "Please put in a name and description";
    if (CheckNull(updatedProduct) != false)
    {
        message = "Please Enter Name Less than 120 Characters and a Price greater than 0";

        if (Verification(updatedProduct) != false)
        {
            if (Products.FirstOrDefault(x => x.ProductId == id) is null)
            {
                return Results.NotFound();
            }
            else
            {
                foreach (ProductDto product in Products)
                {
                    if (product.ProductId == id)
                    {
                        product.ProductId = id;
                        product.Name = updatedProduct.Name;
                        product.Description = updatedProduct.Description;
                        product.Price = updatedProduct.Price;
                        product.SalePrice = updatedProduct.SalePrice;


                    }
                }
                var newProduct = Products.FirstOrDefault(x => x.ProductId == id);

                return Results.Ok(newProduct);
            }

        }
    }

    return Results.Ok(message);

});

// DELETE
app.MapDelete("/api/Producsts/Delete/{id}", (int id) =>
{
    if (Products.FirstOrDefault(x => x.ProductId == id) is null)
    {
        return Results.NotFound();
    }
    else
        Products.RemoveAll(x => x.ProductId == id);
    return Results.Ok("Product Deleted..... Hopefully");
});

app.Run();

static bool Verification(ProductDto product)
{
    var isVerified = false;

    if (product.Name.Length > 120 || product.Name == "")
    {
        return isVerified;
    }
    else if (product.Price <= 0 || product.SalePrice <= 0)
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

//public class ProductDto
//{
//    public int ProductId { get; set; }
//    public string Name { get; set; }
//    public string Description { get; set; }
//    public decimal Price { get; set; }
//    public decimal? SalePrice { get; set; }

//}

