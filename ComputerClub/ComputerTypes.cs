using System.Windows.Media;

namespace ComputerClub;

public static class ComputerTypes
{
    public static readonly IReadOnlyList<ComputerTypeDefinition> All = new List<ComputerTypeDefinition>
    {
        new() { Id = 0, Name = "Не задано", Color = Brushes.Gray },
        new() { Id = 1, Name = "Эконом", Color = Brushes.DarkOrange },
        new() { Id = 2, Name = "Стандарт", Color = Brushes.PaleGoldenrod },
        new() { Id = 3, Name = "Премиум", Color = Brushes.PaleGreen },
        new() { Id = 5, Name = "VIP", Color = Brushes.Violet }
    };

    public static ComputerTypeDefinition GetById(int id) =>
        All.FirstOrDefault(t => t.Id == id) ?? All[0];
}

public class ComputerTypeDefinition
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Brush Color { get; init; } = Brushes.Gray;
}