using ComputerClub.Infrastructure.Entities;
using ComputerClub.Models;

namespace ComputerClub.Mappers;

public static class PcEntityMapper
{
    public static CanvasItem Map(this PcEntity entity)
    {
        return new CanvasItem()
        {
            Id =  entity.Id,
            TypeId = entity.TypeId,
            X = entity.X,
            Y = entity.Y,
        };
    }

    public static PcEntity Map(this CanvasItem item)
    {
        return new PcEntity()
        {
            Id =  item.Id,
            TypeId = item.TypeId,
            X = item.X,
            Y = item.Y,
        };
    }
}