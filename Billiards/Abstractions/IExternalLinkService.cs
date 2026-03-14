namespace Billiards.Abstractions;

public interface IExternalLinkService
{
    Task<bool> OpenUrlAsync(string url);
}
