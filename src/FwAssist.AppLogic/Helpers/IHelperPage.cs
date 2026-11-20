namespace FwAssist.AppLogic.Helpers;

/// <summary>
/// A helper's view model that has to know when the user leaves its page, to stop what only runs
/// while it is on screen, such as the GPS. The shell keeps the view model, so it is not disposed.
/// </summary>
public interface IHelperPage
{
    /// <summary>The user went to another page. Called once per leaving.</summary>
    void Leave();
}
