namespace ComputerClub.Infrastructure.Entities;

public class PaymentEntity
{
    public int Id { get; init; }

    public int ClientId { get; set; }
    public ComputerClubIdentity Client { get; set; } = null!;

    public PaymentType Type { get; set; }
    
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? SessionId { get; set; }
    public SessionEntity? Session { get; set; }
}

public enum PaymentType
{
    TopUp,
    Charge,
    Refund
}