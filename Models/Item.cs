namespace SpotifyNewReleases.Models;

public class Item
{
    public string album_type { get; set; } = String.Empty;
    public List<Artist> artists { get; set; } = new List<Artist>();
    public List<string> available_markets { get; set; } = new List<string> { String.Empty };
    public ExternalUrls? external_urls { get; set; }
    public string href { get; set; } = String.Empty;
    public string id { get; set; } = String.Empty;
    public List<Image> images { get; set; } = new List<Image>();
    public string name { get; set; } = String.Empty;
    public string release_date { get; set; } = String.Empty;
    public string release_date_precision { get; set; } = String.Empty;
    public int total_tracks { get; set; }
    public string type { get; set; } = String.Empty;
    public string uri { get; set; } = String.Empty;
}
