namespace TiendaApi.Application.Features.Customers.Request;

public class CreateCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; } = 0;
}

public class UpdateCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
}

public class RegisterPaymentRequest
{
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}