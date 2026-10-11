using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SilverbridgeWeb.Modules.Matches.Presentation;

public static class TeamTallySignatureVerifier
{
    private static readonly TimeSpan s_tolerance = TimeSpan.FromMinutes(5);

    public static bool Verify(ReadOnlySpan<byte> body, string? signatureHeader, string? secret, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        string[] components = signatureHeader.Split(',', StringSplitOptions.None);
        if (components.Length < 2 ||
            !components[0].StartsWith("t=", StringComparison.Ordinal) ||
            !long.TryParse(components[0].AsSpan(2), NumberStyles.None, CultureInfo.InvariantCulture, out long timestamp))
        {
            return false;
        }

        DateTimeOffset signedAt;
        try
        {
            signedAt = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        DateTimeOffset now = new(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
        if (signedAt < now - s_tolerance || signedAt > now + s_tolerance)
        {
            return false;
        }

        var signatures = new List<byte[]>();
        foreach (string component in components.Skip(1))
        {
            if (!component.StartsWith("v1=", StringComparison.Ordinal))
            {
                return false;
            }

            string encoded = component[3..];
            if (encoded.Length == 0 || encoded.Any(char.IsWhiteSpace))
            {
                return false;
            }

            byte[] decoded;
            try
            {
                decoded = Convert.FromBase64String(encoded);
            }
            catch (FormatException)
            {
                return false;
            }

            if (decoded.Length != 32 || !Convert.ToBase64String(decoded).Equals(encoded, StringComparison.Ordinal))
            {
                return false;
            }

            signatures.Add(decoded);
        }

        if (signatures.Count == 0)
        {
            return false;
        }

        byte[] timestampBytes = Encoding.ASCII.GetBytes(components[0][2..]);
        byte[] signedPayload = new byte[timestampBytes.Length + 1 + body.Length];
        timestampBytes.CopyTo(signedPayload, 0);
        signedPayload[timestampBytes.Length] = (byte)'.';
        body.CopyTo(signedPayload.AsSpan(timestampBytes.Length + 1));

        byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(secretBytes);
        byte[] expected = hmac.ComputeHash(signedPayload);

        bool matched = false;
        foreach (byte[] signature in signatures)
        {
            matched |= CryptographicOperations.FixedTimeEquals(expected, signature);
        }

        return matched;
    }
}
