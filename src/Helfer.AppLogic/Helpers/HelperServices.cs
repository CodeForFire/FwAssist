using Helfer.AppLogic.Services;

namespace Helfer.AppLogic.Helpers;

/// <summary>
/// The platform services a helper may use. Wired by hand in the composition root; a helper that
/// needs something new gets a new nullable parameter here, not a container.
/// </summary>
public sealed record HelperServices(TimeProvider Time, ITicker Ticker, IHelperSettings? Settings = null);
