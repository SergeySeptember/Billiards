using Android.Content;
using Billiards.Abstractions;

namespace Billiards.Platforms.Android;

public sealed class AndroidExternalLinkService : IExternalLinkService
{
    public Task<bool> OpenUrlAsync(string url)
    {
        try
        {
            var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url));
            intent.AddFlags(ActivityFlags.NewTask);
            global::Android.App.Application.Context.StartActivity(intent);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
