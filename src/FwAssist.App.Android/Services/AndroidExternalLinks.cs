using System.Diagnostics.CodeAnalysis;
using Android.Content;
using FwAssist.AppLogic.Services;

namespace FwAssist.App.Android.Services;

/// <summary>Opens a link in whichever browser the user has. Needs no permission.</summary>
internal sealed class AndroidExternalLinks : IExternalLinks
{
    private readonly Context _context;

    public AndroidExternalLinks(Context context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    [SuppressMessage(
        "Design",
        "CA1031",
        Justification = "No browser, or one that refuses, is an exception here; it surfaces as AboutViewModel.Error through the false return.")]
    public bool TryOpen(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        try
        {
            using var target = global::Android.Net.Uri.Parse(uri.AbsoluteUri);
            using var intent = new Intent(Intent.ActionView, target);

            // The application context has no task of its own to start the browser in.
            intent.AddFlags(ActivityFlags.NewTask);
            _context.StartActivity(intent);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
