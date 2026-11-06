using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Helfer.AppLogic.Services;
using Helfer.Core;

namespace Helfer.Helpers.Schaummittel;

/// <summary>
/// One input: the text as typed, kept across a process death, and what it means. An empty field is
/// not an error, it is just not filled in yet.
/// </summary>
public sealed class NumberField : ObservableObject
{
    private readonly IHelperSettings _settings;
    private readonly NumberFieldRule _rule;
    private readonly Action _changed;
    private string _text;

    public NumberField(IHelperSettings settings, NumberFieldRule rule, Action changed)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(changed);
        _settings = settings;
        _rule = rule;
        _changed = changed;
        _text = settings.Get(rule.Key) ?? rule.Default;
        Evaluate();
    }

    public string Label => _rule.Label;

    /// <summary>Never null: a two-way binding may write null back, and that reads as empty.</summary>
    [AllowNull]
    public string Text
    {
        get => _text;
        set
        {
            if (SetProperty(ref _text, value ?? string.Empty))
            {
                _settings.Set(_rule.Key, _text);
                Evaluate();
                OnPropertyChanged(nameof(Error));
                OnPropertyChanged(nameof(IsValid));
                _changed();
            }
        }
    }

    /// <summary>The German reason this input cannot be used, or <see langword="null"/>.</summary>
    public string? Error { get; private set; }

    /// <summary>The number, or <see langword="null"/> when the field is empty or wrong.</summary>
    public decimal? Value { get; private set; }

    public bool IsValid => Error is null;

    private void Evaluate()
    {
        Value = null;
        Error = null;
        if (string.IsNullOrWhiteSpace(_text))
        {
            return;
        }

        if (!Formatting.TryParseNumber(_text, out var number))
        {
            Error = "Zahl eingeben.";
        }
        else if (number < _rule.Minimum || number > _rule.Maximum)
        {
            Error = _rule.RangeError;
        }
        else
        {
            Value = number;
        }
    }
}

/// <summary>What a <see cref="NumberField"/> accepts. Minimum and maximum are both allowed.</summary>
public sealed record NumberFieldRule(
    string Key,
    string Label,
    decimal Minimum,
    decimal Maximum,
    string RangeError,
    string Default = "");
