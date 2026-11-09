using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Services;
using FwAssist.Core;

namespace FwAssist.Helpers.Schaummittel;

/// <summary>
/// The Schaummittel-Rechner in both directions. VORRAT works out what the supplies make, RAUM what a
/// room needs. Everything is kept in <see cref="IHelperSettings"/> as it is typed: an Einsatz does
/// not wait for a phone that was killed in the background.
/// </summary>
public sealed partial class SchaummittelViewModel : ObservableObject
{
    private const string RaumKey = SchaummittelModule.HelperId + ".raum";
    private const string SupplyError = "Von 0,1 bis 1.000.000 l möglich.";
    private const string MeterError = "Von 0,1 bis 1.000 m möglich.";
    private const decimal MaxSupplyLiter = 1000000m;
    private const decimal MaxMeter = 1000m;
    private const decimal MinAmount = 0.1m;

    private readonly IHelperSettings _settings;

    public SchaummittelViewModel(IHelperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _isRaum = bool.TryParse(settings.Get(RaumKey), out var raum) && raum;

        Zumischrate = Field(new("zumischrate", "Zumischrate in %", MinAmount, 50m, "Von 0,1 bis 50 % möglich.", "3"));
        Verschaeumungszahl = Field(new("verschaeumungszahl", "Verschäumungszahl", 1m, 1000m, "Von 1 bis 1.000 möglich."));
        SchaummittelVorrat = Field(new("schaummittel", "Schaummittel in l", MinAmount, MaxSupplyLiter, SupplyError));
        WasserVorrat = Field(new("wasser", "Wasser in l (optional)", MinAmount, MaxSupplyLiter, SupplyError));
        Laenge = Field(new("laenge", "Länge in m", MinAmount, MaxMeter, MeterError));
        Breite = Field(new("breite", "Breite in m", MinAmount, MaxMeter, MeterError));
        Hoehe = Field(new("hoehe", "Höhe in m", MinAmount, MaxMeter, MeterError));
        Zuschlag = Field(new("zuschlag", "Zuschlag für Verluste in %", 0m, 500m, "Von 0 bis 500 % möglich.", "0"));
        Recalculate();
    }

    public NumberField Zumischrate { get; }

    public NumberField Verschaeumungszahl { get; }

    public NumberField SchaummittelVorrat { get; }

    public NumberField WasserVorrat { get; }

    public NumberField Laenge { get; }

    public NumberField Breite { get; }

    public NumberField Hoehe { get; }

    public NumberField Zuschlag { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVorrat))]
    private bool _isRaum;

    public bool IsVorrat
    {
        get => !IsRaum;
        set => IsRaum = !value;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private IReadOnlyList<ResultLine> _results = [];

    [ObservableProperty]
    private string _resultNote = string.Empty;

    public bool HasResults => Results.Count > 0;

    partial void OnIsRaumChanged(bool value)
    {
        _settings.Set(RaumKey, value.ToString(CultureInfo.InvariantCulture));
        Recalculate();
    }

    [RelayCommand]
    private void SetZumischrate(string percent) => Zumischrate.Text = percent;

    [RelayCommand]
    private void SetVerschaeumungszahl(string zahl) => Verschaeumungszahl.Text = zahl;

    /// <summary>The next job starts here: the amounts go, the foam in use stays.</summary>
    [RelayCommand]
    private void Reset()
    {
        SchaummittelVorrat.Text = string.Empty;
        WasserVorrat.Text = string.Empty;
        Laenge.Text = string.Empty;
        Breite.Text = string.Empty;
        Hoehe.Text = string.Empty;
        Zuschlag.Text = "0";
    }

    private static string Liter(decimal value) => $"{Formatting.Number(value, 0)} l";

    private static string Kubikmeter(decimal value) => $"{Formatting.Number(value, 1)} m³";

    private NumberField Field(NumberFieldRule rule) =>
        new(_settings, rule with { Key = $"{SchaummittelModule.HelperId}.{rule.Key}" }, Recalculate);

    private void Recalculate()
    {
        if (IsRaum)
        {
            ShowRaum();
        }
        else
        {
            ShowVorrat();
        }
    }

    private void ShowVorrat()
    {
        if (!WasserVorrat.IsValid
            || Zumischrate.Value is not { } zumisch
            || Verschaeumungszahl.Value is not { } zahl
            || SchaummittelVorrat.Value is not { } schaummittel)
        {
            Clear();
            return;
        }

        var ergebnis = SchaumRechner.Vorrat(schaummittel, WasserVorrat.Value, zumisch, zahl);
        ResultNote = ergebnis.Limit == Limit.Wasser
            ? "Reicht bis das Wasser leer ist."
            : "Reicht bis das Schaummittel leer ist.";
        Results =
        [
            new("Schaum", Kubikmeter(ergebnis.SchaumKubikmeter)),
            new("Gemisch", Liter(ergebnis.GemischLiter)),
            new("Schaummittel", Liter(ergebnis.SchaummittelLiter)),
            new("Wasser", Liter(ergebnis.WasserLiter)),
        ];
    }

    private void ShowRaum()
    {
        if (!Zuschlag.IsValid
            || Zumischrate.Value is not { } zumisch
            || Verschaeumungszahl.Value is not { } zahl
            || Laenge.Value is not { } laenge
            || Breite.Value is not { } breite
            || Hoehe.Value is not { } hoehe)
        {
            Clear();
            return;
        }

        var zuschlag = Zuschlag.Value ?? 0m;
        var ergebnis = SchaumRechner.Raum(laenge, breite, hoehe, zuschlag, zumisch, zahl);
        ResultNote = $"Mit {Formatting.Number(zuschlag, zuschlag.Scale)} % Zuschlag für Verluste.";
        Results =
        [
            new("Raum", Kubikmeter(ergebnis.VolumenKubikmeter)),
            new("Schaum", Kubikmeter(ergebnis.SchaumKubikmeter)),
            new("Gemisch", Liter(ergebnis.GemischLiter)),
            new("Schaummittel", Liter(ergebnis.SchaummittelLiter)),
            new("Wasser", Liter(ergebnis.WasserLiter)),
        ];
    }

    private void Clear()
    {
        ResultNote = string.Empty;
        Results = [];
    }
}

/// <summary>One line of the result: what, and how much, already formatted.</summary>
public sealed record ResultLine(string Label, string Value);
