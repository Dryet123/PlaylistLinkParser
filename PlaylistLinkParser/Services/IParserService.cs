using System.Collections.Generic;
using System.Threading.Tasks;
using PlaylistLinkParser.Models;

namespace PlaylistLinkParser.Services;

public interface IParserService
{
    Task<(PlaylistInfo Playlist, List<TrackInfo> Tracks)> ParsePlaylistAsync(string url);
}