using System.Windows.Media;

namespace ComputerClub;

public static class PcTypes
{
    public static readonly IReadOnlyList<PcTypeDefinition> All = new List<PcTypeDefinition>
    {
        new() { Id = 0, Name = "Не задано", Color = Brushes.Gray },
        new() { Id = 1, Name = "Эконом", Color = Brushes.DarkOrange },
        new() { Id = 2, Name = "Стандарт", Color = Brushes.PaleGoldenrod },
        new() { Id = 3, Name = "Премиум", Color = Brushes.PaleGreen },
        new() { Id = 5, Name = "VIP", Color = Brushes.Violet }
    };

    public static PcTypeDefinition GetById(int id) =>
        All.FirstOrDefault(t => t.Id == id) ?? All[0];
}

public class PcTypeDefinition
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Brush Color { get; init; } = Brushes.Gray;
}