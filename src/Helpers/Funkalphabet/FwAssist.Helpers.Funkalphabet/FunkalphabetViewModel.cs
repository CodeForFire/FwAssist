using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.Funkalphabet;

/// <summary>
/// A word goes in, its code words come out, and the whole table stands below. The table choice is
/// kept in <see cref="IHelperSettings"/>, because an organisation picks it once. The text is not: it
/// is typed and read out in the same breath.
/// </summary>
public sealed partial class FunkalphabetViewModel : ObservableObject
{
    private const string TableKey = FunkalphabetModule.HelperId + ".table";

    // The stored values are permanent: "alt" stays although the screen now says BOS.
    private const string AltValue = "alt";
    private const string Din2022Value = "din2022";
    private const string NatoValue = "nato";

    private const string CodeWordSeparator = " - ";
    private const string WordSeparator = " / ";

    private readonly IHelperSettings _settings;

    public FunkalphabetViewModel(IHelperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _table = settings.Get(TableKey) switch
        {
            Din2022Value => Buchstabiertafel.Din2022,
            NatoValue => Buchstabiertafel.Nato,
            _ => Buchstabiertafel.Alt,
        };
    }

    [ObservableProperty]
    private string? _text = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TableName))]
    [NotifyPropertyChangedFor(nameof(IsAlt))]
    [NotifyPropertyChangedFor(nameof(IsDin2022))]
    [NotifyPropertyChangedFor(nameof(IsNato))]
    [NotifyPropertyChangedFor(nameof(Entries))]
    private Buchstabiertafel _table;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSpelling))]
    [NotifyPropertyChangedFor(nameof(SpelledText))]
    private IReadOnlyList<CodeWord> _spelled = [];

    /// <summary>On screen at all times: the tables give different words for every letter.</summary>
    public string TableName => IsAlt ? "BOS (DIN 5009:1983)" : IsNato ? "NATO" : "DIN 5009:2022";

    // A setter acts on true only. With three RadioButtons, the one losing its check says false
    // while the one gaining it says true, and false names no table.
    public bool IsAlt
    {
        get => ReferenceEquals(Table, Buchstabiertafel.Alt);
        set => SelectWhen(value, Buchstabiertafel.Alt);
    }

    public bool IsDin2022
    {
        get => ReferenceEquals(Table, Buchstabiertafel.Din2022);
        set => SelectWhen(value, Buchstabiertafel.Din2022);
    }

    public bool IsNato
    {
        get => ReferenceEquals(Table, Buchstabiertafel.Nato);
        set => SelectWhen(value, Buchstabiertafel.Nato);
    }

    public bool HasSpelling => Spelled.Count > 0;

    /// <summary>The spelling as one line that wraps: " - " between code words, " / " between typed words.</summary>
    public string SpelledText
    {
        get
        {
            var line = new StringBuilder();
            CodeWord? previous = null;
            foreach (var word in Spelled)
            {
                if (word != CodeWord.Gap)
                {
                    if (previous is not null)
                    {
                        line.Append(previous == CodeWord.Gap ? WordSeparator : CodeWordSeparator);
                    }

                    line.Append(word.Word);
                }

                previous = word;
            }

            return line.ToString();
        }
    }

    public IReadOnlyList<CodeWord> Entries => Table.Entries;

    partial void OnTextChanged(string? value) => Respell();

    partial void OnTableChanged(Buchstabiertafel value)
    {
        _settings.Set(TableKey, IsAlt ? AltValue : IsNato ? NatoValue : Din2022Value);
        Respell();
    }

    [RelayCommand]
    private void Clear() => Text = string.Empty;

    private void SelectWhen(bool selected, Buchstabiertafel table)
    {
        if (selected)
        {
            Table = table;
        }
    }

    private void Respell() => Spelled = Table.Spell(Text ?? string.Empty);
}
