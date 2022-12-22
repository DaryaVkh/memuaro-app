namespace Rememory.Email;

public interface IEmailClient
{
    public Task<bool> SendMessage(string email, string message);
}