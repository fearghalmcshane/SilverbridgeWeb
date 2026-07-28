using FluentAssertions;

namespace SilverbridgeWeb.Modules.News.ArchitectureTests.Abstractions;

internal static class TestResultExtensions
{
    internal static void ShouldBeSuccessful(this NetArchTest.Rules.TestResult testResult)
    {
        testResult.FailingTypes?.Should().BeEmpty();
    }
}
