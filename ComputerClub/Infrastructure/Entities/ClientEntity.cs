namespace ComputerClub.Infrastructure.Entities;

public class ClientEntity
{
    public int Id { get; init; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    public ICollection<SessionEntity> Sessions { get; set; } = [];
}