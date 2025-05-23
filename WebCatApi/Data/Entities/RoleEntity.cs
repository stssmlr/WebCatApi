﻿using Microsoft.AspNetCore.Identity;

namespace WebCatApi.Data.Entities
{
    public class RoleEntity : IdentityRole<long>
    {
        public virtual ICollection<UserRoleEntity>? UserRoles { get; set; }
    }
}
