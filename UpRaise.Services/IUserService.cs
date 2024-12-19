using UpRaise.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace UpRaise.Services
{
    public interface IUserService
    {
        IEnumerable<IDUser> GetAll();

        Task<int?> GetUserIdByAliasIdAsync(Guid aliasId);

        Task<IDUser> GetByIdAsync(int id);
        Task<IDUser> CreateAsync(IDUser user, string password);
        Task UpdateAsync(IDUser user, string password = null);
        Task DeleteAsync(int id);
        Task<bool> ValidateResetInput(string username, string token, int userId);
    }


}