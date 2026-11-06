using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Helfer.Helpers.Funkalphabet;

/// <summary>
/// A word goes in, its code words come out, and the whole table stands below. Nothing here is worth
/// persisting: the text is typed and read out in the same breath.
/// </summary>
public sealed partial class FunkalphabetViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _text = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TableName))]
    [NotifyPropertyChangedFor(nameof(IsAlt))]
    [NotifyPropertyChangedFor(nameof(IsDin2022))]
    [NotifyPropertyChangedFor(nameof(Entries))]
    private Buchstabiertafel _table = Buchstabiertafel.Alt;

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

    partial void OnTableChanged(Buchstabiertafel value) => Respell();

    [RelayCommand]
    private void Clear() => Text = string.Empty;

    private void Respell() => Spelled = Table.Spell(Text ?? string.Empty);
}
