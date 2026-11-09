using CommunityToolkit.Mvvm.ComponentModel;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.DekonPlatz;

/// <summary>
/// A reference with three stages. The choice is kept in <see cref="IHelperSettings"/>, because a crew
/// reading one Stufe must not land on another after a process death.
/// </summary>
public sealed partial class DekonPlatzViewModel : ObservableObject
{
    private const string StufeKey = DekonPlatzModule.HelperId + ".stufe";

    private readonly IHelperSettings _settings;

    public DekonPlatzViewModel(IHelperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _stufe = Enum.TryParse<DekonStufe>(settings.Get(StufeKey), out var stored) && Enum.IsDefined(stored)
            ? stored
            : DekonStufe.Eins;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStufeEins))]
    [NotifyPropertyChangedFor(nameof(IsStufeZwei))]
    [NotifyPropertyChangedFor(nameof(IsStufeDrei))]
    [NotifyPropertyChangedFor(nameof(Sections))]
    private DekonStufe _stufe;

    // A RadioButton also writes false when another one is chosen, and that must not change the Stufe.
    public bool IsStufeEins
    {
        get => Stufe == DekonStufe.Eins;
        set => Choose(value, DekonStufe.Eins);
    }

    public bool IsStufeZwei
    {
        get => Stufe == DekonStufe.Zwei;
        set => Choose(value, DekonStufe.Zwei);
    }

    public bool IsStufeDrei
    {
        get => Stufe == DekonStufe.Drei;
        set => Choose(value, DekonStufe.Drei);
    }

    /// <summary>Only this Stufe's values: a size for one Stufe can never be read as another's.</summary>
    public IReadOnlyList<Section> Sections => Stufe switch
    {
        DekonStufe.Zwei => Dekonvorgaben.StufeZwei,
        DekonStufe.Drei => Dekonvorgaben.StufeDrei,
        _ => Dekonvorgaben.StufeEins,
    };

    partial void OnStufeChanged(DekonStufe value) => _settings.Set(StufeKey, value.ToString());

    private void Choose(bool isChosen, DekonStufe stufe)
    {
        if (isChosen)
        {
            Stufe = stufe;
        }
    }
}
