var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Plataforma Web de Dados - UnB | API v1"));
    /* app.UseReDoc(options =>
    {
        options.DocumentTitle = "Plataforma Web de Dados - UnB | API v1";
        options.SpecUrl = "/openapi/v1.json"; // Aponta para o JSON gerado acima
        options.RoutePrefix = "docs"; // Define a rota, ficando em localhost:5xxx/docs
    }); */
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
