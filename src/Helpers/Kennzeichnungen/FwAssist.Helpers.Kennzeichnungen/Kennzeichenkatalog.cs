namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>
/// Every marking the helper shows, by group. Names and descriptions of the Gefahrzettel are ADR
/// 2021, 5.2.2.2.2 (Gefahrzettelmuster, the class headings and the column "Symbol und Farbe des
/// Symbols"); the GHS entries are CLP, Verordnung (EG) Nr. 1272/2008, Anhang V, in the version of
/// ABl. L 353 of 31.12.2008, shortened to the hazard classes without their categories where the
/// screen has no room. Rohrleitungen and Gasflaschen are documented on their lists. The wording is
/// our own and states the sources' facts in new sentences. Every entry names its paragraph in
/// <see cref="Kennzeichen.Source"/>.
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
        "TRGS 201, Anhang 3: Kennzeichnung von Rohrleitungen nach den Durchflussstoffen (nach DIN 2403)",
        "DIN EN 1089-3:2011, Farbcodierung von Gasflaschen, deutscher Text wie in BGBl. II Nr. 458/2011 (Österreich)",
        "Industriegaseverband (IGV): Faltblatt Farbkennzeichnung von Gasflaschen",
        "Bilder: Wikimedia Commons, gemeinfrei (Public Domain); Rohrleitungen und Gasflaschen eigene Zeichnungen",
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

    /// <summary>
    /// TRGS 201 Anhang 3, Tabelle 2, which gives DIN 2403's groups "beispielhaft". The group
    /// colour is the band, the extra colour a field beside it, the lettering colour the arrow.
    /// </summary>
    public static IReadOnlyList<Kennzeichen> Rohrleitungen { get; } =
    [
        Rohr("1", "Wasser", "grün", null, "weiß"),
        Rohr("2", "Wasserdampf", "rot", null, "weiß"),
        Rohr("3", "Luft", "grau", null, "schwarz"),
        Rohr("4", "Brennbare Gase", "gelb", "rot", "schwarz"),
        Rohr("5", "Nichtbrennbare Gase", "gelb", "schwarz", "schwarz"),
        Rohr("6", "Säuren", "orange", null, "schwarz"),
        Rohr("7", "Laugen", "violett", null, "weiß"),
        Rohr("8", "Brennbare Flüssigkeiten und Feststoffe", "braun", "rot", "weiß"),
        Rohr("9", "Nichtbrennbare Flüssigkeiten und Feststoffe", "braun", "schwarz", "weiß"),
        Rohr("0", "Sauerstoff", "blau", null, "weiß"),
    ];

    /// <summary>
    /// The shoulder colours of EN 1089-3:2011: by hazard (6.2.2), for special gases (6.2.3), for
    /// breathing and medical mixtures (6.2.5, 6.2.6) and the white body (6.3). Examples of gases
    /// are Tabelle D.1 and the IGV leaflet; the body colour of industrial cylinders is the IGV's.
    /// </summary>
    public static IReadOnlyList<Kennzeichen> Gasflaschen { get; } =
    [
        Flasche("flasche-gelb", "gelb", "Giftig und/oder ätzend", "Schulter gelb", "Zum Beispiel Ammoniak, Chlor, Kohlenmonoxid, Schwefeldioxid.", "6.2.2.2 a, Tabelle D.1"),
        Flasche("flasche-rot", "rot", "Entzündbar", "Schulter rot", "Zum Beispiel Wasserstoff, Methan, Ethylen, Formiergas.", "6.2.2.2 b, Tabelle D.1"),
        Flasche("flasche-hellblau", "hellblau", "Oxidierend", "Schulter hellblau", "Brandfördernde Gase und Gemische, soweit sie keine eigene Farbe haben.", "6.2.2.2 c"),
        Flasche("flasche-gruen", "leuchtend grün", "Inert (erstickend)", "Schulter leuchtend gelbgrün", "Nicht giftig, nicht entzündbar, nicht oxidierend, verdrängt aber den Sauerstoff. Zum Beispiel Krypton, Xenon, Neon, Schweißschutzgas, technische Druckluft.", "6.2.2.2 d, IGV"),
        Flasche("flasche-gelb-rot", "gelb + rot", "Giftig und entzündbar", "Schulter gelb mit rotem Ring", "Die Hauptgefahr gelb, die Nebengefahr rot. Der rote Ring darf fehlen.", "6.2.2.3 a"),
        Flasche("flasche-gelb-hellblau", "gelb + hellblau", "Giftig und oxidierend", "Schulter gelb mit hellblauem Ring", "Die Hauptgefahr gelb, die Nebengefahr hellblau. Der hellblaue Ring darf fehlen.", "6.2.2.3 b"),
        Flasche("flasche-acetylen", "kastanienbraun", "Acetylen", "Schulter kastanienbraun", "Entzündbar, mit eigener Farbe statt rot.", "6.2.3.1 a"),
        Flasche("flasche-sauerstoff", "weiß", "Sauerstoff", "Schulter weiß", "Oxidierend, mit eigener Farbe statt hellblau.", "6.2.3.1 b"),
        Flasche("flasche-lachgas", "blau", "Distickstoffmonoxid (Lachgas)", "Schulter dunkelblau", "Oxidierend, mit eigener Farbe.", "6.2.3.1 b"),
        Flasche("flasche-argon", "dunkelgrün", "Argon", "Schulter dunkelgrün", "Inert. Außerhalb der Medizin auch leuchtend grün.", "6.2.3.2 a"),
        Flasche("flasche-stickstoff", "schwarz", "Stickstoff", "Schulter schwarz", "Inert. Außerhalb der Medizin auch leuchtend grün.", "6.2.3.2 b"),
        Flasche("flasche-kohlendioxid", "grau", "Kohlendioxid", "Schulter grau", "Inert, erstickend. Außerhalb der Medizin auch leuchtend grün.", "6.2.3.2 c"),
        Flasche("flasche-helium", "braun", "Helium", "Schulter braun", "Inert. Außerhalb der Medizin auch leuchtend grün.", "6.2.3.2 d"),
        Flasche("flasche-luft", "weiß + schwarz", "Luft zum Atmen", "Schulter in Vierteln weiß und schwarz", "Atemluft oder synthetische Luft, medizinisch oder als Atemgas. Atemluftflaschen der Feuerwehr haben meist einen gelben oder roten Mantel in Leuchtfarbe.", "6.2.5 a, IGV"),
        Flasche("flasche-sauerstoff-helium", "weiß + braun", "Sauerstoff und Helium", "Schulter mit weißem und braunem Ring", "Gemisch zum Atmen mit mindestens 20 % Sauerstoff.", "6.2.5 b"),
        Flasche("flasche-sauerstoff-kohlendioxid", "weiß + grau", "Sauerstoff und Kohlendioxid", "Schulter mit weißem und grauem Ring", "Gemisch zum Atmen mit mindestens 20 % Sauerstoff.", "6.2.5 c"),
        Flasche("flasche-sauerstoff-lachgas", "weiß + blau", "Sauerstoff und Lachgas", "Schulter mit weißem und dunkelblauem Ring", "Gemisch zum Atmen mit mindestens 20 % Sauerstoff.", "6.2.5 d"),
        Flasche("flasche-stickstoffmonoxid", "türkisblau", "Stickstoffmonoxid in Stickstoff", "Schulter türkisblau, Mantel weiß", "Medizinisches Gemisch mit weniger als 1000 ppm Stickstoffmonoxid.", "6.2.6"),
        Flasche("flasche-medizinisch", "weißer Mantel", "Gas für die medizinische Verwendung", "Mantel ganz weiß, Schulter in der Farbe des Gases", "Ein weißer Flaschenkörper bedeutet immer: medizinisches Gas. Industriegase haben ihn nie.", "6.3"),
    ];

    /// <summary>Every marking, group by group, in the order the screen lists them.</summary>
    public static IReadOnlyList<Kennzeichen> All { get; } = [.. Warntafeln, .. Gefahrzettel, .. Ghs, .. Rohrleitungen, .. Gasflaschen];

    public static IReadOnlyList<Kennzeichen> Of(KennzeichenGruppe gruppe) => gruppe switch
    {
        KennzeichenGruppe.Gefahrzettel => Gefahrzettel,
        KennzeichenGruppe.Ghs => Ghs,
        KennzeichenGruppe.Rohrleitung => Rohrleitungen,
        KennzeichenGruppe.Gasflasche => Gasflaschen,
        _ => Warntafeln,
    };

    private static Kennzeichen Zettel(string key, string muster, string name, string look, string klasse) =>
        new(KennzeichenGruppe.Gefahrzettel, key, muster, name, look, klasse, "ADR 5.2.2.2.2, Muster " + muster);

    private static Kennzeichen Rohr(string gruppe, string stoff, string farbe, string? zusatz, string schrift)
    {
        var look = zusatz is null
            ? $"Band {farbe}, Pfeil und Schrift {schrift}"
            : $"Band {farbe} mit Feldern in {zusatz}, Pfeil und Schrift {schrift}";
        var colours = zusatz is null ? $"Gruppenfarbe {farbe}" : $"Gruppenfarbe {farbe}, Zusatzfarbe {zusatz}";
        return new(
            KennzeichenGruppe.Rohrleitung,
            "rohr-" + gruppe,
            "Gruppe " + gruppe,
            stoff,
            look,
            colours + ".",
            "TRGS 201 Anhang 3, Tabelle 2");
    }

    private static Kennzeichen Flasche(string key, string farbe, string name, string look, string meaning, string paragraph) =>
        new(KennzeichenGruppe.Gasflasche, key, farbe, name, look, meaning, "EN 1089-3:2011, " + paragraph);

    private static Kennzeichen Piktogramm(string key, string code, string symbol, string meaning, string section) =>
        new(KennzeichenGruppe.Ghs, key, code, symbol, symbol + " schwarz auf Weiß, roter Rand, Raute", meaning, "CLP Anhang V, " + section);
}
