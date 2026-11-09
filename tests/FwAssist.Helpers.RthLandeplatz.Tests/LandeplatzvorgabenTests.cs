namespace FwAssist.Helpers.RthLandeplatz.Tests;

public class LandeplatzvorgabenTests
{
    public static TheoryData<string> Modes => new() { "Tag", "Nacht" };

    private static IReadOnlyList<Section> Mode(string name) =>
        name == "Tag" ? Landeplatzvorgaben.Tag : Landeplatzvorgaben.Nacht;

    private static List<string> Texts(IReadOnlyList<Section> sections) =>
        sections.SelectMany(s => s.Lines).Select(l => l.Text)
            .Concat(sections.SelectMany(s => s.Drawings).Select(d => d.Description))
            .ToList();

    private static List<string> Phases(IReadOnlyList<Section> sections, string text) =>
        sections.Where(s => s.Lines.Any(l => l.Text.Contains(text, StringComparison.Ordinal)))
            .Select(s => s.Title)
            .ToList();

    [Theory]
    [InlineData("35 m x 70 m")]
    [InlineData("ca. 250 m")]
    public void A_night_figure_never_appears_by_day(string figure)
    {
        Assert.DoesNotContain(Texts(Landeplatzvorgaben.Tag), t => t.Contains(figure, StringComparison.Ordinal));
        Assert.Contains(Texts(Landeplatzvorgaben.Nacht), t => t.Contains(figure, StringComparison.Ordinal));
    }

    [Fact]
    public void The_day_size_never_appears_by_night()
    {
        Assert.Contains(Texts(Landeplatzvorgaben.Tag), t => t.Contains("30 m x 30 m", StringComparison.Ordinal));
        Assert.DoesNotContain(Texts(Landeplatzvorgaben.Nacht), t => t.Contains("30 m x 30 m", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Modes))]
    public void Both_modes_show_the_approach_sector_ratio(string mode) =>
        Assert.Contains(Texts(Mode(mode)), t => t.Contains("6:1, also 20 m Höhe auf 120 m Länge", StringComparison.Ordinal));

    [Fact]
    public void The_arm_signals_stay_in_their_own_phases()
    {
        Assert.Equal(["3. Anflug"], Phases(Landeplatzvorgaben.Tag, "Arme als V"));
        Assert.Empty(Phases(Landeplatzvorgaben.Nacht, "Arme als V"));
        Assert.Equal(["5. Nach der Landung"], Phases(Landeplatzvorgaben.Tag, "Arme unten"));
        Assert.Equal(["5. Nach der Landung"], Phases(Landeplatzvorgaben.Nacht, "Arme unten"));
    }

    [Fact]
    public void The_steps_at_the_helicopter_are_the_same_in_both_modes()
    {
        string[] titles = ["5. Nach der Landung", "6. Vor dem Start"];

        var tag = Landeplatzvorgaben.Tag.Where(s => titles.Contains(s.Title)).ToList();
        var nacht = Landeplatzvorgaben.Nacht.Where(s => titles.Contains(s.Title)).ToList();

        Assert.Equal(2, tag.Count);
        Assert.Equal(tag, nacht);
    }

    [Theory]
    [MemberData(nameof(Modes))]
    public void Both_modes_start_with_the_same_block_and_the_road(string mode) =>
        Assert.Equal(["Zuerst", "Straße"], Mode(mode).Take(2).Select(s => s.Title));

    [Theory]
    [MemberData(nameof(Modes))]
    public void Both_modes_end_with_the_notes_that_are_no_phase(string mode) =>
        Assert.Equal(["Brandschutz", "Vorbereitung vor dem Einsatz"], Mode(mode).TakeLast(2).Select(s => s.Title));

    [Theory]
    [MemberData(nameof(Modes))]
    public void The_position_goes_to_the_leitstelle_before_it_is_asked_to_pass_it_on(string mode)
    {
        var lines = Mode(mode)[0].Lines.Select(l => l.Text).ToList();

        var send = lines.FindIndex(t => t.Contains("# lang drücken", StringComparison.Ordinal));
        var relay = lines.FindIndex(t => t.StartsWith("Danach die Leitstelle bitten", StringComparison.Ordinal));

        Assert.InRange(send, 0, relay - 1);
    }

    [Fact]
    public void A_single_landing_needs_no_brandschutz_and_co2_only_adds_to_the_powder()
    {
        var lines = Landeplatzvorgaben.Tag.Single(s => s.Title == "Brandschutz").Lines;

        Assert.Equal("Für eine einzelne Landung ist kein Brandschutz nötig.", lines[0].Text);
        Assert.False(lines[0].IsSubItem);
        Assert.Contains("2-facher Brandschutz", lines[1].Text, StringComparison.Ordinal);
        Assert.False(lines[1].IsSubItem);
        Assert.Equal(
            ["Wasser-Schaum: mindestens 500 l Wasser, Ausstoß 250 l/min Wasser-Schaummittel-Gemisch", "ABC-Pulver: 24 kg", "Optional zum Pulver: CO2-Löscher"],
            lines.Where(l => l.IsSubItem).Select(l => l.Text));
    }

    [Theory]
    [MemberData(nameof(Modes))]
    public void Every_line_and_drawing_names_its_source(string mode)
    {
        foreach (var section in Mode(mode))
        {
            Assert.All(section.Lines, l => Assert.False(string.IsNullOrWhiteSpace(l.Source), l.Text));
            Assert.All(section.Drawings, d => Assert.False(string.IsNullOrWhiteSpace(d.Source), d.Description));
            Assert.All(section.Drawings, d => Assert.False(string.IsNullOrWhiteSpace(d.Description), d.Kind.ToString()));
        }
    }

    [Fact]
    public void Every_drawing_appears_once_and_each_landing_site_only_in_its_mode()
    {
        var tag = Landeplatzvorgaben.Tag.SelectMany(s => s.Drawings).Select(d => d.Kind).ToList();
        var nacht = Landeplatzvorgaben.Nacht.SelectMany(s => s.Drawings).Select(d => d.Kind).ToList();

        Assert.Equal(tag.Count, tag.Distinct().Count());
        Assert.Equal(nacht.Count, nacht.Distinct().Count());
        Assert.Contains(DrawingKind.LandeplatzTag, tag);
        Assert.DoesNotContain(DrawingKind.LandeplatzTag, nacht);
        Assert.Contains(DrawingKind.LandeplatzNacht, nacht);
        Assert.DoesNotContain(DrawingKind.LandeplatzNacht, tag);
        Assert.Equal(Enum.GetValues<DrawingKind>().Order(), tag.Union(nacht).Order());
    }

    [Fact]
    public void The_road_drawing_is_labelled_as_the_example_it_is()
    {
        var road = Landeplatzvorgaben.Tag.SelectMany(s => s.Drawings).Single(d => d.Kind == DrawingKind.RotorUeberStrasse);

        Assert.Contains("Beispiel H145", road.Description, StringComparison.Ordinal);
    }

    [Fact]
    public void The_screen_claims_neither_a_clearance_nor_an_endorsement()
    {
        Assert.StartsWith("Die Entscheidung trifft der Pilot", Landeplatzvorgaben.PilotDecides, StringComparison.Ordinal);
        Assert.Contains("Keine dieser Stellen hat diese App geprüft", Landeplatzvorgaben.SourceNote, StringComparison.Ordinal);
    }

    [Fact]
    public void The_abort_rule_says_to_turn_the_light_off() =>
        Assert.EndsWith("Licht am Landeplatz aus!", Landeplatzvorgaben.AbortRule, StringComparison.Ordinal);

    [Fact]
    public void The_night_runs_from_after_sunset_to_before_sunrise() =>
        Assert.Contains(
            "30 Minuten nach Sonnenuntergang bis 30 Minuten vor Sonnenaufgang",
            Landeplatzvorgaben.NightDefinition,
            StringComparison.Ordinal);

    [Fact]
    public void The_day_runs_from_before_sunrise_to_after_sunset() =>
        Assert.Contains(
            "30 Minuten vor Sonnenaufgang bis 30 Minuten nach Sonnenuntergang",
            Landeplatzvorgaben.DayDefinition,
            StringComparison.Ordinal);
}
