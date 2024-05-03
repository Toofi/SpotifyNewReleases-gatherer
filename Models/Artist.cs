namespace SpotifyNewReleases.Models;

public class Artist
{
    public ExternalUrls? external_urls { get; set; }
    public string href { get; set; } = String.Empty;
    public string id { get; set; } = String.Empty;
    public string name { get; set; } = String.Empty;
    public string type { get; set; } = String.Empty;
    public string uri { get; set; } = String.Empty;
}
