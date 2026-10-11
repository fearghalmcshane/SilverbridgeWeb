using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SilverbridgeWeb.Modules.Matches.Presentation;

public static class TeamTallySignatureVerifier
{
    private static readonly TimeSpan s_tolerance = TimeSpan.FromMinutes(5);

    public static bool Verify(ReadOnlySpan<byte> body, string? signatureHeader, string? secret, DateTime utcNow)
    {
        return Verify(body, signatureHeader, secret, utcNow, out _);
    }

    public static bool Verify(
        ReadOnlySpan<byte> body,
        string? signatureHeader,
        string? secret,
        DateTime utcNow,
        out string rejectionReason)
    {
        rejectionReason = "Missing signature header or signing secret";
        if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        rejectionReason = "Malformed signature header";
        string[] components = signatureHeader.Split(',', StringSplitOptions.TrimEntries);
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
            rejectionReason = "Signature timestamp outside five-minute tolerance";
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
                decoded = encoded.Length == 64
                    ? Convert.FromHexString(encoded)
                    : Convert.FromBase64String(encoded);
            }
            catch (FormatException)
            {
                return false;
            }

            if (decoded.Length != 32 ||
                encoded.Length != 64 && !Convert.ToBase64String(decoded).Equals(encoded, StringComparison.Ordinal))
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

        rejectionReason = matched ? string.Empty : "Signature digest mismatch";
        return matched;
    }
}
