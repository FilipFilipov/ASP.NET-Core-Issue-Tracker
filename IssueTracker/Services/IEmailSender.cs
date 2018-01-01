using System.Threading.Tasks;

namespace IssueTracker.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
