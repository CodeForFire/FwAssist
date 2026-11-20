using System.Net;
using System.Text;

namespace FwAssist.Acceptance.Tests;

/// <summary>
/// Answers every request with a made-up Kartenpaket catalogue: the Fürstenfeldbruck pack
/// <see cref="FictionalRegionPack"/> installs, and a "Musterkreis" not on the device. No URL in it
/// is ever fetched; the acceptance tests only list it.
/// </summary>
internal sealed class FictionalCatalogue : HttpMessageHandler
{
    public const string Json = $$"""
        {"formatVersion": 2, "packs": [
          {"slug": "ffb", "name": "Landkreis Fürstenfeldbruck", "version": 1,
            "downloadUrl": "https://example.org/ffb-v1.zip", "sizeBytes": 52731904, "sha256": "{{Sha}}",
            "bounds": {"west": 10.9527, "south": 48.0607, "east": 11.4532, "north": 48.3237}, "minZoom": 0, "maxZoom": 15,
            "builtAt": "2026-10-27T20:15:49Z", "attribution": "{{FictionalRegionPack.Attribution}}"},
          {"slug": "mus", "name": "Landkreis Musterkreis", "version": 1,
            "downloadUrl": "https://example.org/mus-v1.zip", "sizeBytes": 88080384, "sha256": "{{OtherSha}}",
            "bounds": {"west": 11.4532, "south": 48.0607, "east": 11.9532, "north": 48.3237}, "minZoom": 0, "maxZoom": 15,
            "builtAt": "2026-10-30T20:15:49Z", "attribution": "{{FictionalRegionPack.Attribution}}"}]}
        """;

    private const string Sha = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    private const string OtherSha = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Json, Encoding.UTF8, "application/json") });
}
