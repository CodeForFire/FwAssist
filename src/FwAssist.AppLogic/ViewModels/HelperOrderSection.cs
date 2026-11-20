using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;

namespace FwAssist.AppLogic.ViewModels;

/// <summary>
/// The order of the helpers on the overview, changed in Einstellungen. It is kept in
/// <see cref="IHelperSettings"/> as the helpers' ids, so it survives a process death and an update.
/// </summary>
public sealed partial class HelperOrderSection : ObservableObject
{
    /// <summary>
    /// The settings key. It belongs to the shell, not a helper, so it starts with <c>shell</c>,
    /// which no helper id is.
    /// </summary>
    public const string SettingsKey = "shell.order";

    private readonly IReadOnlyList<IHelperModule> _defaults;
    private readonly IHelperSettings _settings;
    private readonly Action<IReadOnlyList<string>> _changed;
    private readonly ObservableCollection<HelperOrderRow> _rows;

    /// <param name="modules">Every helper, in the default order.</param>
    /// <param name="settings">Where the order is kept.</param>
    /// <param name="changed">Hears the new order, as ids, after every change.</param>
    public HelperOrderSection(IReadOnlyList<IHelperModule> modules, IHelperSettings settings, Action<IReadOnlyList<string>> changed)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(changed);
        _defaults = modules;
        _settings = settings;
        _changed = changed;
        _rows = new(Arrange(modules, settings.Get(SettingsKey)).Select(m => new HelperOrderRow(m.Id, m.Title, this)));
        Rows = new(_rows);
        UpdateEnds();
    }

    public ReadOnlyObservableCollection<HelperOrderRow> Rows { get; }

    /// <summary>The helpers' ids, in the order the user chose.</summary>
    public IReadOnlyList<string> Ids => _rows.Select(r => r.Id).ToList();

    /// <summary>
    /// The modules in the stored order. A stored id no module claims any more is skipped, and a
    /// module the stored order does not know yet, one new in an update, goes at the end.
    /// </summary>
    public static IReadOnlyList<IHelperModule> Arrange(IReadOnlyList<IHelperModule> modules, string? stored)
    {
        ArgumentNullException.ThrowIfNull(modules);
        if (string.IsNullOrEmpty(stored))
        {
            return modules;
        }

        var byId = modules.ToDictionary(m => m.Id, StringComparer.Ordinal);
        var arranged = new List<IHelperModule>(modules.Count);
        foreach (var id in stored.Split(','))
        {
            if (byId.Remove(id, out var module))
            {
                arranged.Add(module);
            }
        }

        arranged.AddRange(modules.Where(m => byId.ContainsKey(m.Id)));
        return arranged;
    }

    /// <summary>Back to the order the app ships with. Stores nothing, so a later new helper keeps its own place.</summary>
    [RelayCommand]
    private void Reset()
    {
        for (var i = 0; i < _defaults.Count; i++)
        {
            var from = IndexOf(_defaults[i].Id);
            if (from != i)
            {
                _rows.Move(from, i);
            }
        }

        _settings.Set(SettingsKey, string.Empty);
        Changed();
    }

    internal void Move(HelperOrderRow row, int by)
    {
        var from = _rows.IndexOf(row);
        var to = from + by;
        if (from < 0 || to < 0 || to >= _rows.Count)
        {
            return;
        }

        _rows.Move(from, to);
        _settings.Set(SettingsKey, string.Join(',', Ids));
        Changed();
    }

    private void Changed()
    {
        UpdateEnds();
        _changed(Ids);
    }

    private int IndexOf(string id)
    {
        for (var i = 0; i < _rows.Count; i++)
        {
            if (string.Equals(_rows[i].Id, id, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private void UpdateEnds()
    {
        for (var i = 0; i < _rows.Count; i++)
        {
            _rows[i].SetPlace(isFirst: i == 0, isLast: i == _rows.Count - 1);
        }
    }
}
