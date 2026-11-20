using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.RthLandeplatz;

/// <summary>
/// A reference with two modes. The choice is set by hand: working it out from sunset would need the
/// location, and so a permission. It is kept in <see cref="IHelperSettings"/>, because a crew reading
/// the night values must not fall back to the day values after a process death.
/// </summary>
public sealed partial class RthLandeplatzViewModel : ObservableObject
{
    private const string NachtKey = RthLandeplatzModule.HelperId + ".nacht";

    private readonly IHelperSettings _settings;

    public RthLandeplatzViewModel(IHelperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _isNacht = bool.TryParse(settings.Get(NachtKey), out var nacht) && nacht;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTag))]
    [NotifyPropertyChangedFor(nameof(Sections))]
    [NotifyPropertyChangedFor(nameof(ModeHeading))]
    [NotifyPropertyChangedFor(nameof(ModeDefinition))]
    private bool _isNacht;

    public bool IsTag
    {
        get => !IsNacht;
        set => IsNacht = !value;
    }

    /// <summary>Only this mode's values: a night size can never be read as a day size.</summary>
    public IReadOnlyList<Section> Sections => IsNacht ? Landeplatzvorgaben.Nacht : Landeplatzvorgaben.Tag;

    public string ModeHeading => IsNacht ? "NACHT" : "TAG";

    public string ModeDefinition => IsNacht ? Landeplatzvorgaben.NightDefinition : Landeplatzvorgaben.DayDefinition;

    partial void OnIsNachtChanged(bool value) =>
        _settings.Set(NachtKey, value.ToString(CultureInfo.InvariantCulture));
}
