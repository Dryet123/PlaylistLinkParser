using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace PlaylistLinkParser.Services;

public static class ImageHelper
{
    private static readonly HttpClient _httpClient = new();

    public static async Task<Bitmap?> LoadFromWebAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            var data = await _httpClient.GetByteArrayAsync(url);
            using var stream = new MemoryStream(data);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}