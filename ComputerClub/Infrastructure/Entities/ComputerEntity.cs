namespace ComputerClub.Infrastructure.Entities;

public class ComputerEntity
{
    public int Id { get; init; }

    public double X { get; set; }
    public double Y { get; set; }

    public int TypeId { get; set; }
}