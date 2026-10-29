using NetArchTest.Rules;
using FluentAssertions;
using SilverbridgeWeb.Modules.Payments.Infrastructure;
using System.Reflection;

namespace SilverbridgeWeb.Modules.Payments.ArchitectureTests;

public sealed class PaymentsLayerTests
{
    [Fact]
    public void LayersRespectDependencyDirection()
    {
        Assembly domain = SilverbridgeWeb.Modules.Payments.Domain.AssemblyReference.Assembly;
        Assembly application = SilverbridgeWeb.Modules.Payments.Application.AssemblyReference.Assembly;
        Assembly infrastructure = typeof(PaymentsModule).Assembly;
        Assembly presentation = SilverbridgeWeb.Modules.Payments.Presentation.AssemblyReference.Assembly;

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
