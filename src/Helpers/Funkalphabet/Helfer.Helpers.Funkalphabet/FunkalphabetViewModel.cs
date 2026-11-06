using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helfer.AppLogic.Services;

namespace Helfer.Helpers.Funkalphabet;

/// <summary>
/// A word goes in, its code words come out, and the whole table stands below. The table choice is
/// kept in <see cref="IHelperSettings"/>, because an organisation picks it once. The text is not: it
/// is typed and read out in the same breath.
/// </summary>
public sealed partial class FunkalphabetViewModel : ObservableObject
{
    private const string TableKey = FunkalphabetModule.HelperId + ".table";
    private const string AltValue = "alt";
    private const string Din2022Value = "din2022";

    private readonly IHelperSettings _settings;

    public FunkalphabetViewModel(IHelperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _table = settings.Get(TableKey) == Din2022Value ? Buchstabiertafel.Din2022 : Buchstabiertafel.Alt;
    }

    [ObservableProperty]
    private string? _text = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TableName))]
    [NotifyPropertyChangedFor(nameof(IsAlt))]
    [NotifyPropertyChangedFor(nameof(IsDin2022))]
    [NotifyPropertyChangedFor(nameof(Entries))]
    private Buchstabiertafel _table;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSpelling))]
    private IReadOnlyList<CodeWord> _spelled = [];

    /// <summary>On screen at all times: the two tables give different words for every letter.</summary>
    public string TableName => IsAlt ? "ALT (DIN 5009:1983)" : "DIN 5009:2022";

    public bool IsAlt
    {
        get => ReferenceEquals(Table, Buchstabiertafel.Alt);
        set => Table = value ? Buchstabiertafel.Alt : Buchstabiertafel.Din2022;
    }

    public bool IsDin2022
    {
        get => !IsAlt;
        set => IsAlt = !value;
    }

    public bool HasSpelling => Spelled.Count > 0;

    public IReadOnlyList<CodeWord> Entries => Table.Entries;

    partial void OnTextChanged(string? value) => Respell();

    partial void OnTableChanged(Buchstabiertafel value)
    {
        _settings.Set(TableKey, IsAlt ? AltValue : Din2022Value);
        Respell();
    }

    [RelayCommand]
    private void Clear() => Text = string.Empty;

    private void Respell() => Spelled = Table.Spell(Text ?? string.Empty);
}
