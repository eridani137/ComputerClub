using System.ComponentModel.DataAnnotations;

namespace ComputerClub.Infrastructure.Entities;

public class TariffEntity
{
    public int Id { get; init; }
    [MaxLength(100)] public string Name { get; set; } = string.Empty;

    public decimal PricePerHour { get; set; }
    
    public int ComputerTypeId { get; set; }

    public ICollection<SessionEntity> Sessions { get; set; } = [];
}