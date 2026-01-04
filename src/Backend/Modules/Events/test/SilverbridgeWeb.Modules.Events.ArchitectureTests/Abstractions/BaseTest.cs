using System.Reflection;
using SilverbridgeWeb.Modules.Events.Domain.Events;
using SilverbridgeWeb.Modules.Events.Infrastructure;

namespace SilverbridgeWeb.Modules.Events.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // Consider making public types internal
public abstract class BaseTest
#pragma warning restore CA1515 // Consider making public types internal
{
    protected static readonly Assembly ApplicationAssembly = typeof(Events.Application.AssemblyReference).Assembly;

    protected static readonly Assembly DomainAssembly = typeof(Event).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(EventsModule).Assembly;

    protected static readonly Assembly PresentationAssembly = typeof(Events.Presentation.AssemblyReference).Assembly;
}
