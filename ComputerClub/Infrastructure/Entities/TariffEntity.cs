namespace ComputerClub.Infrastructure.Entities;

public class TariffEntity
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;

    public decimal PricePerHour { get; set; }

    public ICollection<SessionEntity> Sessions { get; set; } = [];
}