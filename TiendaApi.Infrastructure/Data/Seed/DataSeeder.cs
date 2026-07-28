using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Infrastructure.Data.Seed;

public class DataSeeder(
    AppDbContext _context,
    IPasswordHasher _passwordHasher,
    ILogger<DataSeeder> _logger)
{

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
            await SeedUsersAsync();
            await SeedDataAsync();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeding completado correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el seeding.");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        var adminExists = await _context.Users
            .AnyAsync(u => u.Username == "admin");

        if (adminExists)
        {
            _logger.LogInformation("Usuarios ya existen, saltando...");
            return;
        }

        var admin = User.Create(
            username: "admin",
            name: "Administrador",
            lastName: "Sistema",
            passwordHash: _passwordHasher.Hash("Admin123!"),
            role: UserRole.Admin);

        var cajero = User.Create(
            username: "cajero",
            name: "Cajero",
            lastName: "Prueba",
            passwordHash: _passwordHasher.Hash("Cajero123!"),
            role: UserRole.Cajero);

        await _context.Users.AddRangeAsync(admin, cajero);
        _logger.LogInformation("Usuarios iniciales creados.");
    }

    private async Task SeedDataAsync()
    {
        // 1. Verificar si ya existen datos para evitar duplicación
        if (await _context.Categories.AnyAsync() ||
            await _context.Products.AnyAsync() ||
            await _context.Suppliers.AnyAsync())
        {
            _logger.LogInformation("Los datos iniciales ya existen. Saltando seeding...");
            return;
        }

        _logger.LogInformation("Iniciando el seeding unificado de datos...");

        // 2. Crear Proveedores (Suppliers)
        var provDistribuidora = Supplier.Create("Distribuidora Global S.A.", "contacto@global.com", "+51999888777", "Av. Principal 123");
        var provLacteos = Supplier.Create("Lácteos del Valle", "ventas@delvalle.com", "+51999111222", "Zona Industrial Mote 4");
        var provLimpieza = Supplier.Create("Limpieza Total Corp", "info@limpiazatotal.com", null, null);

        var suppliers = new[] { provDistribuidora, provLacteos, provLimpieza };

        // 3. Crear Categorías (Categories)
        var bebidas = Category.Create("Bebidas");
        var lacteos = Category.Create("Lácteos");
        var panaderia = Category.Create("Panadería");
        var abarrotes = Category.Create("Abarrotes");
        var limpieza = Category.Create("Limpieza");
        var snacks = Category.Create("Snacks");
        var carnes = Category.Create("Carnes");
        var frutas = Category.Create("Frutas y Verduras");

        var categories = new[] { bebidas, lacteos, panaderia, abarrotes, limpieza, snacks, carnes, frutas };

        // 4. Crear Productos (Products) vinculando los Guids locales de Categorías y Proveedores
        var products = new[]
        {
        // Bebidas (Proveedor: Distribuidora Global)
        Product.Create("Coca Cola 2L", "BEB-COC-0001", 1.50m, 2.00m, 5, bebidas.Id, provDistribuidora.Id, 15),
        Product.Create("Pepsi 2L", "BEB-PEP-0001", 1.40m, 1.90m, 5, bebidas.Id, provDistribuidora.Id, 10),
        Product.Create("Agua Mineral 500ml", "BEB-AGU-0002", 0.50m, 0.90m, 10, bebidas.Id, provDistribuidora.Id, 30),
        Product.Create("Jugo de Naranja 1L", "BEB-JUG-0003", 1.20m, 1.80m, 5, bebidas.Id, provDistribuidora.Id, 12),

        // Lácteos (Proveedor: Lácteos del Valle)
        Product.Create("Leche Entera 1L", "LAC-LEC-0001", 0.80m, 1.20m, 10, lacteos.Id, provLacteos.Id, 25),
        Product.Create("Yogurt Natural 1kg", "LAC-YOG-0002", 1.80m, 2.50m, 5, lacteos.Id, provLacteos.Id, 14),
        Product.Create("Queso Cheddar 200g", "LAC-QUE-0003", 2.20m, 3.10m, 4, lacteos.Id, provLacteos.Id, 8),

        // Limpieza (Proveedor: Limpieza Total)
        Product.Create("Detergente Polvo 1kg", "LIM-DET-0001", 2.50m, 3.50m, 5, limpieza.Id, provLimpieza.Id, 10),
        Product.Create("Lavavajillas Líquido", "LIM-LAV-0002", 1.10m, 1.65m, 6, limpieza.Id, provLimpieza.Id, 20),
        Product.Create("Desinfectante Pisos 1L", "LIM-DES-0003", 0.90m, 1.40m, 5, limpieza.Id, provLimpieza.Id, 15),

        // Abarrotes (Sin proveedor asignado explícitamente por ahora -> null)
        Product.Create("Arroz Extra 1kg", "ABA-ARR-0001", 0.70m, 1.10m, 20, abarrotes.Id, null, 50),
        Product.Create("Fideos Spaghetti 500g", "ABA-FID-0002", 0.45m, 0.75m, 15, abarrotes.Id, null, 40),
        Product.Create("Aceite Vegetal 1L", "ABA-ACE-0003", 2.10m, 2.90m, 10, abarrotes.Id, null, 22),

        // Snacks (Sin proveedor asignado)
        Product.Create("Papas Fritas Clásicas", "SNA-PAP-0001", 0.60m, 1.00m, 15, snacks.Id, null, 35),
        Product.Create("Chocolates con Maní", "SNA-CHO-0002", 0.85m, 1.30m, 10, snacks.Id, null, 18)
    };

        // 5. Registrar todo en el contexto usando AddRangeAsync
        await _context.Suppliers.AddRangeAsync(suppliers);
        await _context.Categories.AddRangeAsync(categories);
        await _context.Products.AddRangeAsync(products);

        _logger.LogInformation(
            "Seeding completado. Creados: {SupCount} proveedores, {CatCount} categorías y {ProdCount} productos.",
            suppliers.Length,
            categories.Length,
            products.Length);
    }


}