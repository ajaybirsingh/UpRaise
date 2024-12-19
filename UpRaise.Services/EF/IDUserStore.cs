using UpRaise.Helpers;
using UpRaise.Services.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpRaise.Entities
{
    public class IDUserStore : UserStore<IDUser, IDRole, AppDatabaseContext, int, IDUserClaim, IDUserRole, IDUserLogin, IDUserToken, IDRoleClaim>
    {
        public IDUserStore(AppDatabaseContext context)
            : base(context)
        {
        }
    }
}