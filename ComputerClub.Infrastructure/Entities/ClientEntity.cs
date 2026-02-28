using Microsoft.AspNetCore.Identity;

namespace ComputerClub.Infrastructure.Entities;

public class ComputerClubIdentity : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    
    public ICollection<SessionEntity> Sessions { get; set; } = [];
}