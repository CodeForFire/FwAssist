namespace Helfer.Helpers.DekonPlatz;

/// <summary>
/// What the screen says, per Stufe. The wording is our own and states the sources' facts in new
/// sentences. Every line names its source, and issue #25 records how they were reconciled:
/// <list type="bullet">
/// <item>TK: Taschenkarte ABC, AK Gefahrgut, 2023. It credits its Platz drawings to the ABC-Konzept Bayern Dekontamination, 06-2022.</item>
/// <item>BBK: BBK, Internetseite "Dekontamination Personal".</item>
/// <item>HB: Handbuch GW Dekon P BUND V1.7, by page.</item>
/// <item>Maintainer: confirmed by the maintainer in #25, in none of the documents above.</item>
/// </list>
/// No source gives a site plan or build order for Stufe III, so none is drawn. HB 10 leaves the
/// setup to the Einsatzführer.
/// </summary>
public static class Dekonvorgaben
{
    public const string SourceNote =
        "Zusammengestellt aus diesen Unterlagen. Keine dieser Stellen hat diese App geprüft oder herausgegeben.";

    /// <summary>Named on screen, and no more than named.</summary>
    public static IReadOnlyList<string> Sources { get; } =
    [
        "Taschenkarte ABC",
        "ABC-Konzept Bayern Dekontamination, 06-2022: Vorlage der Taschenkarte für den Dekon-Platz",
        "FwDV 500, 2022: wie von Taschenkarte und BBK genannt",
        "BBK: Dekontamination Personal (Internetseite)",
        "Handbuch GW Dekon P BUND, V1.7",
        "Reihenfolge des Aufbaus Stufe II: eigene Zusammenstellung",
    ];

    /// <summary>Stufe I, the emergency decontamination.</summary>
    public static IReadOnlyList<Section> StufeEins { get; } =
    [
        new(
            "Sofort-Dekon",
            [
                new("Notdekontamination für ungeschützte Personen", "BBK"),
                new("Bereit ab Vorgehen des 1. Trupps", "TK"),
            ],
            []),
        new(
            "Mindestens dabei",
            [
                new("Wasser", "TK"),
                new("Trage und Decke", "TK"),
                new("Erste-Hilfe-Ausrüstung", "TK"),
            ],
            []),
        new(
            "Maßnahmen",
            [
                new("1. Wenn nötig: Augen spülen", "TK"),
                new("2. Verdacht auf A-Stoffe und Messgerät da: auf radioaktive Kontamination messen", "TK"),
                new("3. Betroffene Körperstellen frei machen", "TK"),
                new("4. Sichtbaren Stoff mit Papiertüchern oder Kompressen abtupfen", "TK"),
                new("5. Noch nötig und nichts spricht dagegen: mit Wasser, Seife und Schwamm abwaschen", "TK"),
                new("6. Verdacht auf B-Stoffe und Mittel da: betroffene Körperstellen desinfizieren", "TK"),
                new("7. Person warm halten", "TK"),
            ],
            []),
    ];

    /// <summary>Stufe II, with the build order the maintainer confirmed in #25.</summary>
    public static IReadOnlyList<Section> StufeZwei { get; } =
    [
        new(
            "Standard-Dekon",
            [
                new("Standard-Dekontamination für Einsatzkräfte in Schutzkleidung", "BBK"),
                new("Steht 15 Minuten nach Vorgehen des 1. Trupps", "TK"),
                new("Spätestens bei der Abwasserentsorgung hilft die Dekon-Einheit", "TK"),
            ],
            []),
        new(
            "Aufbau",
            [
                new("1. Platz am Rand des Gefahrenbereichs wählen. Wind kommt von der sauberen Seite", "TK"),
                new("2. Fläche abgrenzen: 9 x 4 m, bei Schwarz-Weiß 8 x 4 m", "TK"),
                new("3. Daneben 2 m als Eingang freihalten", "TK"),
                new("4. Bereiche in Laufrichtung markieren: rot, gelb, grün oder schwarz, weiß", "TK"),
                new("5. Geräteablage vor dem Platz aufbauen, zum Gefahrenbereich hin", "TK"),
                new("6. Roter Bereich, bei Schwarz-Weiß vorn im schwarzen: Warteplatz. Behälter für benutztes Gerät aufstellen", "Maintainer"),
                new("7. Kleine Wanne für die Schuhe dort direkt vor die große Wanne", "Maintainer"),
                new("8. Große Wanne in den gelben oder schwarzen Bereich stellen. Daneben Plastiksack für den ausgezogenen Chemikalienschutzanzug", "TK, Maintainer"),
                new("9. Abwasserbehälter vor dem Platz aufstellen, zum Gefahrenbereich hin. Pumpe anschließen: Das Abwasser aus der Wanne wird hineingepumpt", "TK, Maintainer"),
                new("10. Erst-Hilfe-Material und Doku in den grünen oder weißen Bereich legen", "TK"),
                new("11. Betriebsbereitschaft melden, ab jetzt Personen erfassen", "TK, Maintainer"),
            ],
            [
                new(DrawingKind.DreiBereiche, "Zeichnung, Platz mit drei Bereichen, 9 m x 4 m, daneben 2 m Eingang. Links der Gefahrenbereich, davor Geräteablage und Abwasserbehälter. Rot: Warten, Behälter für Gerät, kleine Wanne für Schuhe. Gelb: große Wanne und Sack, verbunden mit dem Abwasserbehälter. Grün: Doku und Erst-Hilfe-Material. Der Wind kommt von der sauberen Seite.", "TK, Maintainer"),
                new(DrawingKind.SchwarzWeiss, "Zeichnung, Platz mit Schwarz-Weiß-Aufteilung, 8 m x 4 m, Bereiche nicht maßstäblich, daneben 2 m Eingang. Links der Gefahrenbereich, davor Geräteablage und Abwasserbehälter. Schwarz: Warten, Behälter für Gerät, kleine Wanne für Schuhe, große Wanne und Sack. Weiß: Doku und Erst-Hilfe-Material. Der Wind kommt von der sauberen Seite.", "TK, Maintainer"),
            ]),
    ];

    /// <summary>Stufe III: what the system is and what it needs, without a site plan.</summary>
    public static IReadOnlyList<Section> StufeDrei { get; } =
    [
        new(
            "Erweiterte Dekon",
            [
                new("Wenn nach dem Ablegen der Schutzkleidung komplettes Entkleiden und Duschen nötig sind", "BBK"),
                new("Erweitert die Stufe II", "TK"),
                new("Durch eine Dekon-Einheit mit GW Dekon P", "TK"),
                new("Platzbedarf: 10 m x 25 m", "TK"),
                new("Kein Lageplan: Wie die Anlage steht, legt der Einsatzführer fest", "HB 10"),
            ],
            [new(DrawingKind.AblaufStufeDrei, "Zeichnung, Ablauf für eine Person: Schutzkleidung ablegen, Auskleiden im Auskleidezelt, Duschen im Duschzelt, Ankleiden im Ankleidezelt. Kein Lageplan.", "BBK, Maintainer")]),
        new(
            "Anlage",
            [
                new("Duschzelt, daran anschließend Aus- und Ankleidezelte", "BBK"),
                new("Kleine Einpersonenkabine für die Grobreinigung im Chemikalienschutzanzug", "BBK"),
                new("Aufbau durch 6 Einsatzkräfte", "BBK"),
                new("Verlastet in 7 Rollcontainern auf einem LKW mit Ladebordwand", "BBK"),
            ],
            []),
        new(
            "Vor der Benutzung",
            [
                new("Fertig aufgebaute Anlage mindestens 3 Minuten mit Trinkwasser spülen", "HB 10"),
                new("Wassertemperatur an den Entnahmestellen prüfen", "HB 10"),
            ],
            []),
        new(
            "Wasser",
            [
                new("Nur Trinkwasser verwenden", "HB 5, HB 10"),
                new("Ausgelegt für 5 bar Versorgungsdruck", "HB 5"),
                new("Das Fahrzeug bringt 2000 l für den Erstangriff mit", "HB 10"),
                new("Mit Trinkwassernetz: Standrohr anschließen. Druck zu niedrig: Pumpe einbinden. Druck zu hoch: Druckminderer einbinden", "HB 10"),
                new("Pumpe nie hinter einem Druckminderer betreiben", "HB 11"),
                new("Ohne Netz: aus den flexiblen Tanks mit der Pumpe. Das Fahrzeug kann Wasser im Pendelverkehr nachführen", "HB 11"),
            ],
            []),
        new(
            "Abwasser",
            [
                new("Behälter für 5000 l, dazu zwei Abwasserpumpen", "BBK"),
                new("Pumpe am tiefer liegenden Anschluss verbinden, den anderen verschließen", "HB 12"),
            ],
            []),
        new(
            "Zelte",
            [
                new("Ab 5,5 m/s Wind zusätzlich sichern", "HB 5"),
                new("Füllen mit Druckluft nie unbeaufsichtigt lassen", "HB 13"),
            ],
            []),
        new(
            "Leistung",
            [
                new("Etwa 50 Personen je Stunde", "BBK"),
                new("Etwa 3 m³ Wasser je Stunde bei voller Auslastung", "BBK"),
                new("Mit dem Durchlauferhitzer über 5 Stunden ohne Versorgung von außen", "BBK"),
            ],
            []),
    ];
}
