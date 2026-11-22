using FwAssist.AppLogic.Services.Map;
using FwAssist.Core;

namespace FwAssist.Helpers.Hydranten;

/// <summary>One hydrant in the list of the nearest, with how far it is from the Einsatzstelle.</summary>
/// <param name="Point">The hydrant.</param>
/// <param name="Metres">The Luftlinie from the Einsatzstelle, in whole metres.</param>
public sealed record HydrantRow(WaterPoint Point, int Metres)
{
    /// <summary>How many B-Schläuche reach it, by <see cref="Schlauchbedarf.BSchlaeuche"/>.</summary>
    public int BSchlaeuche => Schlauchbedarf.BSchlaeuche(Metres);

    /// <summary>"Unterflurhydrant", or what else OSM says it is.</summary>
    public string Bauart => Point.Title;

    /// <summary>"120 m Luftlinie · 6 B-Schläuche".</summary>
    public string Text =>
        $"{Formatting.Number(Metres)} m Luftlinie · {Formatting.Number(BSchlaeuche)} {(BSchlaeuche == 1 ? "B-Schlauch" : "B-Schläuche")}";
}
