using FluentAssertions;
using FluentValidation;
using NetArchTest.Rules;
using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.ArchitectureTests.Application;

public sealed class ApplicationTests
{
    [Theory]
    [InlineData(typeof(ICommand), "Command", false)]
    [InlineData(typeof(ICommand<>), "Command", false)]
    [InlineData(typeof(ICommandHandler<>), "CommandHandler", true)]
    [InlineData(typeof(ICommandHandler<,>), "CommandHandler", true)]
    [InlineData(typeof(IQuery<>), "Query", false)]
    [InlineData(typeof(IQueryHandler<,>), "QueryHandler", true)]
    [InlineData(typeof(IDomainEventHandler<>), "DomainEventHandler", true)]
    public void MessagingTypes_RespectNamingAndAccessibility(Type contract, string suffix, bool internalOnly)
    {
        System.Reflection.Assembly assembly = SilverbridgeWeb.Modules.Payments.Application.AssemblyReference.Assembly;
        Types.InAssembly(assembly).That().ImplementInterface(contract)
            .Should().BeSealed().GetResult().IsSuccessful.Should().BeTrue();
        Types.InAssembly(assembly).That().ImplementInterface(contract)
            .Should().HaveNameEndingWith(suffix).GetResult().IsSuccessful.Should().BeTrue();
        if (internalOnly)
        {
            Types.InAssembly(assembly).That().ImplementInterface(contract)
                .Should().NotBePublic().GetResult().IsSuccessful.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData(typeof(AbstractValidator<>), "Validator")]
    [InlineData(typeof(DomainEventHandler<>), "DomainEventHandler")]
    public void DerivedTypes_AreInternalSealedAndCorrectlyNamed(Type baseType, string suffix)
    {
        System.Reflection.Assembly assembly = SilverbridgeWeb.Modules.Payments.Application.AssemblyReference.Assembly;
        Types.InAssembly(assembly).That().Inherit(baseType)
            .Should().NotBePublic().GetResult().IsSuccessful.Should().BeTrue();
        Types.InAssembly(assembly).That().Inherit(baseType)
            .Should().BeSealed().GetResult().IsSuccessful.Should().BeTrue();
        Types.InAssembly(assembly).That().Inherit(baseType)
            .Should().HaveNameEndingWith(suffix).GetResult().IsSuccessful.Should().BeTrue();
    }
}
