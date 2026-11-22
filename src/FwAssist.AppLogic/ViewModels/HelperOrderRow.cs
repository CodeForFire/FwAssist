namespace FwAssist.AppLogic.ViewModels;

/// <summary>One helper in Einstellungen' order, with a handle to drag it to another place.</summary>
public sealed class HelperOrderRow
{
    public HelperOrderRow(string id, string title)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(title);
        Id = id;
        Title = title;
    }

    public string Id { get; }

    public string Title { get; }

    /// <summary>What a screen reader says for the drag handle.</summary>
    public string DragName => $"{Title} verschieben";
}
