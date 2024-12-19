using System.Collections.Generic;
using System.Threading.Tasks;

namespace UpRaise.Services
{
    public interface IEmail
    {
        Task SendEmailAsync(string toAddress, string subject, string message, bool isHighPriority = false);
        Task SendEmailAsync(List<string> email, string subject, string message, bool isHighPriority = false);
    }

}