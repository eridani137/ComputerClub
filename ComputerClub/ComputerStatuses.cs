using System.Windows.Media;
using ComputerClub.Infrastructure.Entities;

namespace ComputerClub;

public static class ComputerStatuses
{
    public static readonly IReadOnlyList<ComputerStatusDefinition> All = new List<ComputerStatusDefinition>
    {
        new()
        {
            Status = ComputerStatus.Available, Name = "Свободен", Color = new SolidColorBrush(Colors.DarkSlateGray)
        },
        new()
        {
            Status = ComputerStatus.Occupied, Name = "Занят", Color = new SolidColorBrush(Colors.DarkRed)
        },
        new()
        {
            Status = ComputerStatus.Reserved, Name = "Забронирован",
            Color = new SolidColorBrush(Colors.MediumAquamarine)
        },
        new()
        {
            Status = ComputerStatus.OutOfService, Name = "Не работает",
            Color = new SolidColorBrush(Colors.Black)
        },
    };

    public static ComputerStatusDefinition GetByStatus(ComputerStatus status) =>
        All.FirstOrDefault(s => s.Status == status) ?? All[0];
}

public record ComputerStatusDefinition
{
    public ComputerStatus Status { get; init; }
    public string Name { get; init; } = string.Empty;
    public SolidColorBrush Color { get; init; } = Brushes.Black;
}