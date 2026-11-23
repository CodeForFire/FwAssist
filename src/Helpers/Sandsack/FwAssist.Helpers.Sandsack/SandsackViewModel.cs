using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.ViewModels;
using FwAssist.Core;

namespace FwAssist.Helpers.Sandsack;

/// <summary>
/// What a Sandsackdamm of a given length and height takes: sacks, sand, and with the Helfer at hand,
/// the time. Everything is kept in <see cref="IHelperSettings"/> as it is typed: an Einsatz does not
/// wait for a phone that was killed in the background.
/// </summary>
public sealed partial class SandsackViewModel : ObservableObject
{
    private const string Ratschlag =
        "Dichter Verbund, Folie nur wasserseitig. Fachberater Deichverteidigung hinzuziehen.";

    private static readonly string MaxHoeheText = Formatting.Number(SandsackRechner.MaxHoeheMeter, 1);

    private readonly IHelperSettings _settings;

    public SandsackViewModel(IHelperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;

        Laenge = Field(new("laenge", "Länge des Damms in m", 1m, 10000m, "Von 1 bis 10.000 m möglich."));
        Hoehe = Field(new("hoehe", "Höhe des Damms in m", 0.1m, SandsackRechner.MaxHoeheMeter, $"Von 0,1 bis {MaxHoeheText} m möglich."));
        Helfer = Field(new("helfer", "Anzahl Helfer", 1m, 1000m, "Von 1 bis 1.000 möglich."));
        Recalculate();
    }

    public NumberField Laenge { get; }

    public NumberField Hoehe { get; }

    public NumberField Helfer { get; }

    public static string Quellen => Sandsackvorgaben.Quellen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private IReadOnlyList<ResultLine> _results = [];

    public bool HasResults => Results.Count > 0;

    /// <summary>Which row of the table was used, how wide the dam has to be, and how to build it.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary>How the results are calculated, one step per line, with the values entered so far (#87).</summary>
    [ObservableProperty]
    private IReadOnlyList<string> _rechenweg = [];

    [RelayCommand]
    private void Reset()
    {
        Laenge.Text = string.Empty;
        Hoehe.Text = string.Empty;
        Helfer.Text = string.Empty;
    }

    private NumberField Field(NumberFieldRule rule) =>
        new(_settings, rule with { Key = $"{SandsackModule.HelperId}.{rule.Key}" }, Recalculate);

    private void Recalculate()
    {
        Rechenweg = SandsackRechenweg.Zeilen(Laenge.Value, Hoehe.Value, Helfer.Value);
        if (Hoehe.Value is not { } hoehe)
        {
            Hinweis = Ratschlag;
            Results = [];
            return;
        }

        var stufe = SandsackRechner.Stufe(hoehe);
        Hinweis = $"Gerechnet für {Formatting.Number(stufe.HoeheMeter, 1)} m Höhe. "
            + $"Basisbreite mindestens {Formatting.Number(SandsackRechner.Basisbreite(hoehe), 1)} m. "
            + Ratschlag;

        if (Laenge.Value is not { } laenge)
        {
            Results = [];
            return;
        }

        var saecke = SandsackRechner.Saecke(laenge, stufe);
        var sand = SandsackRechner.SandKubikmeter(laenge, stufe);
        List<ResultLine> lines =
        [
            new("Sandsäcke", Formatting.Number(saecke, 0)),
            new("Sand", $"{Formatting.Number(sand, 1)} m³"),
            new("Sand, nass", $"{Formatting.Number(SandsackRechner.SandTonnen(sand), 1)} t"),
        ];

        if (Helfer.Value is { } helfer)
        {
            lines.Add(new("Arbeitszeit", SandsackRechenweg.Dauer(SandsackRechner.Arbeitszeit(saecke, helfer))));
        }

        Results = lines;
    }
}

/// <summary>One line of the result: what, and how much, already formatted.</summary>
public sealed record ResultLine(string Label, string Value);
