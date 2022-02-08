using Microsoft.EntityFrameworkCore;
using SP22.P03.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();

    var services = scope.ServiceProvider;

    DbSeedData.Initialize(services);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

//var currentId = 1;
//var products = new List<ProductDto>
//{
//    new ProductDto
//    {
//        Id = currentId++,
//        Name = "Half Life 2",
//        Description = "A game",
//        Price = 12.99m,
//    },
//    new ProductDto
//    {
//        Id = currentId++,
//        Name = "Visual Studio 2022: Professional",
//        Description = "A program",
//        Price = 199m,
//    },
//    new ProductDto
//    {
//        Id = currentId++,
//        Name = "Photoshop",
//        Description = "Fancy mspaint",
//        Price = 10m,
//        SalePrice = 1m
//    }
//};

//app.MapGet("/api/products", () =>
//    {
//        return products;
//    })
//    .Produces(200, typeof(ProductDto[]));

//app.MapGet("/api/products/{id}", (int id) =>
//    {
//        var result = products.FirstOrDefault(x => x.Id == id);
//        if (result == null)
//        {
//            return Results.NotFound();
//        }

//        return Results.Ok(result);
//    })
//    .WithName("GetProductById")
//    .Produces(404)
//    .Produces(200, typeof(ProductDto));

//app.MapGet("/api/products/sales", () =>
//    {
//        return products.Where(x => x.SalePrice != null);
//    })
//    .Produces(200, typeof(ProductDto[]));

//app.MapPost("/api/products", (ProductDto product) =>
//    {
//        if (product.Name == null ||
//            product.Name.Length > 120 ||
//            product.Price <= 0 ||
//            product.SalePrice < 0 ||
//            product.Description == null)
//        {
//            return Results.BadRequest();
//        }

//        product.Id = currentId++;
//        products.Add(product);
//        return Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product);
//    })
//    .Produces(400)
//    .Produces(201, typeof(ProductDto));

//app.MapPut("/api/products/{id}", (int id, ProductDto product) =>
//    {
//        if (product.Name == null ||
//            product.Name.Length > 120 ||
//            product.Price <= 0 ||
//            product.SalePrice < 0 ||
//            product.Description == null)
//        {
//            return Results.BadRequest();
//        }

//        var current = products.FirstOrDefault(x => x.Id == id);
//        if (current == null)
//        {
//            return Results.NotFound();
//        }

//        current.Name = product.Name;
//        current.Name = product.Name;
//        current.Price = product.Price;
//        current.Description = product.Description;
//        current.SalePrice = product.SalePrice;

//        return Results.Ok(current);
//    })
//    .Produces(400)
//    .Produces(404)
//    .Produces(200, typeof(ProductDto));

//app.MapDelete("/api/products/{id}", (int id) =>
//    {
//        var current = products.FirstOrDefault(x => x.Id == id);
//        if (current == null)
//        {
//            return Results.NotFound();
//        }

//        products.Remove(current);

//        return Results.Ok();
//    })
//    .Produces(400)
//    .Produces(404)
//    .Produces(200);


app.Run();

public partial class Program { }
