using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PlaylistLinkParser.Models;

namespace PlaylistLinkParser.Services;

public class ParserService
{
    private readonly HttpClient _httpClient;

    public ParserService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<string> GetHtmlAsync(string url)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred: {ex.Message}");
        }
    }

    public async Task<(PlaylistInfo Playlist, List<TrackInfo> Tracks)> ParsePlaylistAsync(string url)
    {
        string html = await GetHtmlAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var playlist = new PlaylistInfo();
        var tracks = new List<TrackInfo>();

        try
        {
            var titleNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
            playlist.Name = titleNode?.GetAttributeValue("content", "Unknown") ?? "Unknown";

            var descNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
            playlist.Description = descNode?.GetAttributeValue("content", string.Empty) ?? string.Empty;

            var imageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            playlist.AvatarUrl = imageNode?.GetAttributeValue("content", string.Empty) ?? string.Empty;

            var trackNodes = doc.DocumentNode.SelectNodes("//div[@role='row']");

            if (trackNodes != null)
            {
                foreach (var node in trackNodes)
                {
                    var track = new TrackInfo();

                    var titleNodeElement = node.SelectSingleNode(".//div[contains(@class, 'title')]//a");
                    track.Title = titleNodeElement != null ? HtmlEntity.DeEntitize(titleNodeElement.InnerText).Trim() : "Unknown";

                    var artistNode = node.SelectSingleNode(".//div[contains(@class, 'artist')]//a");
                    track.Artist = artistNode != null ? HtmlEntity.DeEntitize(artistNode.InnerText).Trim() : "Unknown";

                    var albumNode = node.SelectSingleNode(".//div[contains(@class, 'album')]//a");
                    track.Album = albumNode != null ? HtmlEntity.DeEntitize(albumNode.InnerText).Trim() : "Unknown";

                    var durationNode = node.SelectSingleNode(".//div[contains(@class, 'duration')]//time") ?? 
                                       node.SelectSingleNode(".//div[contains(@class, 'duration')]");
                    track.Duration = durationNode != null ? HtmlEntity.DeEntitize(durationNode.InnerText).Trim() : "0:00";

                    if (track.Title != "Unknown")
                    {
                        tracks.Add(track);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Parsing error: {ex.Message}");
        }

        return (playlist, tracks);
    }
}