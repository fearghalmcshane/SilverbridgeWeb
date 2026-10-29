using FluentAssertions;
using NetArchTest.Rules;
using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.ArchitectureTests.Presentation;

public sealed class PresentationTests
{
    [Fact]
    public void IntegrationEventHandlers_AreSealedAndCorrectlyNamed()
    {
        System.Reflection.Assembly assembly = SilverbridgeWeb.Modules.Payments.Presentation.AssemblyReference.Assembly;
        Types.InAssembly(assembly).That().ImplementInterface(typeof(IIntegrationEventHandler<>))
            .Should().BeSealed().GetResult().IsSuccessful.Should().BeTrue();
        Types.InAssembly(assembly).That().ImplementInterface(typeof(IIntegrationEventHandler<>))
            .Should().HaveNameEndingWith("Handler").GetResult().IsSuccessful.Should().BeTrue();
    }
}
