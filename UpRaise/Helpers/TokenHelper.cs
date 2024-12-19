using UpRaise.Constants;
using UpRaise.DTOs;
using UpRaise.Entities;
using UpRaise.Services.EF;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Helpers
{

    public class TokenHelper
    {

        public async Task<UserDTO> GetLoginUserDTO(AppDatabaseContext _appDatabaseContext,  IDUser user)
        {
            var userDTO = new UserDTO();
            userDTO.Id = user.Id;
            userDTO.AliasId = user.AliasId;
            //userDTO.CompanyId = user.CompanyId;
            userDTO.Username = user.UserName;
            userDTO.FirstName = user.FirstName;
            userDTO.LastName = user.LastName;
            userDTO.PictureURL = user.PictureURL;
            userDTO.UpdatedAt = user.UpdatedAt;

            /*
            var companyRecord = await _appDatabaseContext.Companies
                                       .AsNoTracking()
                                       .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                       .Select(c => new { c.Id, c.CompanyName, c.PictureURL })
                                       .FirstOrDefaultAsync(i => i.Id == user.CompanyId);

            if (companyRecord != null)
            {
                userDTO.CompanyName = companyRecord.CompanyName;
                userDTO.CompanyPictureURL = companyRecord.PictureURL;
            }
            */

            return userDTO;
        }


        public string GetToken(string authenticationSecret, IDUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(authenticationSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = null,              // Not required as no third-party is involved
                Audience = null,            // Not required as no third-party is involved
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new System.Security.Claims.Claim[]
                {
                                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                    new System.Security.Claims.Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                                    new System.Security.Claims.Claim(ClaimTypes.Name, user.Id.ToString()),
                                    new System.Security.Claims.Claim(ClaimTypes.Role, user.Id.ToString()),
                                    new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer),
                                    //new System.Security.Claims.Claim(CustomClaimTypes.CompanyId,  user.CompanyId.ToString(), ClaimValueTypes.Integer),
                                    new System.Security.Claims.Claim(CustomClaimTypes.UserId,  user.Id.ToString(), ClaimValueTypes.Integer),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

    }
}