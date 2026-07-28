using System.Reflection;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Infrastructure;

namespace SilverbridgeWeb.Modules.News.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected static readonly Assembly ApplicationAssembly = typeof(News.Application.AssemblyReference).Assembly;

    protected static readonly Assembly DomainAssembly = typeof(Article).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(NewsModule).Assembly;

    protected static readonly Assembly PresentationAssembly = typeof(News.Presentation.AssemblyReference).Assembly;
}
