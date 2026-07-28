using TiendaApi.Application.Features.CashRegisters.Requests;
using TiendaApi.Application.Features.CashRegisters.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.CashRegisters;

public class CashRegisterService(IUnitOfWork _uow) : ICashRegisterService
{

    public async Task<CashRegisterResponse> GetByIdAsync(Guid id)
    {
        var session = await _uow.CashRegisters.GetByIdAsync(id)
            ?? throw new NotFoundException("Turno de caja", id);

        var summary = await BuildSummaryAsync(session);
        return MapToDto(session, summary);
    }

    public async Task<CashRegisterResponse?> GetCurrentSessionAsync(Guid userId)
    {
        var session = await _uow.CashRegisters.GetOpenByUserAsync(userId);

        if (session is null) return null;

        var summary = await BuildSummaryAsync(session);
        return MapToDto(session, summary);
    }

    public async Task<PagedResult<CashRegisterResponse>> GetPagedAsync(
        int page, int pageSize,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null,
        bool? isOpen = null)
    {
        var result = await _uow.CashRegisters.GetPagedAsync(
            page, pageSize, userId, from, to, isOpen);

        // Para el listado no cargamos el summary — sería muy costoso
        // El summary solo se carga cuando se abre un turno específico
        var dtos = result.Items
            .Select(cr => MapToDto(cr, null))
            .ToList();

        return new PagedResult<CashRegisterResponse>(
            dtos, result.TotalCount,
            result.Page, result.PageSize);
    }

    public async Task<CashRegisterResponse> OpenAsync(OpenCashRegisterRequest request, Guid userId)
    {
        // Verificar que el cajero no tiene ya un turno abierto
        var hasOpenSession = await _uow.CashRegisters.HasOpenSessionAsync(userId);

        if (hasOpenSession)
            throw new DomainException(
                "Ya tenés un turno de caja abierto. " +
                "Ciérralo antes de abrir uno nuevo.");

        var session = CashRegister.Create(
            userId: userId,
            openingBalance: request.OpeningBalance,
            notes: request.Notes);

        await _uow.CashRegisters.AddAsync(session);
        await _uow.SaveChangesAsync();

        return MapToDto(session, null);
    }

    public async Task<CashRegisterResponse> CloseAsync(
        Guid id,
        CloseCashRegisterRequest request,
        Guid userId)
    {
        var session = await _uow.CashRegisters.GetByIdAsync(id)
            ?? throw new NotFoundException("Turno de caja", id);

        // Verificar que el turno pertenece al usuario
        // o que el usuario es admin
        var requestingUser = await _uow.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException("Usuario", userId);

        if (session.UserId != userId && !requestingUser.IsAdmin())
            throw new DomainException(
                "Solo puedes cerrar tu propio turno de caja.");

        // Calcular el balance esperado según las ventas en efectivo
        var expectedBalance = await CalculateExpectedBalanceAsync(
            session.Id, session.OpeningBalance);

        // La entidad valida que esté abierto y calcula la diferencia
        session.Close(request.ClosingBalance, expectedBalance);

        if (!string.IsNullOrWhiteSpace(request.Notes))
            session.AddNotes(request.Notes);

        _uow.CashRegisters.Update(session);
        await _uow.SaveChangesAsync();

        var summary = await BuildSummaryAsync(session);
        return MapToDto(session, summary);
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────

    private async Task<decimal> CalculateExpectedBalanceAsync(
        Guid sessionId, decimal openingBalance)
    {
        // Traer todas las ventas en efectivo del turno
        var sales = await _uow.Sales.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            cashRegisterId: sessionId,
            isCancelled: false);

        // El balance esperado es el monto inicial más todas las ventas en efectivo
        var cashSales = sales.Items
            .Where(s => s.PaymentType == SalePaymentType.Efectivo)
            .Sum(s => s.Total);

        return openingBalance + cashSales;
    }

    private async Task<CashRegisterSummaryResponse> BuildSummaryAsync(
        CashRegister session)
    {
        var sales = await _uow.Sales.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            cashRegisterId: session.Id);

        var activeSales = sales.Items.Where(s =>
            s.Status != SaleStatus.Cancelada).ToList();

        return new CashRegisterSummaryResponse
        {
            TotalSales = activeSales.Count,
            CancelledSales = sales.Items.Count - activeSales.Count,
            TotalCash = activeSales
                .Where(s => s.PaymentType == SalePaymentType.Efectivo)
                .Sum(s => s.Total),
            TotalCard = activeSales
                .Where(s => s.PaymentType == SalePaymentType.Tarjeta)
                .Sum(s => s.Total),
            TotalTransfer = activeSales
                .Where(s => s.PaymentType == SalePaymentType.Transferencia)
                .Sum(s => s.Total),
            TotalCredit = activeSales
                .Where(s => s.PaymentType == SalePaymentType.Fiado)
                .Sum(s => s.Total),
            TotalRevenue = activeSales.Sum(s => s.Total)
        };
    }

    // ─── Mapeo ────────────────────────────────────────────────────────────────

    private static CashRegisterResponse MapToDto(
        CashRegister session,
        CashRegisterSummaryResponse? summary) => new()
        {
            Id = session.Id,
            OpenedAt = session.OpenedAt,
            ClosedAt = session.ClosedAt,
            OpeningBalance = session.OpeningBalance,
            ClosingBalance = session.ClosingBalance,
            ExpectedBalance = session.ExpectedBalance,
            Difference = session.Difference,
            IsOpen = session.IsOpen,
            Notes = session.Notes,
            UserId = session.UserId,
            UserFullName = session.User?.FullName ?? string.Empty,
            Summary = summary,
            CreatedAt = session.CreatedAt
        };
}