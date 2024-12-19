using UpRaise.Helpers;
using UpRaise.Services.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpRaise.Entities
{
    public class IDRoleStore : RoleStore<IDRole, AppDatabaseContext, int, IDUserRole, IDRoleClaim>
    {
        public IDRoleStore(AppDatabaseContext context)
            : base(context)
        {
        }
    }
}