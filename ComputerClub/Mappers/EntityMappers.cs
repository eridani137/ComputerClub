using ComputerClub.Infrastructure.Entities;
using ComputerClub.Models;

namespace ComputerClub.Mappers;

public static class EntityMappers
{
    public static ComputerItem Map(this ComputerEntity e) => new()
    {
        Id = e.Id,
        X = e.X,
        Y = e.Y,
        TypeId = e.TypeId,
        Status = e.Status
    };

    public static ClientItem Map(this ComputerClubIdentity e) => new()
    {
        Id = e.Id,
        Login = e.UserName ?? string.Empty,
        FullName = e.FullName,
        PhoneNumber = e.PhoneNumber,
        Balance = e.Balance
    };

    public static TariffItem Map(this TariffEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        PricePerHour = e.PricePerHour,
        ComputerTypeId =  e.ComputerTypeId,
    };

    public static SessionItem Map(this SessionEntity e) => new()
    {
        Id = e.Id,
        ClientId = e.ClientId,
        ClientName = e.Client?.FullName ?? string.Empty,
        ComputerId = e.ComputerId,
        TariffId = e.TariffId,
        TariffName = e.Tariff?.Name ?? string.Empty,
        PricePerHour = e.Tariff?.PricePerHour ?? 0,
        StartedAt = e.StartedAt,
        EndedAt = e.EndedAt,
        TotalCost = e.TotalCost,
        Status = e.Status,
        PlannedDuration = e.PlannedDuration,
    };
    
    public static PaymentItem Map(this PaymentEntity e) => new()
    {
        Id = e.Id,
        ClientId = e.ClientId,
        ClientName = e.Client?.UserName ?? string.Empty,
        Amount = e.Amount,
        CreatedAt = e.CreatedAt,
        SessionId = e.SessionId
    };
}