namespace DynamicBingo.Application.Interfaces;

public interface IEmailSender
{
    Task SendMagicLinkAsync(string email, string magicLink);
}
