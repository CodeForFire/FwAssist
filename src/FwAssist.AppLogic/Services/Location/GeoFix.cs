using FwAssist.Core.Map;

namespace FwAssist.AppLogic.Services.Location;

/// <summary>Where the phone is, as its location service last said.</summary>
/// <param name="Position">The position, WGS84.</param>
/// <param name="AccuracyMetres">How far off it may be, in metres (68 % radius, as Android reports it).</param>
/// <param name="Time">When the fix arrived, by the app's clock.</param>
public sealed record GeoFix(GeoPoint Position, double AccuracyMetres, DateTimeOffset Time);
