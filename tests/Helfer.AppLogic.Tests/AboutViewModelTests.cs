using Helfer.AppLogic.ViewModels;
using Helfer.Testing;

namespace Helfer.AppLogic.Tests;

public class AboutViewModelTests
{
    private const string Failed = "Link konnte nicht geöffnet werden.";

    private readonly FakeExternalLinks _links = new();

    private AboutViewModel About() => new(_links, "1.2.3");

    [Fact]
    public void Shows_the_version_it_was_given() =>
        Assert.Equal("1.2.3", About().VersionText);

    [Fact]
    public void The_link_opens_the_projects_repository()
    {
        var about = About();

        about.OpenRepositoryCommand.Execute(null);

        Assert.Equal(new Uri("https://www.github.com/codeforfire/helfer"), Assert.Single(_links.Opened));
        Assert.Null(about.Error);
    }

    [Fact]
    public void A_link_the_platform_cannot_open_says_so()
    {
        _links.Refuse = true;
        var about = About();

        about.OpenRepositoryCommand.Execute(null);

        Assert.Equal(Failed, about.Error);
    }

    [Fact]
    public void A_later_success_clears_the_failure()
    {
        _links.Refuse = true;
        var about = About();
        about.OpenRepositoryCommand.Execute(null);

        _links.Refuse = false;
        about.OpenRepositoryCommand.Execute(null);

        Assert.Null(about.Error);
    }

    [Fact]
    public void An_address_that_is_not_http_never_reaches_the_platform()
    {
        var about = new AboutViewModel(_links, "1.2.3") { RepositoryText = "intent://example.com#Intent;scheme=https;end" };

        about.OpenRepositoryCommand.Execute(null);

        Assert.Empty(_links.Opened);
        Assert.Equal(Failed, about.Error);
    }

    [Fact]
    public void A_null_version_is_refused() =>
        Assert.Throws<ArgumentNullException>(() => new AboutViewModel(_links, null!));

    [Fact]
    public void Null_links_are_refused() =>
        Assert.Throws<ArgumentNullException>(() => new AboutViewModel(null!, "1.2.3"));
}
