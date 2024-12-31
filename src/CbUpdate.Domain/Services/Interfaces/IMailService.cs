using System.Threading.Tasks;
using CbUpdate.Domain.Entities;

namespace CbUpdate.Domain.Services.Interfaces;

public interface IMailService
{
    Task SendPasswordResetMail(User user);
    Task SendActivationEmail(User user);
    Task SendCreationEmail(User user);
}
