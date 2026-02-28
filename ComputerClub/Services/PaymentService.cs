using ComputerClub.Infrastructure;
using ComputerClub.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComputerClub.Services;

public class PaymentService(ApplicationDbContext context)
{
    public IQueryable<PaymentEntity> GetAll()
    {
        return context.Payments
            .Include(p => p.Client)
            .Include(p => p.Session)
            .OrderByDescending(p => p.CreatedAt);
    }

    public IQueryable<PaymentEntity> GetByClient(int clientId)
    {
        return GetAll().Where(p => p.ClientId == clientId);
    }

    public async Task<PaymentEntity> TopUp(int clientId, decimal amount, CancellationToken ctx = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Сумма должна быть положительной");
        }

        var client = await context.Users.FindAsync([clientId], ctx)
                     ?? throw new InvalidOperationException("Клиент не найден");

        client.Balance += amount;

        var payment = new PaymentEntity
        {
            ClientId = clientId,
            Amount = amount,
            CreatedAt = DateTime.UtcNow
        };

        context.Payments.Add(payment);
        await context.SaveChangesAsync(ctx);

        return payment;
    }

    public async Task<decimal> GetTotalTopUp(int clientId, CancellationToken ctx = default)
    {
        return await context.Payments
            .Where(p => p.ClientId == clientId && p.Amount > 0 && p.SessionId == null)
            .SumAsync(p => p.Amount, ctx);
    }

    public async Task<decimal> GetTotalSpent(int clientId, CancellationToken ctx = default)
    {
        return await context.Payments
            .Where(p => p.ClientId == clientId && p.Amount < 0)
            .SumAsync(p => p.Amount, ctx);
    }
}