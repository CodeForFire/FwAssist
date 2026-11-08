using System.Diagnostics.CodeAnalysis;
using Helfer.AppLogic.Services;

namespace Helfer.AppLogic.Tests;

[SuppressMessage("Design", "CA1054", Justification = "Tests exercise the validator with free-form (even hostile) strings; that is their point.")]
public class HttpUrlValidatorTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("https://example.com/path?query=1#fragment")]
    public void Accepts_a_well_formed_http_or_https_url(string url)
    {
        Assert.True(HttpUrlValidator.TryGetHttpUri(url, out var uri));
        Assert.Equal(url, uri.AbsoluteUri.TrimEnd('/'));
    }

    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.org")]
    [InlineData("intent://example.com#Intent;scheme=https;end")]
    [InlineData("content://media/external/images/1")]
    public void Refuses_a_scheme_other_than_http(string url) =>
        Assert.False(HttpUrlValidator.TryGetHttpUri(url, out _));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("example.com")]
    public void Refuses_anything_that_is_not_an_absolute_url(string? url) =>
        Assert.False(HttpUrlValidator.TryGetHttpUri(url, out _));
}
