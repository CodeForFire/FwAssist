namespace FwAssist.Helpers.RthLandeplatz;

/// <summary>
/// What the screen says, per mode. The wording is our own and states the sources' facts in new
/// sentences. Every line names its source, and issue #26 records how they were reconciled:
/// <list type="bullet">
/// <item>vfdb: vfdb-Merkblatt 06/02 Zusammenarbeit Feuerwehr - Luftrettung, 2014, by section.</item>
/// <item>vfdb-C: vfdb-Checkliste "Landung eines Rettungshubschraubers", 2014.</item>
/// <item>DRF-T: DRF Luftrettung, Checkliste Hubschrauberlandung bei Tag, 2021.</item>
/// <item>DRF-N: DRF Luftrettung, Checkliste Nachtlandung, 2020.</item>
/// <item>Folie: DRF Luftrettung, Unterrichtung der Feuerwehren, 2022, by slide.</item>
/// <item>ADAC: ADAC Luftrettung, Taschenkarte für Einsatzkräfte, 2024.</item>
/// <item>ILS-TS: ILS Traunstein, Kurzanleitungen Motorola, 2021.</item>
/// <item>ZRF-FFB: KFV Fürstenfeldbruck, Bedienungsanleitung Sepura, 2021.</item>
/// <item>LFS-BY: Merkblatt Sprechfunk für die Feuerwehren Bayerns, 2017.</item>
/// <item>Maintainer: confirmed by the maintainer, in none of the documents above.</item>
/// </list>
/// </summary>
public static class Landeplatzvorgaben
{
    // Folie 30, 34; vfdb 4. Nothing here may read as a clearance to land.
    public const string PilotDecides = "Die Entscheidung trifft der Pilot. Er kann auch woanders landen.";

    // Folie 36.
    public const string AbortRule = "Gefahr für die Landung und kein Funk? Licht am Landeplatz aus!";

    // Folie 8.
    public const string NightDefinition =
        "Als Nacht gilt die Zeit von 30 Minuten nach Sonnenuntergang bis 30 Minuten vor Sonnenaufgang.";

    public const string SourceNote =
        "Zusammengestellt aus diesen Unterlagen. Keine dieser Stellen hat diese App geprüft oder herausgegeben.";

    private static readonly Section Zuerst = new(
        "Zuerst",
        [
            new("Eine Führungskraft übernimmt den Abschnitt \"Landeplatz RTH\"", "vfdb 4.1"),
            new("Funk früh herstellen. Kanal oder TMO-Gruppe nennt die Leitstelle", "vfdb 4.2, ADAC"),
            new("Rufname: \"Christoph\" mit Nummer oder Name, SAR-Hubschrauber \"SAR\" mit Nummer", "vfdb 4.2"),
            new("Ankunftszeit und gewünschten Landeplatz klären", "vfdb 4.2"),
            new("Tetra-Funkgerät im TMO: # lang drücken sendet die GPS-Position an die Leitstelle", "ILS-TS, ZRF-FFB"),
            new("Das Handfunkgerät sendet die letzte bekannte Position, sie kann bis zu 9 Minuten alt sein. Die Tastenbelegung kann örtlich abweichen", "ZRF-FFB, LFS-BY"),
            new("Danach die Leitstelle bitten, die Position an den Hubschrauber weiterzugeben. Er funkt meist in einer anderen TMO-Gruppe als die Feuerwehr", "Maintainer"),
            new("Der Crew melden: Hindernisse, Leitungen, Drohnen", "ADAC"),
            new("Im Endanflug nicht mit Antwort rechnen: Die Crew hört nur noch mit", "ADAC"),
            new("Ein Fahrzeug mit Blaulicht markiert den Platz, so früh wie möglich", "vfdb 4.4"),
            new("Abstand für Unbeteiligte: mindestens 50 m", "ADAC"),
        ],
        [new(DrawingKind.AbstandUnbeteiligte, "Zeichnung: Hubschrauber in der Mitte eines Kreises. Unbeteiligte stehen außerhalb, mindestens 50 m entfernt.", "ADAC")]);

    private static readonly Section Strasse = new(
        "Straße",
        [
            new("Sperren in jede Richtung, auch für Einsatzfahrzeuge", "vfdb-C, DRF-N"),
            new("Polizei einbinden", "vfdb 4.4"),
            new("Autobahn: Drei Fahrstreifen plus Pannenstreifen genügen meist, dann nur diese Seite sperren. Vierspurig: eventuell beide Seiten", "vfdb 4.4"),
            new("Gefahr für den Rotor am Rand: Böschung, Geländer, Schild, Leitplanke, Mast", "Folie 22, 25"),
            new("Der Rotor kann über die Gegenfahrbahn ragen", "Folie 25"),
        ],
        [new(DrawingKind.RotorUeberStrasse, "Zeichnung, Beispiel H145: Der Rotor hat 11,00 m Durchmesser, die Straße mit zwei Fahrstreifen und Seitenstreifen ist 10,50 m breit. Der Rotor reicht über den Fahrbahnrand.", "Folie 26")]);

    private static readonly Section NachDerLandung = new(
        "5. Nach der Landung",
        [
            new("Erst hingehen, wenn der Rotor steht oder der Pilot dazu auffordert", "DRF-T, DRF-N, vfdb-C"),
            new("Nur dort gehen, wo der Pilot einen sieht. Er sitzt rechts in Flugrichtung", "DRF-T"),
            new("Nie von hinten: Am Heckrotor droht Lebensgefahr", "vfdb-C, vfdb 4.7"),
            new("Geduckt gehen, Arme unten, nichts über den Kopf heben, Brille festhalten", "Folie 38"),
            new("Am Hang nur von der Talseite", "Folie 39"),
            new("Fahrzeuge: erst nach Anweisung und Rotorstillstand, im Schritttempo", "vfdb-C, DRF-T"),
            new("Niemals unter dem Rotor durchfahren", "vfdb 4.4, DRF-T"),
            new("RTW weit genug entfernt abstellen", "vfdb 4.8"),
            new("Am Hubschrauber arbeitet die Besatzung. Helfen nur, wenn sie darum bittet", "Folie 40"),
            new("Hubschrauber berührt? Sofort dem Piloten sagen", "vfdb 4.4"),
            new("Nur Einsatzkräfte an den Platz, Zuschauer weg", "vfdb-C, Folie 40"),
            new("Nicht rauchen", "DRF-T, DRF-N"),
        ],
        [
            new(DrawingKind.Gefahrenbereich, "Zeichnung, Hubschrauber von oben: Der Rotorkreis ist Gefahrenbereich, am Heck besteht Lebensgefahr. Von vorn und von der Seite im Blickfeld des Piloten nähern, nie von hinten.", "ADAC, vfdb 4.7"),
            new(DrawingKind.AnnaeherungHang, "Zeichnung, Hubschrauber am Hang: Auf der Talseite sind die Rotorblätter weiter vom Boden entfernt, nur von dort nähern. Nicht von der Bergseite.", "Folie 39"),
        ]);

    private static readonly Section VorDemStart = new(
        "6. Vor dem Start",
        [
            new("Mit dem Piloten klären, wie beim Start geleuchtet wird", "vfdb-C, DRF-N"),
            new("Gefahrenbereich ganz sperren, für alle: Einsatzkräfte, Rettungsdienst, Polizei", "vfdb-C, DRF-N"),
            new("Sperre erst aufheben, wenn der Pilot zugestimmt hat", "vfdb 4.4"),
        ],
        []);

    private static readonly Section Brandschutz = new(
        "Brandschutz",
        [
            new("Für eine einzelne Landung ist kein Brandschutz nötig.", "vfdb-C"),
            new("Wird ein Platz mehrfach angeflogen, etwa bei einem MANV, gilt Kategorie H1. Dann 2-facher Brandschutz:", "vfdb 5, vfdb-C"),
            new("Wasser-Schaum: mindestens 500 l Wasser, Ausstoß 250 l/min Wasser-Schaummittel-Gemisch", "vfdb-C, vfdb 5", IsSubItem: true),
            new("ABC-Pulver: 24 kg", "vfdb-C, vfdb 5", IsSubItem: true),
            new("Optional zum Pulver: CO2-Löscher", "vfdb-C", IsSubItem: true),
        ],
        []);

    private static readonly Section Vorbereitung = new(
        "Vorbereitung vor dem Einsatz",
        [
            new("Stichwort (meist FluSi), Fahrzeuge und Personal mit der Leitstelle vereinbaren und in der AAO festhalten", "Folie 16"),
            new("Geeignete Plätze regelmäßig ansehen und in Einsatzplänen notieren", "vfdb 4.3"),
            new("Mit den Betreibern klären, wer an die Flutlicht-Steuerung kommt", "vfdb 4.5"),
            new("Prüfen, welche Luftrettung vor Ort verfügbar ist, und ihre Schulungen nutzen", "vfdb 3, 4"),
        ],
        []);

    /// <summary>Named on screen, and no more than named.</summary>
    public static IReadOnlyList<string> Sources { get; } =
    [
        "vfdb-Merkblatt 06/02 und Checkliste, 2014",
        "DRF Luftrettung: Checklisten Tag (2021) und Nacht (2020), Unterrichtung der Feuerwehren (2022)",
        "ADAC Luftrettung: Taschenkarte für Einsatzkräfte, 2024",
        "ILS Traunstein: Kurzanleitungen Motorola, 2021",
        "KFV Fürstenfeldbruck: Bedienungsanleitung Sepura, 2021",
        "Merkblatt Sprechfunk für die Feuerwehren Bayerns, 2017",
    ];

    /// <summary>The steps by day. Size and approach apply by day only.</summary>
    public static IReadOnlyList<Section> Tag { get; } =
    [
        Zuerst,
        Strasse,
        new(
            "1. Eigenschutz",
            [
                new("Augen schützen: Staub und Teile fliegen", "DRF-T"),
                new("Gehörschutz tragen", "DRF-T"),
                new("Warnweste anziehen", "DRF-T"),
                new("Keine Zigarette, keine Flamme nahe am Hubschrauber", "DRF-T"),
            ],
            []),
        new(
            "2. Platz wählen",
            [
                new("Fläche: 30 m x 30 m oder größer", "DRF-T, ADAC"),
                new("Der Anflug führt gegen den Wind", "DRF-T"),
                new("Gut erreichbar", "ADAC"),

                // The sources give no figure by day: the ratio comes from the night slide, by decision in #26.
                new("Anflugsektor frei von Hindernissen: mindestens 6:1, also 20 m Höhe auf 120 m Länge", "Folie 46"),
                new("Bodenfallen suchen: Zaunpfähle, kaum sichtbare Hindernisse", "ADAC"),
                new("Alles Lose wegräumen oder festmachen, auch Pavillons, Markisen, Schirme", "ADAC"),
                new("Meiden: Sand, Steine, lockerer Boden", "DRF-T"),
                new("Nicht am Hang, nicht auf Kunstrasen", "DRF-T"),
                new("Die Sperre gilt für alle, auch für Rettungsdienst und Polizei. Kein Absperrband", "DRF-T, vfdb 4.4"),
            ],
            [new(DrawingKind.LandeplatzTag, "Zeichnung, Landeplatz bei Tag: Fläche 30 m x 30 m, Anflug gegen den Wind über einen hindernisfreien Anflugsektor. Im Schnitt 6:1, also 20 m Höhe auf 120 m Länge, und die Steillandung.", "DRF-T, ADAC, Folie 46")]),
        new(
            "3. Anflug",
            [
                new("Einweiser nur, wenn der Platz schwer zu finden ist", "vfdb 4.6"),
                new("Einweiser trägt Helm, Handschuhe, Brille und eine gut sichtbare Jacke", "DRF-T"),
                new("Eine berechtigte Person spricht mit der Crew", "DRF-T"),

                // DRF-T's signal, not vfdb 4.6's: the newest source that describes it in full (#26).
                new("Im Endanflug: knien, Arme als V nach oben, ruhig bleiben, den Piloten im Blick behalten", "DRF-T"),
                new("Sieht der Pilot den Platz: zum Rand gehen", "DRF-T"),
            ],
            []),
        new(
            "4. Landezone freihalten",
            [
                new("Der Abwind schleudert Kies und Steine mehr als 40 m weit", "DRF-T"),
                new("Keine Fahrzeuge im Anflugweg oder neben der Landezone", "DRF-T"),
                new("Nichts liegen lassen, das hochfliegen kann", "DRF-T"),
                new("An den Piloten melden: Umfeld geprüft, Landefläche frei", "DRF-T"),
            ],
            []),
        NachDerLandung,
        VorDemStart,
        Brandschutz,
        Vorbereitung,
    ];

    /// <summary>The steps by night. Size and approach apply by night only.</summary>
    public static IReadOnlyList<Section> Nacht { get; } =
    [
        Zuerst,
        Strasse,
        new(
            "1. Platz wählen",
            [
                new("Liegt ein schon bei Tag geprüfter Platz nah? Klinik, Sportplatz, Parkplatz. In die Einsatzunterlagen schauen", "vfdb-C"),
                new("Fläche: 35 m x 70 m oder größer", "DRF-N, vfdb-C"),
                new("Der Anflug führt gegen den Wind", "vfdb 4.5"),
                new("Keine Freileitung über dem Platz, seitlich mindestens 300 m Abstand. Leitungen immer der Crew melden", "DRF-N, vfdb 4.3"),
                new("Anflugsektor auf ca. 250 m frei von Hindernissen. Dazu mindestens 6:1, also 20 m Höhe auf 120 m Länge", "vfdb-C, Folie 46"),
                new("Der Boden muss tragen: kein Sand, keine Steine, kein Schotter", "DRF-N"),
                new("Platz eben und mit Fahrzeugen befahrbar", "DRF-N"),
                new("Nicht auf Kunstrasen. Tor und Schlüssel klären", "Folie 20"),
                new("Der Rettungswagen muss hinkommen", "DRF-N"),
            ],
            [new(DrawingKind.LandeplatzNacht, "Zeichnung, Landeplatz bei Nacht: Fläche 35 m x 70 m mit einem Scheinwerfer an jeder Ecke. Anflugsektor auf ca. 250 m hindernisfrei, mindestens 6:1. Ein Fahrzeug mit Blaulicht steht am Rand, ein nahes Hindernis ist angestrahlt.", "DRF-N, vfdb-C, Folie 46")]),
        new(
            "2. Platz sichern",
            [
                new("Fläche ohne Band sichern: weder Absperrband noch Flatterband", "vfdb-C, Folie 43"),
                new("Alles Lose wegräumen oder festmachen", "vfdb-C, DRF-N"),
                new("Nichts auf dem Boden auslegen, auch keine Markierung", "Folie 43"),
            ],
            []),
        new(
            "3. Ausleuchtung: Landepunkt",
            [
                new("Vier Scheinwerfer, einer pro Ecke", "vfdb-C"),
                new("Licht niedrig und flach, ohne zu blenden, in Anflugrichtung", "DRF-N, vfdb 4.5"),
                new("Der Pilot kommt gegen den Wind und in Lichtrichtung", "vfdb 4.5"),
                new("Minimum: 2 Kfz, 35 bis 50 m auseinander, Abblendlicht über Kreuz auf den Landepunkt", "vfdb 4.5"),
                new("Lichtmast möglichst nicht nutzen, sein Licht sitzt zu hoch. Muss er sein: Strahler ganz nach unten", "vfdb 4.5, Folie 31"),
                new("Stativ eingefahren lassen, Strahler ganz nach unten, gegen Kippen sichern", "vfdb 4.5, Folie 32"),
                new("Blaulicht oder Warnblinker spätestens 5 Minuten vor der Landung an, dann an lassen", "DRF-N, Folie 43"),
                new("Sportplatz: Flutlicht nutzen. Bleibt es dunkel, die Masten sichtbar machen", "vfdb 4.5"),
                new("Bei Bedarf eine Person zum Einweisen. Bei Schnee in der Hocke, bis zur Landung am Platz bleiben", "DRF-N"),
            ],
            [new(DrawingKind.Lichtkreuz, "Zeichnung, Mindestbeleuchtung: Zwei Kfz stehen 35 bis 50 m auseinander. Ihr Abblendlicht kreuzt sich auf dem Landepunkt.", "vfdb 4.5")]),
        new(
            "3. Ausleuchtung: Hindernisse",
            [
                new("Was nah steht, anstrahlen: Bäume, Masten, Antennen, Windräder, Gebäude", "vfdb 4.5, vfdb-C"),
                new("Das Licht zeigt vom Platz weg aufs Hindernis, nicht zur Crew", "vfdb 4.5"),
                new("Freileitung: zwei oder mehr Masten anstrahlen, so wird ihr Verlauf sichtbar", "vfdb 4.5"),
            ],
            []),
        new(
            "4. Sprechfunk",
            [
                new("Melden: Platz, Hindernisse, Gefahrenstellen", "vfdb-C, DRF-N"),
                new("An den Piloten melden: Umfeld geprüft, Landefläche frei", "DRF-N"),
                new("Empfangsbereit bleiben, Funk bis zur Landung halten", "DRF-N, vfdb-C"),
                new("Licht ändern, wenn der Pilot es sagt. Nachtsichtgeräte reagieren stark auf Licht", "vfdb 4.5, Folie 35"),
                new("Die Crew prüft den Platz erst von oben, dann sinkt sie steil ab", "Folie 17"),
            ],
            []),
        new(
            "Schnee",
            [new("Hochwirbelnder Schnee kann dem Piloten die Sicht nehmen. Bläst er ihn vorher weg, kostet das Zeit. An den Fahrzeugen Fenster und Türen schließen.", "Folie 37")],
            []),
        NachDerLandung,
        VorDemStart,
        Brandschutz,
        Vorbereitung,
    ];
}
