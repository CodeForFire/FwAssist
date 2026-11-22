namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>
/// One marking as the screen shows it. <see cref="PictureKey"/> names its drawing in the Ui
/// project. <see cref="Look"/> says in words what the picture shows, so the meaning never hangs on
/// telling red from orange in daylight. <see cref="Source"/> names the paragraph the entry rests on.
/// </summary>
public sealed record Kennzeichen(
    KennzeichenGruppe Gruppe,
    string PictureKey,
    string Code,
    string Name,
    string Look,
    string Meaning,
    string Source);
