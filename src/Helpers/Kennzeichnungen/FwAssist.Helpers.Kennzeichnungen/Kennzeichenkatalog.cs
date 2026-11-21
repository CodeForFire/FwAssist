namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>
/// Every marking the helper shows, by group. Names and descriptions of the Gefahrzettel are ADR
/// 2021, 5.2.2.2.2 (Gefahrzettelmuster, the class headings and the column "Symbol und Farbe des
/// Symbols"); the GHS entries are CLP, Verordnung (EG) Nr. 1272/2008, Anhang V, in the version of
/// ABl. L 353 of 31.12.2008, shortened to the hazard classes without their categories where the
/// screen has no room. Every entry names its paragraph in <see cref="Kennzeichen.Source"/>.
/// </summary>
public static class Kennzeichenkatalog
{
    public const string SourceNote =
        "Zusammengestellt aus diesen Vorschriften. Keine dieser Stellen hat diese App geprüft oder herausgegeben.";

    /// <summary>Named on screen, and no more than named.</summary>
    public static IReadOnlyList<string> Sources { get; } =
    [
        "ADR 2021, Teil 5: Gefahrzettel 5.2.2.2.2, Kennzeichen 5.2.1.8 und 5.3.3, Warntafeln 5.3.2",
        "Verordnung (EG) Nr. 1272/2008 (CLP), Anhang V: Gefahrenpiktogramme",
        "Bilder: Wikimedia Commons, gemeinfrei (Public Domain)",
    ];

    public static IReadOnlyList<Kennzeichen> Warntafeln { get; } =
    [
        new(
            KennzeichenGruppe.Warntafel,
            "warntafel-leer",
            "ohne Nummern",
            "Warntafel ohne Nummern",
            "orange Rechteck, schwarzer Rand, leer",
            "Gefahrgut an Bord, vorn und hinten am Fahrzeug. Welcher Stoff, steht im Beförderungspapier.",
            "ADR 5.3.2.1.1, 5.4.1"),
        new(
            KennzeichenGruppe.Warntafel,
            "warntafel-nummern",
            "33 / 1203",
            "Warntafel mit Nummern",
            "orange Rechteck, schwarzer Rand, waagerechte Linie, oben 2 oder 3 Ziffern, unten 4 Ziffern",
            "Oben die Nummer zur Kennzeichnung der Gefahr, unten die UN-Nummer. Beispiel: 33 leicht entzündbarer flüssiger Stoff, UN 1203 Benzin.",
            "ADR 5.3.2.2.3, 5.3.2.3"),
        new(
            KennzeichenGruppe.Warntafel,
            "erwaermt",
            "erwärmt",
            "Kennzeichen für erwärmte Stoffe",
            "rotes Dreieck mit der Spitze nach oben, Thermometer",
            "Flüssig bei 100 °C oder mehr oder fest bei 240 °C oder mehr befördert.",
            "ADR 5.3.3"),
        new(
            KennzeichenGruppe.Warntafel,
            "umwelt",
            "umweltgefährdend",
            "Kennzeichen für umweltgefährdende Stoffe",
            "Raute, schwarzer Fisch und toter Baum auf Weiß",
            "Umweltgefährdender Stoff, an Versandstück, Container, Tank oder Fahrzeug.",
            "ADR 5.2.1.8, 5.3.6"),
    ];

    public static IReadOnlyList<Kennzeichen> Gefahrzettel { get; } =
    [
        Zettel("adr-1", "1", "Explosive Stoffe und Gegenstände mit Explosivstoff", "explodierende Bombe schwarz auf Orange", "Unterklassen 1.1, 1.2 und 1.3"),
        Zettel("adr-1.4", "1.4", "Explosive Stoffe und Gegenstände mit Explosivstoff", "Ziffern 1.4 schwarz auf Orange", "Unterklasse 1.4"),
        Zettel("adr-1.5", "1.5", "Explosive Stoffe und Gegenstände mit Explosivstoff", "Ziffern 1.5 schwarz auf Orange", "Unterklasse 1.5"),
        Zettel("adr-1.6", "1.6", "Explosive Stoffe und Gegenstände mit Explosivstoff", "Ziffern 1.6 schwarz auf Orange", "Unterklasse 1.6"),
        Zettel("adr-2.1", "2.1", "Entzündbare Gase", "Flamme schwarz oder weiß auf Rot", "Klasse 2: Gase"),
        Zettel("adr-2.2", "2.2", "Nicht entzündbare, nicht giftige Gase", "Gasflasche schwarz oder weiß auf Grün", "Klasse 2: Gase"),
        Zettel("adr-2.3", "2.3", "Giftige Gase", "Totenkopf mit gekreuzten Gebeinen schwarz auf Weiß", "Klasse 2: Gase"),
        Zettel("adr-3", "3", "Entzündbare flüssige Stoffe", "Flamme schwarz oder weiß auf Rot", "Klasse 3"),
        Zettel("adr-4.1", "4.1", "Entzündbare feste Stoffe, selbstzersetzliche Stoffe, polymerisierende Stoffe und desensibilisierte explosive feste Stoffe", "Flamme schwarz auf Weiß mit sieben senkrechten roten Streifen", "Klasse 4.1"),
        Zettel("adr-4.2", "4.2", "Selbstentzündliche Stoffe", "Flamme schwarz, obere Hälfte weiß, untere Hälfte rot", "Klasse 4.2"),
        Zettel("adr-4.3", "4.3", "Stoffe, die in Berührung mit Wasser entzündbare Gase entwickeln", "Flamme schwarz oder weiß auf Blau", "Klasse 4.3"),
        Zettel("adr-5.1", "5.1", "Entzündend (oxidierend) wirkende Stoffe", "Flamme über einem Kreis schwarz auf Gelb", "Klasse 5.1"),
        Zettel("adr-5.2", "5.2", "Organische Peroxide", "Flamme schwarz oder weiß, obere Hälfte rot, untere Hälfte gelb", "Klasse 5.2"),
        Zettel("adr-6.1", "6.1", "Giftige Stoffe", "Totenkopf mit gekreuzten Gebeinen schwarz auf Weiß", "Klasse 6.1"),
        Zettel("adr-6.2", "6.2", "Ansteckungsgefährliche Stoffe", "Kreis mit drei Sicheln schwarz auf Weiß", "Klasse 6.2"),
        Zettel("adr-7a", "7A", "Radioaktive Stoffe, Kategorie I – WEISS", "Strahlensymbol schwarz auf Weiß, ein roter Streifen", "Klasse 7"),
        Zettel("adr-7b", "7B", "Radioaktive Stoffe, Kategorie II – GELB", "Strahlensymbol schwarz, obere Hälfte gelb, zwei rote Streifen", "Klasse 7"),
        Zettel("adr-7c", "7C", "Radioaktive Stoffe, Kategorie III – GELB", "Strahlensymbol schwarz, obere Hälfte gelb, drei rote Streifen", "Klasse 7"),
        Zettel("adr-7e", "7E", "Spaltbare Stoffe", "Aufschrift FISSILE schwarz auf Weiß", "Klasse 7"),
        Zettel("adr-8", "8", "Ätzende Stoffe", "Flüssigkeit aus zwei Reagenzgläsern greift Hand und Metall an; obere Hälfte weiß, untere schwarz", "Klasse 8"),
        Zettel("adr-9", "9", "Verschiedene gefährliche Stoffe und Gegenstände", "sieben senkrechte schwarze Streifen oben auf Weiß", "Klasse 9"),
        Zettel("adr-9a", "9A", "Verschiedene gefährliche Stoffe und Gegenstände: Batterien", "sieben schwarze Streifen oben, unten Batterien, eine beschädigt und brennend", "Klasse 9"),
    ];

    public static IReadOnlyList<Kennzeichen> Ghs { get; } =
    [
        Piktogramm("ghs01", "GHS01", "Explodierende Bombe", "Instabile explosive Stoffe, explosive Stoffe der Unterklassen 1.1 bis 1.4, selbstzersetzliche Stoffe und organische Peroxide der Typen A und B", "1.1"),
        Piktogramm("ghs02", "GHS02", "Flamme", "Entzündbare Gase, Aerosole, Flüssigkeiten und Feststoffe; pyrophore und selbsterhitzungsfähige Stoffe; Stoffe, die mit Wasser entzündbare Gase abgeben; selbstzersetzliche Stoffe und organische Peroxide der Typen B bis F", "1.2"),
        Piktogramm("ghs03", "GHS03", "Flamme über einem Kreis", "Oxidierende Gase, Flüssigkeiten und Feststoffe", "1.3"),
        Piktogramm("ghs04", "GHS04", "Gasflasche", "Gase unter Druck: verdichtet, verflüssigt, tiefgekühlt verflüssigt oder gelöst", "1.4"),
        Piktogramm("ghs05", "GHS05", "Ätzwirkung", "Auf Metalle korrosiv wirkend, hautätzend, schwere Augenschädigung", "1.5, 2.2"),
        Piktogramm("ghs06", "GHS06", "Totenkopf mit gekreuzten Knochen", "Akute Toxizität (oral, dermal, inhalativ), Kategorien 1 bis 3", "2.1"),
        Piktogramm("ghs07", "GHS07", "Ausrufezeichen", "Akute Toxizität Kategorie 4, Reizung von Haut und Augen, Sensibilisierung der Haut, Atemwegsreizung, narkotisierende Wirkung", "2.3"),
        Piktogramm("ghs08", "GHS08", "Gesundheitsgefahr", "Sensibilisierung der Atemwege, Keimzellmutagenität, Karzinogenität, Reproduktionstoxizität, Zielorgan-Toxizität, Aspirationsgefahr", "2.4"),
        Piktogramm("ghs09", "GHS09", "Umwelt", "Gewässergefährdend: akut Kategorie 1, chronisch Kategorien 1 und 2", "3.1"),
    ];

    /// <summary>Every marking, group by group, in the order the screen lists them.</summary>
    public static IReadOnlyList<Kennzeichen> All { get; } = [.. Warntafeln, .. Gefahrzettel, .. Ghs];

    public static IReadOnlyList<Kennzeichen> Of(KennzeichenGruppe gruppe) => gruppe switch
    {
        KennzeichenGruppe.Gefahrzettel => Gefahrzettel,
        KennzeichenGruppe.Ghs => Ghs,
        _ => Warntafeln,
    };

    private static Kennzeichen Zettel(string key, string muster, string name, string look, string klasse) =>
        new(KennzeichenGruppe.Gefahrzettel, key, muster, name, look, klasse, "ADR 5.2.2.2.2, Muster " + muster);

    private static Kennzeichen Piktogramm(string key, string code, string symbol, string meaning, string section) =>
        new(KennzeichenGruppe.Ghs, key, code, symbol, symbol + " schwarz auf Weiß, roter Rand, Raute", meaning, "CLP Anhang V, " + section);
}
