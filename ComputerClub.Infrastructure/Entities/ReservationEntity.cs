namespace ComputerClub.Infrastructure.Entities;

public class ReservationEntity
{
    public int Id { get; init; }

    public int ClientId { get; set; }
    public ComputerClubIdentity Client { get; set; } = null!;

    public int ComputerId { get; set; }
    public ComputerEntity Computer { get; set; } = null!;

    public int TariffId { get; set; }
    public TariffEntity Tariff { get; set; } = null!;

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public ReservationStatus Status { get; set; }
}

public enum ReservationStatus
{
    Pending,
    Active,
    Completed,
    Cancelled
}