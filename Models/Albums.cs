namespace SpotifyNewReleases.Models;

public class Albums
{
    public string href { get; set; } = String.Empty;
    public List<Item> items { get; set; } = new List<Item>();
    public int limit { get; set; }
    public string next { get; set; } = String.Empty;
    public int offset { get; set; }
    public object previous { get; set; } = new object();
    public int total { get; set; }
}
