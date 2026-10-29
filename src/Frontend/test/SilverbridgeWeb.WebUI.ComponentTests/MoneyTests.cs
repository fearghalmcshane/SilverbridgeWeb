using FluentAssertions;
using SilverbridgeWeb.WebUI.Services;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class MoneyTests
{
    [Theory]
    [InlineData(1250, "£12.50")]
    [InlineData(5, "£0.05")]
    [InlineData(100000, "£1,000.00")]
    public void FormatsPenceAsPounds(long amountMinor, string expected)
    {
        Money.Format(amountMinor, "GBP").Should().Be(expected);
    }

    [Theory]
    [InlineData("12.50", 1250)]
    [InlineData("£7", 700)]
    [InlineData(" 0.01 ", 1)]
    public void ParsesPoundsIntoPence(string input, long expected)
    {
        Money.TryParsePounds(input, out long amountMinor).Should().BeTrue();
        amountMinor.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-3")]
    [InlineData("1.005")]
    [InlineData("twelve")]
    public void RejectsAmountsThatAreNotWholePence(string input)
    {
        Money.TryParsePounds(input, out _).Should().BeFalse();
    }
}
