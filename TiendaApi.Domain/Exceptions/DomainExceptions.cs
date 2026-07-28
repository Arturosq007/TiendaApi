namespace TiendaApi.Domain.Exceptions;

// ─── Base ────────────────────────────────────────────────────────────────────

/// <summary>
/// Violación de una regla de negocio. HTTP 400.
/// </summary>
public class DomainException(string message) : Exception(message)
{
}

// ─── Específicas ─────────────────────────────────────────────────────────────

/// <summary>
/// Entidad no encontrada en la base de datos. HTTP 404.
/// </summary>
public class NotFoundException(string entityName, object key) : Exception(
    $"{entityName} con id '{key}' no fue encontrado.")
{
}

/// <summary>
/// Stock insuficiente para completar una venta.
/// </summary>
public class InsufficientStockException(string productName, int available, int requested) : DomainException(
    $"Stock insuficiente para '{productName}'. " + $"Disponible: {available}, solicitado: {requested}.")
{
}

/// <summary>
/// El cliente superaría su límite de crédito con esta operación.
/// </summary>
public class CreditLimitExceededException(string customerName, decimal limit, decimal currentDebt, decimal amount) : DomainException(
    $"'{customerName}' superaría su límite de crédito ({limit:C}). " +
               $"Deuda actual: {currentDebt:C}, intento agregar: {amount:C}.")
{
}

/// <summary>
/// Operación inválida dado el estado actual de la entidad.
/// Ej: intentar recibir una compra ya cancelada.
/// </summary>
public class InvalidEntityStateException(string entityName, string currentState, string attemptedAction) : DomainException(
    $"No se puede '{attemptedAction}' en {entityName} con estado '{currentState}'.")
{
}