using LanguageExt;
using LanguageExt.Common;
using MongoDB.Driver;
using SpotifyNewReleases.Exceptions;
using SpotifyNewReleases.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SpotifyNewReleases.Enums;

namespace SpotifyNewReleases.Repositories;

public class SpotifyRepository : ISpotifyRepository
{
    private readonly HttpClient _httpClient;

    public SpotifyRepository()
    {
        _httpClient = new();
    }

    public async Task<Result<List<Item>>> GetAllLatestReleases(SpotifyToken token)
    {
        List<Item> allReleases = new List<Item>();
        foreach (string country in countries)
        {
            Result<List<Item>> receivedReleases = await this.GetLastReleasesByCountry(country, token);
            receivedReleases.IfFail(Console.WriteLine);
            receivedReleases.IfSucc(allReleases.AddRange);
        }
        return new Result<List<Item>>(allReleases
            .DistinctBy(release => release.id)
            .OrderBy(release => release.release_date)
            .Reverse()
            .ToList());
    }

    private async Task<Result<List<Item>>> GetLastReleasesByCountry(string country, SpotifyToken token)
    {
        string spotifyGetReleaseByCountryUrl = Environment.GetEnvironmentVariable("spotifyGetReleaseByCountryUrl") ?? String.Empty;
        Uri path = new Uri($"{spotifyGetReleaseByCountryUrl}{country}&limit={50}");

        this.ConfigureHttpClientResponseType();
        this.ConfigureHttpClientAuthorization(AuthenticationScheme.Bearer, token.access_token);

        //pour une recherche d'album, il y a ça :
        //https://api.spotify.com/v1/search?q=album%3ARendezvous%20artist%3AJenevieve&type=album&limit=50
        //album:Rendezvous artist:Jenevieve
        //type:album (le type de media qu'il faut sortir, on peut remplacer par artist par exemple

        HttpResponseMessage response = await this._httpClient.GetAsync(path);
        string responseContent = await response.Content.ReadAsStringAsync();
        try
        {
            SpotifyReleases? deserializedJson = JsonSerializer.Deserialize<SpotifyReleases>(responseContent);
            if (deserializedJson is null || deserializedJson?.albums?.items is null)
            {
                return new Result<List<Item>>(new DeserializationException());
            }
            return new Result<List<Item>>(deserializedJson.albums.items);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return new Result<List<Item>>(new DeserializationException(exception));
        }

    }

    public async Task<Result<SpotifyToken>> GetSpotifyToken()
    {
        string clientId = Environment.GetEnvironmentVariable("spotifyClientId") ?? string.Empty;
        string clientSecret = Environment.GetEnvironmentVariable("spotifyClientSecret") ?? string.Empty;
        string url = Environment.GetEnvironmentVariable("spotifyUrl") ?? string.Empty;

        ConfigureHttpClientResponseType();
        ConfigureHttpClientAuthorization(AuthenticationScheme.Basic, GetCredentials(clientId, clientSecret));
        FormUrlEncodedContent requestBody = GetRequestBody();
        //Request Token
        var request = await _httpClient.PostAsync(url, requestBody);
        if (!request.IsSuccessStatusCode)
        {
            return new Result<SpotifyToken>(new SpotifyTokenException("Error on http request"));
        }
        var response = await request.Content.ReadAsStringAsync();
        SpotifyToken? token = JsonSerializer.Deserialize<SpotifyToken>(response);
        if (token is null)
        {
            return new Result<SpotifyToken>(new SpotifyTokenException("Error on deserialization"));
        }
        return new Result<SpotifyToken>(token);
    }

    private void ConfigureHttpClientResponseType()
    {
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private void ConfigureHttpClientAuthorization(AuthenticationScheme scheme, string parameter)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme.ToString(), parameter);
    }

    private static string GetCredentials(string clientId, string clientSecret)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", clientId, clientSecret)));
    }

    private static FormUrlEncodedContent GetRequestBody()
    {
        List<KeyValuePair<string, string>> requestData = new()
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        };
        return new FormUrlEncodedContent(requestData);
    }

    public List<string> countries = new List<string>() {
        "AD","AE","AG","AL","AM","AO","AR","AT","AU",
        "AZ","BA","BB","BD","BE","BF","BG","BH","BI",
        "BJ","BN","BO","BR","BS","BT","BW","BZ","CA",
        "CD","CG","CH","CI","CL","CM","CO","CR","CV",
        "CW","CY","CZ","DE","DJ","DK","DM","DO","DZ",
        "EC","EE","EG","ES","FI","FJ","FM","FR","GA",
        "GB","GD","GE","GH","GM","GN","GQ","GR","GT",
        "GW","GY","HK","HN","HR","HT","HU","ID","IE",
        "IL","IN","IQ","IS","IT","JM","JO","JP","KE",
        "KG","KH","KI","KM","KN","KR","KW","KZ","LA",
        "LB","LC","LI","LK","LR","LS","LT","LU","LV",
        "LY","MA","MC","MD","ME","MG","MH","MK","ML",
        "MN","MO","MR","MT","MU","MV","MW","MX","MY",
        "MZ","NA","NE","NG","NI","NL","NO","NP","NR",
        "NZ","OM","PA","PE","PG","PH","PK","PL","PS",
        "PT","PW","PY","QA","RO","RS","RW","SA","SB",
        "SC","SE","SG","SI","SK","SL","SM","SN","SR",
        "ST","SV","SZ","TD","TG","TH","TJ","TL","TN",
        "TO","TR","TT","TV","TW","TZ","UA","UG","US",
        "UY","UZ","VC","VE","VN","VU","WS","XK","ZA",
        "ZM","ZW"};
}
