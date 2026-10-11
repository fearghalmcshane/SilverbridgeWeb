using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using SilverbridgeWeb.Modules.Matches.Presentation;

namespace SilverbridgeWeb.Modules.Matches.UnitTests;

public sealed class TeamTallySignatureVerifierTests
{
    private const string Secret = "raw-secret-é";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Verify_AcceptsHexDigestWithHeaderWhitespace(bool uppercase)
    {
        byte[] body = Encoding.UTF8.GetBytes("{ \"type\": \"ping\" }");
        DateTime now = DateTime.UnixEpoch.AddSeconds(1_791_127_264);
        const string timestamp = "1791127264";
        string signature = Convert.ToHexString(Convert.FromBase64String(CreateSignature(timestamp, body)));
        if (!uppercase)
        {
            signature = signature.ToLowerInvariant();
        }

        Assert.True(TeamTallySignatureVerifier.Verify(body, $"t={timestamp}, v1={signature}", Secret, now));
        Assert.False(TeamTallySignatureVerifier.Verify(body, $"t={timestamp},v1={signature}", "wrong-secret", now));
        Assert.False(TeamTallySignatureVerifier.Verify("{}"u8, $"t={timestamp},v1={signature}", Secret, now));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("v1=abc")]
    [InlineData("t=1791127264,v1=0123")]
    [InlineData("t=1791127264,v1=gggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg")]
    [InlineData("t=1791127264,v1=0000000000000000000000000000000000000000000000000000000000000000")]
    public void Verify_RejectsMissingMalformedOrIncorrectHexSignature(string? header)
    {
        Assert.False(TeamTallySignatureVerifier.Verify(
            "{}"u8, header, Secret, DateTime.UnixEpoch.AddSeconds(1_791_127_264)));
    }

    [Fact]
    public void Verify_AcceptsCorrectRawBodySignatureAndRotationSignature()
    {
        byte[] body = Encoding.UTF8.GetBytes("{\"club\":\"Cúil Óg\"}");
        DateTime now = DateTime.UnixEpoch.AddSeconds(1_791_127_264);
        string timestamp = new DateTimeOffset(now).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        string signature = CreateSignature(timestamp, body);
        string header = $"t={timestamp},v1={Convert.ToBase64String(new byte[32])},v1={signature}";
        DateTime futureSigningTime = now.AddMinutes(5);
        string futureTimestamp = new DateTimeOffset(futureSigningTime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        Assert.True(TeamTallySignatureVerifier.Verify(body, header, Secret, now));
        Assert.True(TeamTallySignatureVerifier.Verify(
            body,
            $"t={futureTimestamp},v1={CreateSignature(futureTimestamp, body)}",
            Secret,
            now));
    }

    [Fact]
    public void Verify_RejectsModifiedBodyExpiredTimestampAndMalformedHeader()
    {
        byte[] body = Encoding.UTF8.GetBytes("{\"score\":1}");
        DateTime now = DateTime.UnixEpoch.AddSeconds(1_791_127_264);
        string timestamp = new DateTimeOffset(now).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        string signature = CreateSignature(timestamp, body);

        Assert.False(TeamTallySignatureVerifier.Verify(
            Encoding.UTF8.GetBytes("{\"score\":2}"),
            $"t={timestamp},v1={signature}",
            Secret,
            now));
        Assert.False(TeamTallySignatureVerifier.Verify(
            body,
            $"t={timestamp},v1={signature}",
            Secret,
            now.AddMinutes(5).AddSeconds(1)));
        Assert.False(TeamTallySignatureVerifier.Verify(body, $"t={timestamp};v1={signature}", Secret, now));
        Assert.False(TeamTallySignatureVerifier.Verify(body, $"t={timestamp},v1=bad", Secret, now));
        Assert.False(TeamTallySignatureVerifier.Verify(body, $"t={timestamp},v1={signature},v2={signature}", Secret, now));
    }

    private static string CreateSignature(string timestamp, byte[] body)
    {
        byte[] signedBody = Encoding.UTF8.GetBytes(timestamp + ".").Concat(body).ToArray();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        return Convert.ToBase64String(hmac.ComputeHash(signedBody));
    }
}
