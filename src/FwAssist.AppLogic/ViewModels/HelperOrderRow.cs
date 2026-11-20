using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FwAssist.AppLogic.ViewModels;

/// <summary>One helper in Einstellungen' order, with a button to move it up and one to move it down.</summary>
public sealed partial class HelperOrderRow : ObservableObject
{
    private readonly HelperOrderSection _owner;

    public HelperOrderRow(string id, string title, HelperOrderSection owner)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(owner);
        Id = id;
        Title = title;
        _owner = owner;
    }

    public string Id { get; }

    public string Title { get; }

    /// <summary>What a screen reader says for the button that moves this helper up.</summary>
    public string MoveUpName => $"{Title} nach oben";

    /// <summary>What a screen reader says for the button that moves this helper down.</summary>
    public string MoveDownName => $"{Title} nach unten";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveUpCommand))]
    private bool _canMoveUp;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveDownCommand))]
    private bool _canMoveDown;

    internal void SetPlace(bool isFirst, bool isLast)
    {
        CanMoveUp = !isFirst;
        CanMoveDown = !isLast;
    }

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveUp() => _owner.Move(this, -1);

    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveDown() => _owner.Move(this, 1);
}
