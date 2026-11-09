namespace FwAssist.Helpers.DekonPlatz.Tests;

public class DekonvorgabenTests
{
    private static readonly string[] KnownSources = ["TK", "BBK", "HB", "Maintainer"];

    public static TheoryData<string> Stufen => new() { "Eins", "Zwei", "Drei" };

    private static IReadOnlyList<Section> Stufe(string name) => name switch
    {
        "Eins" => Dekonvorgaben.StufeEins,
        "Zwei" => Dekonvorgaben.StufeZwei,
        _ => Dekonvorgaben.StufeDrei,
    };

    private static List<string> Texts(IReadOnlyList<Section> sections) =>
        sections.SelectMany(s => s.Lines).Select(l => l.Text)
            .Concat(sections.SelectMany(s => s.Drawings).Select(d => d.Description))
            .ToList();

    private static bool Mentions(IReadOnlyList<Section> sections, string figure) =>
        Texts(sections).Exists(t => t.Contains(figure, StringComparison.Ordinal));

    private static List<string> Steps(IReadOnlyList<Section> sections, string title) =>
        sections.Single(s => s.Title == title).Lines.Select(l => l.Text).ToList();

    private static bool IsKnownSource(string part) =>
        Array.Exists(KnownSources, known => part.StartsWith(known, StringComparison.Ordinal));

    [Theory]
    [InlineData("9 x 4 m")]
    [InlineData("8 x 4 m")]
    public void A_stufe_zwei_size_appears_only_under_stufe_zwei(string size)
    {
        Assert.True(Mentions(Dekonvorgaben.StufeZwei, size));
        Assert.False(Mentions(Dekonvorgaben.StufeEins, size));
        Assert.False(Mentions(Dekonvorgaben.StufeDrei, size));
    }

    [Fact]
    public void The_stufe_drei_size_appears_only_under_stufe_drei()
    {
        Assert.True(Mentions(Dekonvorgaben.StufeDrei, "10 m x 25 m"));
        Assert.False(Mentions(Dekonvorgaben.StufeEins, "10 m x 25 m"));
        Assert.False(Mentions(Dekonvorgaben.StufeZwei, "10 m x 25 m"));
    }

    [Fact]
    public void Stufe_eins_lists_its_seven_measures_in_order()
    {
        var measures = Steps(Dekonvorgaben.StufeEins, "Maßnahmen");

        Assert.Equal(7, measures.Count);
        Assert.All(measures, (step, i) => Assert.StartsWith($"{i + 1}. ", step, StringComparison.Ordinal));
    }

    [Fact]
    public void Stufe_zwei_has_the_eleven_confirmed_steps_in_order()
    {
        var steps = Steps(Dekonvorgaben.StufeZwei, "Aufbau");

        Assert.Equal(11, steps.Count);
        Assert.All(steps, (step, i) => Assert.StartsWith($"{i + 1}. ", step, StringComparison.Ordinal));
    }

    [Fact]
    public void The_shoe_tub_comes_directly_before_the_big_tub()
    {
        var steps = Steps(Dekonvorgaben.StufeZwei, "Aufbau");

        var shoes = steps.FindIndex(s => s.Contains("Schuhe", StringComparison.Ordinal));
        var tub = steps.FindIndex(s => s.Contains("Große Wanne", StringComparison.Ordinal));

        Assert.Equal(shoes + 1, tub);
    }

    [Fact]
    public void Stufe_zwei_shows_both_layouts()
    {
        var drawings = Dekonvorgaben.StufeZwei.SelectMany(s => s.Drawings).Select(d => d.Kind);

        Assert.Equal([DrawingKind.DreiBereiche, DrawingKind.SchwarzWeiss], drawings);
    }

    [Fact]
    public void Stufe_drei_draws_the_flow_and_says_it_has_no_site_plan()
    {
        var drawing = Assert.Single(Dekonvorgaben.StufeDrei.SelectMany(s => s.Drawings));

        Assert.Equal(DrawingKind.AblaufStufeDrei, drawing.Kind);
        Assert.Contains("Kein Lageplan", drawing.Description, StringComparison.Ordinal);
        Assert.True(Mentions(Dekonvorgaben.StufeDrei, "legt der Einsatzführer fest"));
    }

    [Theory]
    [MemberData(nameof(Stufen))]
    public void Every_line_and_drawing_names_a_listed_kind_of_source(string name)
    {
        var sections = Stufe(name);
        var sources = sections.SelectMany(s => s.Lines).Select(l => l.Source)
            .Concat(sections.SelectMany(s => s.Drawings).Select(d => d.Source));

        Assert.All(sources, source => Assert.All(source.Split(", "), part => Assert.True(IsKnownSource(part), part)));
    }

    [Theory]
    [MemberData(nameof(Stufen))]
    public void Every_drawing_describes_itself(string name) =>
        Assert.All(Stufe(name).SelectMany(s => s.Drawings), d => Assert.StartsWith("Zeichnung", d.Description, StringComparison.Ordinal));

    [Theory]
    [InlineData("Fürstenfeldbruck")]
    [InlineData("FFB")]
    [InlineData("Unterpfaffenhofen")]
    [InlineData("Germering")]
    public void No_wehr_or_place_from_the_source_reaches_the_screen(string name)
    {
        var shown = Texts(Dekonvorgaben.StufeEins)
            .Concat(Texts(Dekonvorgaben.StufeZwei))
            .Concat(Texts(Dekonvorgaben.StufeDrei))
            .Concat(Dekonvorgaben.Sources);

        Assert.DoesNotContain(shown, t => t.Contains(name, StringComparison.Ordinal));
    }
}
