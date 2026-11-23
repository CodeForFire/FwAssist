namespace FwAssist.Helpers.Sandsack.Tests;

public class SandsackRechenwegTests
{
    [Fact]
    public void Without_values_every_step_names_its_rule_and_the_tables_figures()
    {
        var zeilen = Zeilen(null, null, null);

        Assert.Equal(
            [
                "Höhe: aufgerundet auf die nächste Stufe der Tabelle, 0,5 / 1,0 / 1,5 / 2,0 m.",
                "Sandsäcke: Länge × Säcke je m der Stufe (45 / 160 / 350 / 580), aufgerundet.",
                "Sand: Länge × m³ je m der Stufe (1 / 3 / 6 / 9,6), aufgerundet auf 0,1 m³.",
                "Sand, nass: m³ × 2,0 t/m³.",
                "Sand, trocken: 1,2 bis 1,6 t/m³. Nasser Sand wiegt bis zu 67 % mehr als trockener. "
                    + "Gerechnet wird mit nassem Sand, damit Ladung und Fahrten reichen.",
                "Arbeitszeit: Säcke × 2 Arbeitsgänge (Füllen und Tragen, Verlegen) ÷ 40 Säcke je Helfer und Stunde "
                    + "÷ Helfer, aufgerundet auf die Viertelstunde. Nur ganze Helfer zählen.",
                "Basisbreite: Höhe × 2.",
                SandsackRechenweg.SichereSeite,
            ],
            Zeilen(null, null, null));
        Assert.Equal(zeilen, Zeilen(100m, null, 10m));
    }

    [Fact]
    public void A_whole_calculation_shows_the_numbers_it_used()
    {
        Assert.Equal(
            [
                "Höhe: 1 m, gerechnet mit der Stufe 1,0 m (Stufen 0,5 / 1,0 / 1,5 / 2,0 m).",
                "Sandsäcke: 100 m × 160 je m = 16.000.",
                "Sand: 100 m × 3 m³ je m = 300,0 m³.",
                "Sand, nass: 300,0 m³ × 2,0 t/m³ = 600,0 t.",
                "Sand, trocken: 300,0 m³ × 1,2 bis 1,6 t/m³ = 360,0 bis 480,0 t. "
                    + "Nasser Sand wiegt bis zu 67 % mehr als trockener. "
                    + "Gerechnet wird mit nassem Sand, damit Ladung und Fahrten reichen.",
                "Arbeitszeit: 16.000 Säcke × 2 ÷ 40 je Helfer und Stunde ÷ 10 Helfer = 80 h.",
                "Basisbreite: 1 m × 2 = 2,0 m.",
                SandsackRechenweg.SichereSeite,
            ],
            Zeilen(100m, 1m, 10m));
    }

    [Fact]
    public void A_height_alone_shows_its_row_and_base_and_the_rules_for_the_rest()
    {
        var zeilen = Zeilen(null, 0.8m, null);

        Assert.Equal("Höhe: 0,8 m, gerechnet mit der Stufe 1,0 m (Stufen 0,5 / 1,0 / 1,5 / 2,0 m).", zeilen[0]);
        Assert.Equal("Sandsäcke: Länge × 160 je m, aufgerundet.", zeilen[1]);
        Assert.Equal("Basisbreite: 0,8 m × 2 = 1,6 m.", zeilen[6]);
    }

    [Fact]
    public void A_step_that_was_rounded_says_so()
    {
        // 10,5 m × 45 = 472,5 sacks; 10,5 m × 1 m³ = 10,5 m³; 473 × 2 ÷ 40 ÷ 4 = 5,9125 h.
        var zeilen = Zeilen(10.5m, 0.5m, 4m);

        Assert.Equal("Sandsäcke: 10,5 m × 45 je m = 473, aufgerundet.", zeilen[1]);
        Assert.Equal("Sand: 10,5 m × 1 m³ je m = 10,5 m³.", zeilen[2]);
        Assert.Equal(
            "Arbeitszeit: 473 Säcke × 2 ÷ 40 je Helfer und Stunde ÷ 4 Helfer = 6 h, aufgerundet auf die Viertelstunde.",
            zeilen[5]);
    }

    // 23 m × 160 = 3.680 sacks; × 2 ÷ 40 ÷ 32 = 5,75 h, an exact quarter hour, so nothing rounds.
    [Fact]
    public void A_time_that_ends_on_a_quarter_hour_is_not_called_rounded() =>
        Assert.Equal(
            "Arbeitszeit: 3.680 Säcke × 2 ÷ 40 je Helfer und Stunde ÷ 32 Helfer = 5 h 45 min.",
            Zeilen(23m, 1m, 32m)[5]);

    // 450 × 2 ÷ 40 ÷ 4 = 5,625 h: rounded up to 5 h 45 min.
    [Fact]
    public void A_time_between_quarter_hours_is_called_rounded() =>
        Assert.Equal(
            "Arbeitszeit: 450 Säcke × 2 ÷ 40 je Helfer und Stunde ÷ 4 Helfer = 5 h 45 min, aufgerundet auf die Viertelstunde.",
            Zeilen(10m, 0.5m, 4m)[5]);

    // 1,01 m × 3 m³ = 3,03, so 3,1 m³; × 1,2 and × 1,6 are 3,72 and 4,96 t.
    [Fact]
    public void A_dry_range_that_was_rounded_says_so() =>
        Assert.StartsWith(
            "Sand, trocken: 3,1 m³ × 1,2 bis 1,6 t/m³ = 3,8 bis 5,0 t, aufgerundet. ",
            Zeilen(1.01m, 1m, null)[4],
            StringComparison.Ordinal);

    [Fact]
    public void A_base_that_was_rounded_says_so() =>
        Assert.Equal("Basisbreite: 0,77 m × 2 = 1,6 m, aufgerundet.", Zeilen(null, 0.77m, null)[6]);

    [Fact]
    public void Part_of_a_helfer_is_dropped_and_the_line_says_why() =>
        Assert.Equal(
            "Arbeitszeit: 16.000 Säcke × 2 ÷ 40 je Helfer und Stunde ÷ 10 Helfer = 80 h. Nur ganze Helfer zählen.",
            Zeilen(100m, 1m, 10.5m)[5]);

    [Fact]
    public void Without_helfer_the_time_shows_the_sacks_and_the_rule() =>
        Assert.Equal(
            "Arbeitszeit: 16.000 Säcke × 2 ÷ 40 je Helfer und Stunde ÷ Helfer, aufgerundet auf die Viertelstunde.",
            Zeilen(100m, 1m, null)[5]);

    [Fact]
    public void The_list_of_rows_never_wraps_between_two_figures() =>
        Assert.Contains(
            "0,5 / 1,0 / 1,5 / 2,0 m",
            SandsackRechenweg.Zeilen(null, null, null)[0],
            StringComparison.Ordinal);

    [Fact]
    public void A_result_line_and_its_explanation_agree() =>
        Assert.Equal("5 h 45 min", SandsackRechenweg.Dauer(SandsackRechner.Arbeitszeit(450m, 4m)));

    // The lists are joined with no-break spaces; the expectations above are written with plain ones.
    private static string[] Zeilen(decimal? laengeMeter, decimal? hoeheMeter, decimal? helfer) =>
        [.. SandsackRechenweg.Zeilen(laengeMeter, hoeheMeter, helfer).Select(z => z.Replace(' ', ' '))];
}
