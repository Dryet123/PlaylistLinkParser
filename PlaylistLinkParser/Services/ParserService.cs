using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PlaylistLinkParser.Models;

namespace PlaylistLinkParser.Services;

public class ParserService
{
    private static readonly HttpClient _httpClient;

    static ParserService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<(PlaylistInfo Playlist, List<TrackInfo> Tracks)> ParsePlaylistAsync(string url)
    {
        var playlist = new PlaylistInfo();
        var tracks = new List<TrackInfo>();

        try
        {
            string html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var titleNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']");
            playlist.Name = titleNode?.GetAttributeValue("content", "Unknown") ?? "Unknown";

            var descNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
            playlist.Description = descNode?.GetAttributeValue("content", string.Empty) ?? string.Empty;

            var imageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            playlist.AvatarUrl = imageNode?.GetAttributeValue("content", string.Empty) ?? string.Empty;

            string playlistId = url.Split(new[] { '?', '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
            
            int offset = 0;
            int limit = 50;
            bool hasMoreTracks = true;

            while (hasMoreTracks)
            {
                string apiUrl = $"https://tidal.com/v1/playlists/{playlistId}/items?countryCode=US&limit={limit}&offset={offset}";

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Add("x-tidal-token", "txNoH4kkV41MfH25");

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                string json = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(json);
                
                if (jsonDoc.RootElement.TryGetProperty("items", out var itemsArray) && itemsArray.ValueKind == JsonValueKind.Array)
                {
                    int currentBatchCount = itemsArray.GetArrayLength();

                    foreach (var arrayItem in itemsArray.EnumerateArray())
                    {
                        if (arrayItem.TryGetProperty("item", out var trackItem))
                        {
                            var track = new TrackInfo();

                            if (trackItem.TryGetProperty("title", out var titleProp))
                            {
                                track.Title = titleProp.GetString() ?? "Unknown";
                            }

                            if (trackItem.TryGetProperty("artists", out var artistsArray) && artistsArray.ValueKind == JsonValueKind.Array && artistsArray.GetArrayLength() > 0)
                            {
                                if (artistsArray[0].TryGetProperty("name", out var artistNameProp))
                                {
                                    track.Artist = artistNameProp.GetString() ?? "Unknown";
                                }
                            }

                            if (trackItem.TryGetProperty("album", out var albumObj) && albumObj.TryGetProperty("title", out var albumTitleProp))
                            {
                                track.Album = albumTitleProp.GetString() ?? "Unknown";
                            }

                            if (trackItem.TryGetProperty("duration", out var durationProp) && durationProp.TryGetInt32(out var seconds))
                            {
                                var timeSpan = TimeSpan.FromSeconds(seconds);
                                track.Duration = $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}";
                            }

                            if (!string.IsNullOrEmpty(track.Title) && track.Title != "Unknown")
                            {
                                tracks.Add(track);
                            }
                        }
                    }

                    if (currentBatchCount < limit)
                    {
                        hasMoreTracks = false;
                    }
                    else
                    {
                        offset += limit;
                    }
                }
                else
                {
                    hasMoreTracks = false;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Parsing error: {ex.Message}");
        }

        return (playlist, tracks);
    }
}