namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>What Tabelle A says in place of the transport columns.</summary>
public enum StoffStatus
{
    /// <summary>An ordinary row: dangerous goods, carried under the ADR.</summary>
    Gefahrgut,

    /// <summary>"BEFÖRDERUNG VERBOTEN": the ADR forbids carrying this at all.</summary>
    BefoerderungVerboten,

    /// <summary>"UNTERLIEGT NICHT DEN VORSCHRIFTEN DES ADR", e.g. Trockeneis or Heu.</summary>
    NichtAdr,
}
