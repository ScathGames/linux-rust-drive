using System.Net.Mime;
using Microsoft.AspNetCore.StaticFiles;

namespace ProtonDrive.Client.MediaTypes;

internal sealed class FileContentTypeProvider : IFileContentTypeProvider
{
    private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider = new()
    {
        Mappings =
        {
            { ".apk", "application/vnd.android.package-archive" },
            { ".apng", "image/apng" },
            { ".arc", "application/x-freearc" },
            { ".avif", "image/avif" },
            { ".bzip2", "application/x-bzip2" },
            { ".cr3", "image/x-canon-cr3" },
            { ".epub", "application/epub+zip" },
            { ".flac", "audio/flac" },
            { ".gzip", "application/gzip" },
            { ".heic", "image/heic" },
            { ".heics", "image/heic-sequence" },
            { ".heif", "image/heif" },
            { ".heifs", "image/heif-sequence" },
            { ".keynote", "application/vnd.apple.keynote" },
            { ".mp1s", "video/mp1s" },
            { ".mp2p", "video/mp2p" },
            { ".mp2t", "video/mp2t" },
            { ".mp4a", "audio/mp4" },
            { ".numbers", "application/vnd.apple.numbers" },
            { ".odp", "application/vnd.oasis.opendocument.presentation" },
            { ".odt", "application/vnd.oasis.opendocument.text" },
            { ".opus", "audio/opus" },
            { ".pages", "application/vnd.apple.pages" },
            { ".qcp", "audio/qcelp" },
            { ".v3g2", "video/3gpp2" },
            { ".v3gp", "video/3gpp" },
            { ".x7zip", "application/x-7z-compressed" },
        },
    };

    public string GetContentType(string filename)
    {
        return _fileExtensionContentTypeProvider.TryGetContentType(filename, out var contentType)
            ? contentType
            : MediaTypeNames.Application.Octet;
    }
}
