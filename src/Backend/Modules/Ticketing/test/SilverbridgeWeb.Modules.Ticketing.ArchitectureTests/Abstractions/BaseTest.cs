using System.Reflection;
using SilverbridgeWeb.Modules.Ticketing.Domain.Orders;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure;

namespace SilverbridgeWeb.Modules.Ticketing.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // Consider making public types internal
public abstract class BaseTest
#pragma warning restore CA1515 // Consider making public types internal
{
    protected static readonly Assembly ApplicationAssembly = typeof(Ticketing.Application.AssemblyReference).Assembly;

    protected static readonly Assembly DomainAssembly = typeof(Order).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(TicketingModule).Assembly;

    protected static readonly Assembly PresentationAssembly = typeof(Ticketing.Presentation.AssemblyReference).Assembly;
}
