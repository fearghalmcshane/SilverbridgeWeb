using NetArchTest.Rules;
using FluentAssertions;
using SilverbridgeWeb.Modules.Matches.Infrastructure;
using System.Reflection;

namespace SilverbridgeWeb.Modules.Matches.ArchitectureTests;

public sealed class MatchesLayerTests
{
    [Fact]
    public void LayersRespectDependencyDirection()
    {
        Assembly domain = typeof(SilverbridgeWeb.Modules.Matches.Domain.AssemblyReference).Assembly;
        Assembly application = typeof(SilverbridgeWeb.Modules.Matches.Application.AssemblyReference).Assembly;
        Assembly infrastructure = typeof(MatchesModule).Assembly;
        Assembly presentation = typeof(SilverbridgeWeb.Modules.Matches.Presentation.AssemblyReference).Assembly;

        Types.InAssembly(domain).Should().NotHaveDependencyOn(application.GetName().Name).GetResult().IsSuccessful
            .Should().BeTrue();
        Types.InAssembly(domain).Should().NotHaveDependencyOn(infrastructure.GetName().Name).GetResult().IsSuccessful
            .Should().BeTrue();
        Types.InAssembly(application).Should().NotHaveDependencyOn(infrastructure.GetName().Name).GetResult().IsSuccessful
            .Should().BeTrue();
        Types.InAssembly(application).Should().NotHaveDependencyOn(presentation.GetName().Name).GetResult().IsSuccessful
            .Should().BeTrue();
        Types.InAssembly(presentation).Should().NotHaveDependencyOn(infrastructure.GetName().Name).GetResult().IsSuccessful
            .Should().BeTrue();
    }
}
