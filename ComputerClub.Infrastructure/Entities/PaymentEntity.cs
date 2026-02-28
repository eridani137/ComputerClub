namespace ComputerClub.Infrastructure.Entities;

public class PaymentEntity
{
    public int Id { get; init; }

    public int ClientId { get; set; }
    public ComputerClubIdentity Client { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? SessionId { get; set; }
    public SessionEntity? Session { get; set; }
}