using UpRaise.Entities;
using UpRaise.Helpers;
using UpRaise.Services.EF;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace UpRaise.Services
{

    public class UserService : IUserService
    {
        private readonly AppDatabaseContext _context;
        private readonly EmailHelper _emailHelper;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDatabaseContext context, EmailHelper emailHelper, ILogger<UserService> logger)
        {
            _context = context;
            _emailHelper = emailHelper;
            _logger = logger;
        }

        public async Task<int?> GetUserIdByAliasIdAsync(Guid aliasId)
        {
            var user = await _context.Users
              .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
              .SingleAsync(i => i.AliasId == aliasId);

            if (user != null)
                return user.Id;

            return null;
        }

        public IEnumerable<IDUser> GetAll()
        {
            return _context.Users;
        }

        public async Task<IDUser> GetByIdAsync(int id)
        {
            return await _context.Users
                .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                .SingleAsync(i => i.Id == id);
        }


        //var user = await _context.Users
        //.Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
        //.FirstOrDefaultAsync(i => i.UserName.ToLower() == username.ToLower());


        private string RandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }


        public async Task<IDUser> CreateAsync(IDUser user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            var lowercaseUsername = user.UserName.ToLower();
            if (_context.Users.Any(x => x.UserName == lowercaseUsername))
                throw new AppException("Username \"" + user.UserName + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            //user.PasswordHash = passwordHash;
            //user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);

            //
            //add blank company
            //
            //user.Company = new Company();
            //user.Company.IsActive = false;

            _context.SaveChanges();

            await _emailHelper.SendWelcomeEmailAsync(user.Id);

            return user;
        }

        public async Task UpdateAsync(IDUser userParam, string password = null)
        {
            var user = await _context.Users
                .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                .SingleAsync(i => i.Id == userParam.Id);

            if (user == null)
                throw new AppException("User not found");

            if (userParam.UserName != user.UserName)
            {
                // username has changed so check if the new username is already taken
                if (_context.Users.Any(x => x.UserName == userParam.UserName))
                    throw new AppException("Username " + userParam.UserName + " is already taken");
            }

            // update user properties
            user.FirstName = userParam.FirstName;
            user.LastName = userParam.LastName;
            user.UserName = userParam.UserName;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                //user.PasswordHash = passwordHash;
                //user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users
                .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                .SingleAsync(i => i.Id == id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        // private helper methods

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        public async Task<bool> ValidateResetInput(string username, string token, int userId)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(username))
                        _logger.LogError("Missing username");
                    else
                    {
                        if (string.IsNullOrWhiteSpace(token))
                            _logger.LogError("Missing token");
                        else
                        {
                            if (userId == 0)
                                _logger.LogError("UserId is invalid");
                            else
                            {
                                var lowercaseUsername = username.ToLower();
                                //var user = await _context.Users
                                //.Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                //.FirstOrDefaultAsync(i => i.Id == userId && i.UserName == lowercaseUsername && i.PasswordResetToken == token);
                                //if (user != null)
                                //{
                                //if (user.PasswordRequestedOn.HasValue)
                                //{
                                //var timeSinceLink = DateTimeOffset.Now - user.PasswordRequestedOn.Value;
                                //if (timeSinceLink.TotalHours < 24.0)
                                //return true;
                                //}
                                //}

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return false;
            }
        }


    }
}