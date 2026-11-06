using CommunityToolkit.Mvvm.ComponentModel;
using Helfer.AppLogic.Helpers;
using Helfer.AppLogic.Services;

namespace Helfer.Helpers.RthLandeplatz;

public sealed class RthLandeplatzModule : IHelperModule
{
    public const string HelperId = "rthlandeplatz";

    public string Id => HelperId;

    public string Title => "RTH-Landeplatz";

    public string Summary => "Vorgaben für den Landeplatz, bei Tag und bei Nacht.";

    // Drawn for this app: a helicopter from the side, with its rotor above the cabin, a window cut
    // out and a skid below, filled even-odd. The launcher icon in the Android head draws the same path.
    public string IconPathData => "F0 M2,4 H22 V6 H13 V8 H15 C18,8 19,11 19.5,12 H23 V9 H24 V16 H23 V14 H19 C18,17 15,18 12,18 H6 C3,18 2,15 2,13 C2,10 4,8 7,8 H11 V6 H2 Z M6,10 C5,10 4,11 4,13 H9 V10 Z M4,20 H18 V22 H4 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new RthLandeplatzViewModel(services.Settings ?? new InMemoryHelperSettings());
    }
}
