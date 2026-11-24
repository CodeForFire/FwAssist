using System.Text.RegularExpressions;

namespace FwAssist.Helpers.Kennzeichnungen.Tests;

public partial class TabelleATests
{
    private static Stoff Single(string unNummer)
    {
        Assert.True(TabelleA.TryFind(unNummer, out var rows));
        return Assert.Single(rows);
    }

    // The shape of the upper half of a plate, written out on its own so the data is checked against
    // the ADR, not against Gefahrnummern. 1.5D is the one exception, carried by UN 0331 and 0332.
    [GeneratedRegex(@"^(X?\d{2,3}|1\.5D)$")]
    private static partial Regex GefahrnummerShape();

    [GeneratedRegex(@"^(1|2|3|4\.[123]|5\.[12]|6\.[12]|7|8|9)$")]
    private static partial Regex KlasseShape();

    [Fact]
    public void Holds_every_un_nummer_of_adr_2025_once_per_distinct_row()
    {
        var all = TabelleA.All;

        Assert.InRange(all.Count, 2600, 2800);
        Assert.InRange(all.Select(s => s.UnNummer).Distinct(StringComparer.Ordinal).Count(), 2340, 2360);
        Assert.Equal(all.Count, all.Distinct().Count());
    }

    [Fact]
    public void Every_row_is_well_formed()
    {
        foreach (var s in TabelleA.All)
        {
            Assert.Matches(@"^\d{4}$", s.UnNummer);
            Assert.False(string.IsNullOrWhiteSpace(s.Benennung), s.UnNummer);
            Assert.Matches(KlasseShape(), s.Klasse);
            if (s.Gefahrnummer is { } number)
            {
                Assert.Matches(GefahrnummerShape(), number);
            }
        }
    }

    [Fact]
    public void Rows_stay_in_the_order_of_tabelle_a()
    {
        var numbers = TabelleA.All.Select(s => s.UnNummer).ToList();

        Assert.Equal(numbers.Order(StringComparer.Ordinal), numbers);
        Assert.Equal("0004", numbers[0]);
        Assert.Equal("3560", numbers[^1]);
    }

    [Fact]
    public void Benzin_is_33_over_1203_klasse_3()
    {
        var benzin = Single("1203");

        Assert.Equal("BENZIN oder OTTOKRAFTSTOFF", benzin.Benennung);
        Assert.Equal("3", benzin.Klasse);
        Assert.Equal("3", benzin.Gefahrzettel);
        Assert.Equal("33", benzin.Gefahrnummer);
        Assert.Equal(StoffStatus.Gefahrgut, benzin.Status);
    }

    [Fact]
    public void Ammoniak_carries_both_its_labels_and_268()
    {
        var ammoniak = Single("1005");

        Assert.Equal("AMMONIAK, WASSERFREI", ammoniak.Benennung);
        Assert.Equal("2", ammoniak.Klasse);
        Assert.Equal("2.3+8", ammoniak.Gefahrzettel);
        Assert.Equal("268", ammoniak.Gefahrnummer);
    }

    [Fact]
    public void Diesel_keeps_each_of_its_rows()
    {
        Assert.True(TabelleA.TryFind("1202", out var diesel));

        Assert.Equal(3, diesel.Count);
        Assert.All(diesel, s => Assert.StartsWith("DIESELKRAFTSTOFF", s.Benennung, StringComparison.Ordinal));
        Assert.All(diesel, s => Assert.Equal("30", s.Gefahrnummer));
    }

    [Fact]
    public void An_explosive_has_no_gefahrnummer()
    {
        var pikrat = Single("0004");

        Assert.Equal("1", pikrat.Klasse);
        Assert.Null(pikrat.Gefahrnummer);
        Assert.Equal(string.Empty, pikrat.Tafel);
    }

    [Fact]
    public void A_water_reactive_stoff_keeps_its_x()
    {
        Assert.Equal("X423", Single("3404").Gefahrnummer);
    }

    [Fact]
    public void Trockeneis_is_listed_as_not_subject_to_the_adr()
    {
        var trockeneis = Single("1845");

        Assert.Equal(StoffStatus.NichtAdr, trockeneis.Status);
        Assert.Equal(string.Empty, trockeneis.Gefahrzettel);
        Assert.Equal("Klasse 9 · unterliegt nicht dem ADR", trockeneis.Merkmale);
    }

    [Fact]
    public void A_forbidden_stoff_says_so()
    {
        var trioxid = Single("2421");

        Assert.Equal(StoffStatus.BefoerderungVerboten, trioxid.Status);
        Assert.Equal("Klasse 2 · Beförderung verboten", trioxid.Merkmale);
    }

    [Fact]
    public void The_2023_amendments_are_applied()
    {
        Assert.False(TabelleA.TryFind("1169", out _));
        Assert.Equal("BUTEN", Single("1012").Benennung);
        Assert.Equal("66", Single("3550").Gefahrnummer);

        var ethylbromid = Single("1891");
        Assert.Equal("3", ethylbromid.Klasse);
        Assert.Equal("3+6.1", ethylbromid.Gefahrzettel);
        Assert.Equal("336", ethylbromid.Gefahrnummer);
    }

    [Fact]
    public void The_2025_amendments_are_applied()
    {
        Assert.Equal("DISILAN", Single("3553").Benennung);
        Assert.Equal("FEUERLÖSCHMITTEL-DISPERGIERVORRICHTUNGEN", Single("0514").Benennung);

        var fest = Single("3423");
        Assert.Equal("6.1", fest.Klasse);
        Assert.Equal("668", fest.Gefahrnummer);

        Assert.True(TabelleA.TryFind("1835", out var loesung));
        Assert.Equal(["86", "80"], loesung.Select(s => s.Gefahrnummer));
        Assert.All(loesung, s => Assert.Contains("WÄSSERIGE LÖSUNG", s.Benennung, StringComparison.Ordinal));
    }

    [Fact]
    public void Merkmale_spell_out_klasse_and_gefahrzettel()
    {
        Assert.Equal("Klasse 2 · Gefahrzettel 2.3 + 8", Single("1005").Merkmale);
        Assert.Equal("Klasse 7 · kein Gefahrzettel", TabelleA.All.First(s => s.UnNummer == "2908").Merkmale);
    }

    [Theory]
    [InlineData("1203", "1203")]
    [InlineData(" UN 1203 ", "1203")]
    [InlineData("un1203", "1203")]
    [InlineData("12", null)]
    [InlineData("120", null)]
    [InlineData("UN 12", null)]
    [InlineData("Benzin", null)]
    [InlineData("12034", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Reads_a_un_nummer_however_it_is_typed(string? query, string? expected)
    {
        Assert.Equal(expected, TabelleA.ReadNumber(query));
    }

    [Theory]
    [InlineData("9999")]
    [InlineData("12")]
    [InlineData("Benzin")]
    [InlineData(null)]
    public void Finds_nothing_for_an_unknown_or_partial_number(string? query)
    {
        Assert.False(TabelleA.TryFind(query, out _));
    }

    [Theory]
    [InlineData("1203")]
    [InlineData("UN 1203")]
    [InlineData("un1203")]
    public void A_un_nummer_finds_exactly_its_rows(string query)
    {
        var hits = TabelleA.Search(query);

        Assert.Equal("BENZIN oder OTTOKRAFTSTOFF", Assert.Single(hits).Benennung);
    }

    // Two or three digits are the upper number of a plate: they list no UN-Nummern that start with
    // them and search no Benennung, although names such as "1,1,1-TRICHLORETHAN" hold digits.
    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("33")]
    [InlineData("268")]
    [InlineData("UN 12")]
    [InlineData("12034")]
    [InlineData("9999")]
    public void Any_other_number_finds_nothing(string query)
    {
        Assert.Empty(TabelleA.Search(query));
    }

    [Fact]
    public void A_word_ignores_case_and_umlauts()
    {
        Assert.Contains(TabelleA.Search("ammoniak, wasserfrei"), s => s.UnNummer == "1005");
        Assert.Contains(TabelleA.Search("atzend"), s => s.Benennung.Contains("ÄTZEND", StringComparison.Ordinal));
    }

    [Fact]
    public void Names_that_begin_with_the_word_come_first()
    {
        var hits = TabelleA.Search("propan");

        Assert.StartsWith("PROPAN", hits[0].Benennung, StringComparison.Ordinal);
        Assert.Contains(hits, s => !s.Benennung.StartsWith("PROPAN", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void An_empty_search_finds_nothing(string? query)
    {
        Assert.Empty(TabelleA.Search(query));
    }

    [Fact]
    public void Names_carry_no_leftovers_from_the_typesetting()
    {
        foreach (var s in TabelleA.All)
        {
            Assert.DoesNotContain(" ,", s.Benennung, StringComparison.Ordinal);
            Assert.DoesNotContain(" )", s.Benennung, StringComparison.Ordinal);
            Assert.DoesNotContain("  ", s.Benennung, StringComparison.Ordinal);

            // "Industrie- oder", "KAUTSCHUK- (Gummi-)" and "VERSANDSTÜCK - LEERE" are the ADR's own.
            var rest = s.Benennung
                .Replace("- oder", string.Empty, StringComparison.Ordinal)
                .Replace("- und", string.Empty, StringComparison.Ordinal)
                .Replace("- (", string.Empty, StringComparison.Ordinal)
                .Replace(" - ", string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain("- ", rest, StringComparison.Ordinal);
        }

        // A hyphen at the end of a printed line is a break or part of the name; these are the hard cases.
        Assert.Contains(TabelleA.All, s => s.UnNummer == "1139" && s.Benennung.Contains("Industrie- oder", StringComparison.Ordinal));
        Assert.Equal("5-tert-BUTYL-2,4,6-TRINITRO-m-XYLEN (XYLENMOSCHUS)", Single("2956").Benennung);
        Assert.Equal("ETHYLENGLYCOLMONOETHYLETHER", Single("1171").Benennung);
        Assert.Equal("KALIUMQUECKSILBER(II)-CYANID", Single("1626").Benennung);
    }

    [Fact]
    public void The_reader_skips_comments_and_blank_lines_and_reads_every_column()
    {
        const string tsv = "# Kommentar\n\n1203\tBENZIN\t3\t3\t33\t\n0004\tAMMONIUMPIKRAT\t1\t1\t\t\n";

        var catalogue = TabelleA.Parse(new StringReader(tsv));

        Assert.Equal(2, catalogue.All.Count);
        Assert.Equal(new Stoff("1203", "BENZIN", "3", "3", "33", StoffStatus.Gefahrgut), catalogue.All[0]);
        Assert.Equal("33", catalogue.All[0].Tafel);
        Assert.Null(catalogue.All[1].Gefahrnummer);
        Assert.Equal(string.Empty, catalogue.All[1].Tafel);
        Assert.Same(catalogue.All[0], Assert.Single(catalogue.ByNumber["1203"]));
    }

    [Theory]
    [InlineData("verboten", StoffStatus.BefoerderungVerboten)]
    [InlineData("frei", StoffStatus.NichtAdr)]
    public void The_reader_maps_the_status_column(string cell, StoffStatus expected)
    {
        var catalogue = TabelleA.Parse(new StringReader("1845\tTROCKENEIS\t9\t\t\t" + cell + "\n"));

        Assert.Equal(expected, Assert.Single(catalogue.All).Status);
    }

    [Theory]
    [InlineData("1203\tBENZIN\t3\t3\t33")]
    [InlineData("1203\tBENZIN\t3\t3\t33\tunbekannt")]
    public void The_reader_rejects_a_malformed_row(string line)
    {
        Assert.Throws<InvalidDataException>(() => TabelleA.Parse(new StringReader(line)));
    }
}
