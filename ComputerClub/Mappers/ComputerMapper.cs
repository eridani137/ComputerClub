using ComputerClub.Infrastructure.Entities;
using ComputerClub.Models;

namespace ComputerClub.Mappers;

public static class ComputerMapper
{
    public static ComputerCanvasItem Map(this ComputerEntity entity)
    {
        return new ComputerCanvasItem()
        {
            Id =  entity.Id,
            TypeId = entity.TypeId,
            X = entity.X,
            Y = entity.Y,
        };
    }

    public static ComputerEntity Map(this ComputerCanvasItem item)
    {
        return new ComputerEntity()
        {
            Id =  item.Id,
            TypeId = item.TypeId,
            X = item.X,
            Y = item.Y,
        };
    }
}