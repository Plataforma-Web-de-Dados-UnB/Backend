using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Services;
using api.Services.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMultipleOrigins", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUsuario, UsuarioService>();
builder.Services.AddScoped<ICategoria, CategoriaService>();
builder.Services.AddScoped<IPainel, PainelService>();
builder.Services.AddScoped<ISugestao, SugestaoService>();
builder.Services.AddScoped<IPipeline, PipelineService>();
builder.Services.AddScoped<IPipelineExecucao, PipelineExecucaoService>();
builder.Services.AddScoped<ISupersetService, SupersetService>();
builder.Services.AddScoped<IAdmin, AdminService>();
builder.Services.AddSingleton<IRedisPublisher, RedisPublisher>();

builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<api.Helpers.PortugueseIdentityErrorDescriber>();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plataforma Web de Dados - UnB | API v1");
    });
}

app.UseCors("AllowMultipleOrigins");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();

    dbContext.Database.Migrate();
    dbContext.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS \"unaccent\";");

    var userManager = services.GetRequiredService<UserManager<Usuario>>();

    var configuration = services.GetRequiredService<IConfiguration>();
    var adminEmail = configuration["SuperAdmin:Email"] ?? "";
    var adminPassword = configuration["SuperAdmin:Password"] ?? "";

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var superAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (superAdmin == null)
        {
            var admin = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nome = "Admin",
                UltimoNome = "Principal",
                Cargo = CargoUsuario.SuperAdministrador,
                Status = StatusUsuario.Ativo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (!result.Succeeded)
            {
                throw new Exception("Erro ao criar Super Administrador: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}

app.Run();
