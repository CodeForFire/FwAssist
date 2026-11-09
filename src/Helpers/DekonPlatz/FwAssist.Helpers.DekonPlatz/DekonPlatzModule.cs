using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.DekonPlatz;

public sealed class DekonPlatzModule : IHelperModule
{
    public const string HelperId = "dekonplatz";

    public string Id => HelperId;

    public string Title => "Dekon-Platz";

    public string Summary => "Dekon-Stufen I bis III: Maßnahmen, Aufbau und Technik.";

    // Drawn for this app: a tub with two drops above it. The launcher icon in the Android head draws
    // the same path.
    public string IconPathData => "M3,12 H21 V14 C21,18 18,20 15,20 H9 C6,20 3,18 3,14 Z M8,3 C8,3 5,7 5,8.5 A3,3 0 0 0 11,8.5 C11,7 8,3 8,3 Z M16,3 C16,3 13,7 13,8.5 A3,3 0 0 0 19,8.5 C19,7 16,3 16,3 Z";

    public ObservableObject CreateViewModel(HelperServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new DekonPlatzViewModel(services.Settings ?? new InMemoryHelperSettings());
    }
}
