using System.Threading.Tasks;
using CbUpdate.Domain.Entities;
using CbUpdate.Domain.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CbUpdate.Domain.Services;

public class MailService : IMailService
{
    public MailService()
    {
    }

    // private readonly SecuritySettings _securitySettings;

    // public MailService(IOptions<SecuritySettings> securitySettings)
    // {
    //     _securitySettings = securitySettings.Value;
    // }

    public virtual Task SendPasswordResetMail(User user)
    {
        //TODO send reset Email
        return Task.FromResult(Task.CompletedTask);
    }

    public virtual Task SendActivationEmail(User user)
    {
        //TODO Activation Email
        return Task.FromResult(Task.CompletedTask);
    }

    public virtual Task SendCreationEmail(User user)
    {
        //TODO Creation Email
        return Task.FromResult(Task.CompletedTask);
    }
}
