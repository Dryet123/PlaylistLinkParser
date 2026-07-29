using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace PlaylistLinkParser.Services;

public static class ImageHelper
{
    public static async Task<Bitmap?> LoadFromWebAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        using var httpClient = new HttpClient();
        try
        {
            var data = await httpClient.GetByteArrayAsync(url);
            using var stream = new MemoryStream(data);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}