using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Matches.ArchitectureTests.Domain;

public sealed class DomainTests
{
    [Fact]
    public void DomainEvents_AreSealedAndCorrectlyNamed()
    {
        Assembly assembly = SilverbridgeWeb.Modules.Matches.Domain.AssemblyReference.Assembly;
        Types.InAssembly(assembly).That().ImplementInterface(typeof(IDomainEvent))
            .Or().Inherit(typeof(DomainEvent))
            .Should().BeSealed().GetResult().IsSuccessful.Should().BeTrue();
        Types.InAssembly(assembly).That().ImplementInterface(typeof(IDomainEvent))
            .Or().Inherit(typeof(DomainEvent))
            .Should().HaveNameEndingWith("DomainEvent").GetResult().IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Entities_HavePrivateParameterlessConstructorAndNoPublicConstructors()
    {
        IEnumerable<Type> entities = Types.InAssembly(SilverbridgeWeb.Modules.Matches.Domain.AssemblyReference.Assembly)
            .That().Inherit(typeof(Entity)).GetTypes();
        foreach (Type entity in entities)
        {
            entity.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Should().Contain(constructor => constructor.IsPrivate && constructor.GetParameters().Length == 0);
            entity.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Should().BeEmpty();
        }
    }
}
