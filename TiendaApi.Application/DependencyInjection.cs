using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TiendaApi.Application.Features.Alerts;
using TiendaApi.Application.Features.Auth;
using TiendaApi.Application.Features.CashRegisters;
using TiendaApi.Application.Features.Categories;
using TiendaApi.Application.Features.Customers;
using TiendaApi.Application.Features.Products;
using TiendaApi.Application.Features.Purchases;
using TiendaApi.Application.Features.Sales;
using TiendaApi.Application.Features.Stock;
using TiendaApi.Application.Features.Suppliers;
using TiendaApi.Application.Features.Users;
using TiendaApi.Application.Interfaces.Services;

namespace TiendaApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Registra todos los validadores del ensamblado automáticamente
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();  
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<IAlertService, AlertService>();

        return services;
    }
}