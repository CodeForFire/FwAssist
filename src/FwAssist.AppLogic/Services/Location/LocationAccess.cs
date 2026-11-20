namespace FwAssist.AppLogic.Services.Location;

/// <summary>What came of asking for the phone's position.</summary>
public enum LocationAccess
{
    /// <summary>Fixes will arrive through <see cref="ILocationSource.FixChanged"/>.</summary>
    Granted,

    /// <summary>The user refused the location permission.</summary>
    Denied,

    /// <summary>The permission is there, but location is switched off on the phone.</summary>
    SwitchedOff,
}
