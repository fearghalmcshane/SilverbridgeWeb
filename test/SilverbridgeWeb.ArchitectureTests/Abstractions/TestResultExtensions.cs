using FluentAssertions;

namespace SilverbridgeWeb.ArchitectureTests.Abstractions;

internal static class TestResultExtensions
{
    internal static void ShouldBeSuccessful(this NetArchTest.Rules.TestResult testResult)
    {
        testResult.FailingTypes?.Should().BeEmpty();
    }
}
