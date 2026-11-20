using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.Services.Map;

namespace FwAssist.AppLogic.Helpers;

/// <summary>
/// The platform services a helper may use. Wired by hand in the composition root; a helper that
/// needs something new gets a new nullable parameter here, not a container.
/// </summary>
/// <param name="Time">The clock.</param>
/// <param name="Ticker">What drives a running display.</param>
/// <param name="Settings">Small values kept across a process death; null keeps nothing.</param>
/// <param name="Maps">The Kartenpakete on this device; null where there are none to be had.</param>
public sealed record HelperServices(TimeProvider Time, ITicker Ticker, IHelperSettings? Settings = null, IRegionPackLibrary? Maps = null);
