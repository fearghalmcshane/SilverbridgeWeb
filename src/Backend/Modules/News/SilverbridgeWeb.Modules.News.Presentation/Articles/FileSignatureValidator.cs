namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

/// <summary>
/// Validates uploaded media files by inspecting their leading "magic bytes" rather than trusting the
/// client-supplied file extension or <c>Content-Type</c> header, both of which are attacker-controlled and
/// can be spoofed to bypass extension/MIME-based checks.
/// </summary>
internal static class FileSignatureValidator
{
    private const int HeaderBytesToRead = 12;

    /// <summary>
    /// Reads the leading bytes of <paramref name="stream"/> and confirms they match a known signature for
    /// <paramref name="extension"/>. The stream position is reset to the start afterward so it can still be
    /// used for the actual upload.
    /// </summary>
    public static async Task<bool> MatchesExtensionAsync(Stream stream, string extension, CancellationToken cancellationToken)
    {
        if (!stream.CanSeek)
        {
            return false;
        }

        byte[] header = new byte[HeaderBytesToRead];
        int totalRead = 0;

        while (totalRead < header.Length)
        {
            int read = await stream.ReadAsync(header.AsMemory(totalRead, header.Length - totalRead), cancellationToken);

            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        stream.Seek(0, SeekOrigin.Begin);

        ReadOnlySpan<byte> bytes = header.AsSpan(0, totalRead);

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => StartsWith(bytes, [0xFF, 0xD8, 0xFF]),
            ".png" => StartsWith(bytes, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
            ".gif" => StartsWith(bytes, "GIF87a"u8) || StartsWith(bytes, "GIF89a"u8),
            ".webp" => StartsWith(bytes, "RIFF"u8) && bytes.Length >= 12 && bytes[8..12].SequenceEqual("WEBP"u8),
            ".mp4" or ".mov" => bytes.Length >= 8 && bytes[4..8].SequenceEqual("ftyp"u8),
            ".webm" => StartsWith(bytes, [0x1A, 0x45, 0xDF, 0xA3]),
            _ => false
        };
    }

    private static bool StartsWith(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> signature) =>
        bytes.Length >= signature.Length && bytes[..signature.Length].SequenceEqual(signature);
}
