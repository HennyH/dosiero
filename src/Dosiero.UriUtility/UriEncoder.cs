using System.Buffers.Text;
using System.Text;

namespace Dosiero.UriUtility;

public static class UriEncoder
{
    public static string ToBase64Uri(this Uri uri)
    {
        var uriString = uri.ToString();
        var bytes = Encoding.UTF8.GetBytes(uriString);
        var utf8Base64Bytes = Base64Url.EncodeToUtf8(bytes);
        var base64Url = Encoding.UTF8.GetString(utf8Base64Bytes);
        return base64Url;
    }

    public static Uri FromBase64Uri(this string base64Url)
    {
        var utf8Base64Bytes = Encoding.UTF8.GetBytes(base64Url);
        var bytes = Base64Url.DecodeFromUtf8(utf8Base64Bytes);
        var uriString = Encoding.UTF8.GetString(bytes);
        var uri = new Uri(uriString);
        return uri;
    }
}
