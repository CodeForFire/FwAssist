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
    [NotifyPropertyChangedFor(nameof(Spelled))]
    [NotifyPropertyChangedFor(nameof(HasSpelling))]
    private string _text = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TableName))]
    [NotifyPropertyChangedFor(nameof(IsAlt))]
    [NotifyPropertyChangedFor(nameof(IsDin2022))]
    [NotifyPropertyChangedFor(nameof(Spelled))]
    [NotifyPropertyChangedFor(nameof(HasSpelling))]
    [NotifyPropertyChangedFor(nameof(Entries))]
    private Buchstabiertafel _table = Buchstabiertafel.Alt;

    /// <summary>On screen at all times: the two tables give different words for every letter.</summary>
    public string TableName => IsAlt ? "ALT (DIN 5009:1983)" : "DIN 5009:2022";

    public bool IsAlt => ReferenceEquals(Table, Buchstabiertafel.Alt);

    public bool IsDin2022 => !IsAlt;

    public bool HasSpelling => Spelled.Count > 0;

    public IReadOnlyList<CodeWord> Spelled => Table.Spell(Text);

    public IReadOnlyList<CodeWord> Entries => Table.Entries;

    [RelayCommand]
    private void UseAlt() => Table = Buchstabiertafel.Alt;

    [RelayCommand]
    private void UseDin2022() => Table = Buchstabiertafel.Din2022;
}
