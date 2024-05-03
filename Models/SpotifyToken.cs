namespace SpotifyNewReleases.Models;

public record SpotifyToken(string access_token, string token_type, long expires_in);
