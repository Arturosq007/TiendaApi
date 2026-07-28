using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Text;
using TiendaApi.Application;
using TiendaApi.Infrastructure;
using TiendaApi.Infrastructure.Data.Seed;
using TiendaApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRO DE SERVICIOS DE TUS CAPAS
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);


// --- CONFIGURACIÓN JWT Y COOKIES ---

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "TuLlaveSecretaSuperLargaDe32Caracteres");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Buscamos el token en la cookie llamada "access_token"
            context.Token = context.Request.Cookies["access_token"];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();

// 1. Registra tus validadores de Application
builder.Services.AddValidatorsFromAssembly(typeof(TiendaApi.Application.DependencyInjection).Assembly);

// 2. Activa la validación automática moderna
builder.Services.AddFluentValidationAutoValidation();

// 2. CONFIGURACIÓN DE OPENAPI NATIVO (.NET 9)
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
// 3. PIPELINE DE PETICIONES HTTP (Middleware)
if (app.Environment.IsDevelopment())
{
    // Genera el endpoint del JSON (por defecto en /openapi/v1.json)
    app.MapOpenApi();

    // Renderiza la interfaz de Scalar apuntando al JSON nativo
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Tienda API")
            .WithTheme(ScalarTheme.DeepSpace) // Elige tu tema: DeepSpace, Mars, Midnight, etc.
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient); // Código de ejemplo por defecto
    });
}

app.UseHttpsRedirection();
// Flujo de seguridad para tus Cookies HttpOnly

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ─── BLOQUE PARA EJECUTAR EL SEEDER ──────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<DataSeeder>();
        // Ejecutamos el método asíncrono y esperamos a que termine
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error crítico inesperado al arrancar el Seeder.");
    }
}
app.Run();