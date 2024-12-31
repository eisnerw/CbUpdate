using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace CbUpdate.Domain.Entities;

public class Role : IdentityRole<string>
{
    public ICollection<UserRole> UserRoles { get; set; }
}
