using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.ViewModels;
using FwAssist.Core;

namespace FwAssist.Helpers.Strahlenschutz;

/// <summary>
/// Three calculations from one value: what a TKZ means, the dose rate at another distance, and
/// where the Absperrgrenze falls. Everything is kept in <see cref="IHelperSettings"/> as it is
/// typed: an Einsatz does not wait for a phone that was killed in the background.
/// </summary>
public sealed partial class StrahlenschutzViewModel : ObservableObject
{
    private const string ModusKey = StrahlenschutzModule.HelperId + ".modus";
    private const string MeterError = "Von 0,1 bis 10.000 m möglich.";
    private const decimal MinMeter = 0.1m;
    private const decimal MaxMeter = 10000m;
    private const decimal MikroJeMilli = 1000m;
    private const decimal Kleinstwert = 0.01m;

    private static readonly string GrenzwertText =
        Strahlenschutzvorgaben.GrenzwertGefahrenbereich.ToString(CultureInfo.InvariantCulture);

    private static readonly string BezugsabstandText =
        Strahlenschutzvorgaben.TkzBezugsabstandMeter.ToString(CultureInfo.InvariantCulture);

    private readonly IHelperSettings _settings;

    public StrahlenschutzViewModel(IHelperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _modus = Enum.TryParse<Modus>(settings.Get(ModusKey), out var modus) && Enum.IsDefined(modus)
            ? modus
            : Modus.Tkz;

        Tkz = Field(new("tkz", "Transportkennzahl (TKZ)", 0m, 1000m, "Von 0 bis 1.000 möglich."));
        TkzAbstand = Field(new("tkz.abstand", "Abstand zum Versandstück in m", MinMeter, MaxMeter, MeterError, BezugsabstandText));
        Dosisleistung = Field(new("dosisleistung", "Gemessen in µSv/h", Kleinstwert, 10000000m, "Von 0,01 bis 10.000.000 µSv/h möglich."));
        Messabstand = Field(new("messabstand", "Messabstand in m", MinMeter, MaxMeter, MeterError));
        Zielabstand = Field(new("zielabstand", "Neuer Abstand in m", MinMeter, MaxMeter, MeterError));
        Grenzwert = Field(new("grenzwert", "Grenzwert in µSv/h", Kleinstwert, 100000m, "Von 0,01 bis 100.000 µSv/h möglich.", GrenzwertText));
        Recalculate();
    }

    public NumberField Tkz { get; }

    public NumberField TkzAbstand { get; }

    public NumberField Dosisleistung { get; }

    public NumberField Messabstand { get; }

    public NumberField Zielabstand { get; }

    public NumberField Grenzwert { get; }

    /// <summary>The value the 25 µSv/h preset sets, as its button reads it.</summary>
    public static string GrenzwertPreset => GrenzwertText;

    public static string Quellen => Strahlenschutzvorgaben.Quellen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTkz), nameof(IsAbstand), nameof(IsAbsperrung), nameof(Hinweis))]
    private Modus _modus;

    public bool IsTkz
    {
        get => Modus == Modus.Tkz;
        set => Select(Modus.Tkz, value);
    }

    public bool IsAbstand
    {
        get => Modus == Modus.Abstand;
        set => Select(Modus.Abstand, value);
    }

    public bool IsAbsperrung
    {
        get => Modus == Modus.Absperrung;
        set => Select(Modus.Absperrung, value);
    }

    /// <summary>What the result of this calculation does not know.</summary>
    public string Hinweis => Modus switch
    {
        Modus.Tkz => "Gilt nur für ein unbeschädigtes Versandstück. TKZ 0 zählt als 0,05.",
        Modus.Abstand => PunktQuelle,
        _ => $"{PunktQuelle} Gefahrenbereich mindestens {Formatting.Number(Strahlenschutzvorgaben.MindestGefahrenbereichMeter, 0)} m.",
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private IReadOnlyList<ResultLine> _results = [];

    public bool HasResults => Results.Count > 0;

    private static string PunktQuelle =>
        "Gilt für eine punktförmige Quelle. Nah an großen Quellen fällt die Dosisleistung langsamer.";

    partial void OnModusChanged(Modus value)
    {
        _settings.Set(ModusKey, value.ToString());
        Recalculate();
    }

    [RelayCommand]
    private void SetGrenzwert(string wert) => Grenzwert.Text = wert;

    /// <summary>The next measurement starts here: the values go, the Grenzwert in use stays.</summary>
    [RelayCommand]
    private void Reset()
    {
        Tkz.Text = string.Empty;
        TkzAbstand.Text = BezugsabstandText;
        Dosisleistung.Text = string.Empty;
        Messabstand.Text = string.Empty;
        Zielabstand.Text = string.Empty;
    }

    /// <summary>A dose rate as read: mSv/h from 1.000 µSv/h on, two significant figures either way.</summary>
    private static string Dosis(decimal mikroProStunde)
    {
        if (mikroProStunde < Kleinstwert)
        {
            return $"unter {Formatting.Number(Kleinstwert, 2)} µSv/h";
        }

        return mikroProStunde >= MikroJeMilli
            ? $"{Zahl(mikroProStunde / MikroJeMilli)} mSv/h"
            : $"{Zahl(mikroProStunde)} µSv/h";
    }

    // Two significant figures need one decimal below 10, two below 1, and so on.
    private static string Zahl(decimal value)
    {
        var decimals = 0;
        for (var limit = 10m; value < limit && decimals < 3; limit /= 10m)
        {
            decimals++;
        }

        return Formatting.Number(value, decimals);
    }

    /// <summary>A distance as typed: "2,5 m" stays 2,5, never rounded to what the user did not type.</summary>
    private static string Meter(decimal value) => $"{Formatting.Number(value, value.Scale)} m";

    private void Select(Modus modus, bool selected)
    {
        if (selected)
        {
            Modus = modus;
        }
    }

    private NumberField Field(NumberFieldRule rule) =>
        new(_settings, rule with { Key = $"{StrahlenschutzModule.HelperId}.{rule.Key}" }, Recalculate);

    private void Recalculate() => Results = Modus switch
    {
        Modus.Tkz => ShowTkz(),
        Modus.Abstand => ShowAbstand(),
        _ => ShowAbsperrung(),
    };

    private IReadOnlyList<ResultLine> ShowTkz()
    {
        if (Tkz.Value is not { } tkz || TkzAbstand.Value is not { } abstand)
        {
            return [];
        }

        var bezug = Strahlenschutzvorgaben.TkzBezugsabstandMeter;
        var amBezug = new ResultLine($"In {Meter(bezug)}", Dosis(StrahlenRechner.Tkz(tkz, bezug)));
        return abstand == bezug
            ? [amBezug]
            : [amBezug, new($"In {Meter(abstand)}", Dosis(StrahlenRechner.Tkz(tkz, abstand)))];
    }

    private IReadOnlyList<ResultLine> ShowAbstand()
    {
        if (Dosisleistung.Value is not { } dosis
            || Messabstand.Value is not { } mess
            || Zielabstand.Value is not { } ziel)
        {
            return [];
        }

        return [new($"In {Meter(ziel)}", Dosis(StrahlenRechner.Abstand(dosis, mess, ziel)))];
    }

    private IReadOnlyList<ResultLine> ShowAbsperrung()
    {
        if (Dosisleistung.Value is not { } dosis
            || Messabstand.Value is not { } mess
            || Grenzwert.Value is not { } grenzwert)
        {
            return [];
        }

        var grenze = StrahlenRechner.Absperrgrenze(dosis, mess, grenzwert);
        return [new("Absperrgrenze", $"{Formatting.Number(grenze, 0)} m")];
    }
}

/// <summary>Which of the three calculations is on screen.</summary>
public enum Modus
{
    Tkz,
    Abstand,
    Absperrung,
}

/// <summary>One line of the result: what, and how much, already formatted.</summary>
public sealed record ResultLine(string Label, string Value);
