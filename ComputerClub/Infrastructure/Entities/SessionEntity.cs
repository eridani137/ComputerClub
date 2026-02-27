namespace ComputerClub.Infrastructure.Entities;

public class SessionEntity
{
    public int Id { get; init; }

    public int ClientId { get; set; }
    public ComputerClubIdentity Client { get; set; } = null!;

    public int ComputerId { get; set; }
    public ComputerEntity Computer { get; set; } = null!;

    public int TariffId { get; set; }
    public TariffEntity Tariff { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    public TimeSpan PlannedDuration { get; set; }
    public DateTime PlannedEndAt => StartedAt + PlannedDuration;

    public decimal? TotalCost { get; set; }
    public SessionStatus Status { get; set; }
}

public enum SessionStatus
{
    Active,
    Completed,
    CancelledInsufficientFunds
}