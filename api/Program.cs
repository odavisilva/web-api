using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<Product.ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);
 
 app.MapPost("/products", (ProductDTO productDTO, Product.ApplicationDbContext context) => {
    var category = context.Categories.Where(c => c.Id == productDTO.categoryId).First();
    var product = new Product {
        Code = productDTO.Code,
        Name = productDTO.Name,
        Description = productDTO.Description,
        Category = category
    };
    context.Products.Add(product);
    context.SaveChanges();
    return Results.Created($"/products/{product.Id}", product.Id);
 });

app.MapGet("products/{code}", ( [FromRoute] string code) => { 
    var product = ProductRepository.GetBy(code);
    if(product != null) {
        Console.WriteLine("Product found");
        return Results.Ok(product);
    }  
    return Results.NotFound();
});

app.MapPut("/products", (Product product) => {
    var productSaved = ProductRepository.GetBy(product.Code);
    productSaved.Name = product.Name;
    return Results.Ok();
});

app.MapDelete("/products/{code}", ( [FromRoute] string code) => {
    var productSaved = ProductRepository.GetBy(code);
    ProductRepository.Remove(productSaved);
    return Results.Ok();
});
if(app.Environment.IsStaging())
app.MapGet("/configuration/database", (IConfiguration configuration) => {
    return Results.Ok($"{configuration["Database:connection"]}/{configuration["Database:port"]}");
});

app.Run();
