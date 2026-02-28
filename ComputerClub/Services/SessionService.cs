using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.Services;

public class SessionService(
    ApplicationDbContext db,
    UserManager<ComputerClubIdentity> userManager
)
{
    public async Task<SessionEntity> OpenSession(
        int clientId, int computerId, int tariffId,
        TimeSpan plannedDuration,
        CancellationToken ctx = default)
    {
        var client = await userManager.FindByIdAsync(clientId.ToString())
                     ?? throw new InvalidOperationException("Клиент не найден");

        var computer = await db.Computers.FindAsync([computerId], ctx)
                       ?? throw new InvalidOperationException("Компьютер не найден");

        if (computer.Status != ComputerStatus.Available)
        {
            throw new InvalidOperationException("Компьютер недоступен");
        }

        var tariff = await db.Tariffs.FindAsync([tariffId], ctx)
                     ?? throw new InvalidOperationException("Тариф не найден");

        var plannedCost = Math.Round((decimal)plannedDuration.TotalHours * tariff.PricePerHour, 2);
        if (client.Balance < plannedCost)
        {
            throw new InvalidOperationException(
                $"Недостаточно средств. Стоимость аренды: {plannedCost} ₽, баланс: {client.Balance} ₽");
        }

        computer.Status = ComputerStatus.Occupied;
        client.Balance -= plannedCost;

        var session = new SessionEntity
        {
            ClientId = clientId,
            ComputerId = computerId,
            TariffId = tariffId,
            StartedAt = DateTime.UtcNow,
            PlannedDuration = plannedDuration,
            Status = SessionStatus.Active
        };

        db.Sessions.Add(session);

        db.Payments.Add(new PaymentEntity
        {
            ClientId = clientId,
            Amount = -plannedCost,
            Type = PaymentType.Charge,
            CreatedAt = DateTime.UtcNow,
            Session = session
        });

        await db.SaveChangesAsync(ctx);
        return session;
    }

    public async Task<SessionEntity> CloseSession(int sessionId, CancellationToken ctx = default)
    {
        var session = await db.Sessions
                          .Include(s => s.Client)
                          .Include(s => s.Computer)
                          .Include(s => s.Tariff)
                          .FirstOrDefaultAsync(s => s.Id == sessionId, ctx)
                      ?? throw new InvalidOperationException("Сессия не найдена");

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException("Сессия уже завершена");
        }

        var endedAt = DateTime.UtcNow;
        var actualHours = (decimal)(endedAt - session.StartedAt).TotalHours;
        var actualCost = Math.Round(actualHours * session.Tariff.PricePerHour, 2);
        var plannedCost = Math.Round((decimal)session.PlannedDuration.TotalHours * session.Tariff.PricePerHour, 2);

        session.EndedAt = endedAt;
        session.TotalCost = actualCost;
        session.Status = SessionStatus.Completed;
        session.Computer.Status = ComputerStatus.Available;

        var refund = plannedCost - actualCost;
        if (refund > 0)
        {
            session.Client.Balance += refund;

            db.Payments.Add(new PaymentEntity
            {
                ClientId = session.ClientId,
                Amount = refund,
                Type = PaymentType.Refund,
                CreatedAt = DateTime.UtcNow,
                SessionId = session.Id
            });
        }

        await db.SaveChangesAsync(ctx);
        return session;
    }
}