using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.Services;

public class SessionService(ApplicationDbContext db)
{
    public async Task<SessionEntity> OpenSession(
        int clientId, 
        int computerId, 
        int tariffId,
        CancellationToken ctx = default)
    {
        var client = await db.Clients.FindAsync([clientId], ctx)
                     ?? throw new InvalidOperationException("Клиент не найден");

        var computer = await db.Computers.FindAsync([computerId], ctx)
                       ?? throw new InvalidOperationException("Компьютер не найден");

        if (computer.Status != ComputerStatus.Available) throw new InvalidOperationException("Компьютер недоступен");

        var tariff = await db.Tariffs.FindAsync([tariffId], ctx)
                     ?? throw new InvalidOperationException("Тариф не найден");
        
        if (client.Balance < tariff.PricePerHour)
        {
            throw new InvalidOperationException($"Недостаточно средств. Минимальный баланс для этого тарифа: {tariff.PricePerHour} ₽");
        }
        
        computer.Status = ComputerStatus.Occupied;
        
        var session = new SessionEntity
        {
            ClientId = clientId,
            ComputerId = computerId,
            TariffId = tariffId,
            StartedAt = DateTime.UtcNow,
            Status = SessionStatus.Active
        };
        
        db.Sessions.Add(session);
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
        var hours = (decimal)(endedAt - session.StartedAt).TotalHours;
        var cost = Math.Round(hours * session.Tariff.PricePerHour, 2);
        
        session.EndedAt = endedAt;
        session.TotalCost = cost;
        
        if (session.Client.Balance >= cost)
        {
            session.Client.Balance -= cost;
            session.Status = SessionStatus.Completed;
        }
        else
        {
            session.TotalCost = session.Client.Balance;
            session.Client.Balance = 0;
            session.Status = SessionStatus.CancelledInsufficientFunds;
        }
        
        session.Computer.Status = ComputerStatus.Available;
        
        await db.SaveChangesAsync(ctx);

        return session;
    }
    
    public async Task TopUpBalance(int clientId, decimal amount, CancellationToken ctx = default)
    {
        if (amount <= 0) throw new ArgumentException("Сумма должна быть положительной");

        var client = await db.Clients.FindAsync([clientId], ctx)
                     ?? throw new InvalidOperationException("Клиент не найден.");

        client.Balance += amount;
        await db.SaveChangesAsync(ctx);
    }
    
    public IQueryable<SessionEntity> GetActiveSessions()
    {
        return db.Sessions
            .Include(s => s.Client)
            .Include(s => s.Computer)
            .Include(s => s.Tariff)
            .Where(s => s.Status == SessionStatus.Active);
    }
}