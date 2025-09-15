using DynamicBingo.Application.Interfaces;

namespace DynamicBingo.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    public async Task SendMagicLinkAsync(string email, string magicLink)
    {
        Console.WriteLine($"Sending magic link to {email}: {magicLink}");
        await Task.CompletedTask;
    }
}
