
using FluentAssertions;
using Moq;
using TiendaApi.Application.Features.Customers;
using TiendaApi.Application.Features.Customers.Request;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;
using TiendaApi.UnitTests.Helpers;

namespace TiendaApi.UnitTests.Application;

public class CustomerServiceTests
{
    private readonly MockUnitOfWork _mockUow;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _mockUow = new MockUnitOfWork();
        _customerService = new CustomerService(_mockUow.Object);
    }

    // Helpers
    private static Customer CreateCustomer(string fullName = "Jhon Doe", decimal creditLimit = 1000m, string? phone = null, string? address = null)
    {
        var customer = Customer.Create(
            fullName,
            phone,
            address,
            creditLimit
        );

        return customer;
    }
    private static CreateCustomerRequest CreateRequest(string fullName = "Jhon Doe", string? phone = null, string? address = null, decimal creditLimit = 1000m)
    {
        return new CreateCustomerRequest
        {
            FullName = fullName,
            Phone = phone ?? "1234567890",
            Address = address,
            CreditLimit = creditLimit
        };
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCustomer_ShouldReturnCustomerDto()
    {
        // Arrange
        var customer = CreateCustomer(fullName: "John Doe", creditLimit: 1000m, phone: "1234567890", address: "123 Main St");
        var customerId = customer.Id;

        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("John Doe");
        result.Phone.Should().Be("1234567890");
        result.Address.Should().Be("123 Main St");
        result.CreditLimit.Should().Be(1000m);

    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingCustomer_ShouldThrowNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync((Customer)null!); // Forzamos el retorno nulo

        // Act & Assert
        var action = async () => await _customerService.GetByIdAsync(customerId);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCustomer()
    {
        // Arrange
        var request = CreateRequest(fullName: "Alice Smith", phone: "1234567890", address: "123 Main St", creditLimit: 1000m);

        // Act
        var result = await _customerService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        // Assert
        _mockUow.Customers.Verify(
            r => r.AddAsync(It.IsAny<Customer>()), Times.Once);

        _mockUow.Mock.Verify(
            u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ToggleActiveAsync_WithActiveCustomerAndDebt_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer(fullName: "Debtor Customer");
     
        customer.AddDebt(500m); // Simulamos que el cliente tiene una deuda pendiente

        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customer.Id))
            .ReturnsAsync(customer);

        // Act & Assert
        var action = async () => await _customerService.ToggleActiveAsync(customer.Id);

        await action.Should().ThrowAsync<DomainException>()
            .WithMessage($"No se puede desactivar a '{customer.FullName}' porque tiene una deuda pendiente*");

        // Verificar que nunca se guardaron cambios en la base de datos debido al error
        _mockUow.Mock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ToggleActiveAsync_WithActiveCustomerAndNoDebt_ShouldDeactivateCustomer()
    {
        // Arrange
        var customer = CreateCustomer(fullName: "Active Customer", creditLimit: 1000m);
        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customer.Id))
            .ReturnsAsync(customer);
        // Act
        await _customerService.ToggleActiveAsync(customer.Id);
        // Assert
        customer.IsActive.Should().BeFalse();
        _mockUow.Customers.Verify(r => r.Update(customer), Times.Once);
        _mockUow.Mock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ToggleActiveAsync_WithInactiveCustomer_ShouldActivateCustomer()
    {
        // Arrange
        var customer = CreateCustomer(fullName: "Inactive Customer", creditLimit: 1000m);
        customer.Deactivate(); // Desactivamos al cliente
        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customer.Id))
            .ReturnsAsync(customer);
        // Act
        await _customerService.ToggleActiveAsync(customer.Id);
        // Assert
        customer.IsActive.Should().BeTrue();
        _mockUow.Customers.Verify(r => r.Update(customer), Times.Once);
        _mockUow.Mock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }
    [Fact]
    public async Task RegisterPaymentAsync_WithExistingDebt_ShouldRegisterPayment()
    {
        // Arrange
        var customer = CreateCustomer(); // Cliente con deuda pendiente
        customer.AddDebt(500m); // Simulamos que el cliente tiene una deuda pendiente
        var userId = Guid.NewGuid();
        var request = new RegisterPaymentRequest { Amount = 200m, Notes = "Abono parcial" };
        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customer.Id))
            .ReturnsAsync(customer);
        // Act
        await _customerService.RegisterPaymentAsync(customer.Id, request, userId);
        // Assert
        customer.CurrentDebt.Should().Be(300m); // La deuda pendiente se reduce en 200
        _mockUow.Customers.Verify(r => r.Update(customer), Times.Once);
        _mockUow.Mock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RegisterPaymentAsync_WithNoDebt_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateCustomer(); // Cliente nuevo sin deuda
        var userId = Guid.NewGuid();
        var request = new RegisterPaymentRequest { Amount = 100m, Notes = "Abono inicial" };

        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customer.Id))
            .ReturnsAsync(customer);

        // Act & Assert
        var action = async () => await _customerService.RegisterPaymentAsync(customer.Id, request, userId);

        await action.Should().ThrowAsync<DomainException>()
            .WithMessage($"El cliente '{customer.FullName}' no tiene deuda pendiente.");
    }
}