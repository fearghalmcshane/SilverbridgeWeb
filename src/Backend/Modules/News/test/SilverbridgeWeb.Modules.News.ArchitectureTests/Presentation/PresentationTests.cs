using NetArchTest.Rules;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Modules.News.ArchitectureTests.Abstractions;

namespace SilverbridgeWeb.Modules.News.ArchitectureTests.Presentation;

public class PresentationTests : BaseTest
{
    [Fact]
    public void IntegrationEventHandler_Should_BeSealed()
    {
        Types.InAssembly(PresentationAssembly)
            .That()
            .ImplementInterface(typeof(IIntegrationEventHandler<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void IntegrationEventHandler_ShouldHave_NameEndingWith_Handler()
    {
        Types.InAssembly(PresentationAssembly)
            .That()
            .ImplementInterface(typeof(IIntegrationEventHandler<>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
