using System.Reflection;
using NetArchTest.Rules;
using SilverbridgeWeb.ArchitectureTests.Abstractions;
using SilverbridgeWeb.Modules.Attendance.Domain.Attendees;
using SilverbridgeWeb.Modules.Attendance.Infrastructure;
using SilverbridgeWeb.Modules.Events.Domain.Events;
using SilverbridgeWeb.Modules.Events.Infrastructure;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Infrastructure;
using SilverbridgeWeb.Modules.Ticketing.Domain.Orders;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure;
using SilverbridgeWeb.Modules.Users.Domain.Users;
using SilverbridgeWeb.Modules.Users.Infrastructure;

namespace SilverbridgeWeb.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
    [Fact]
    public void UsersModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [EventsNamespace, TicketingNamespace, AttendanceNamespace, NewsNamespace, MatchesNamespace];
        string[] integrationEventsModules =
        [
            EventsIntegrationEventsNamespace,
            TicketingIntegrationEventsNamespace,
            AttendanceIntegrationEventsNamespace,
            NewsIntegrationEventsNamespace,
            MatchesIntegrationEventsNamespace
        ];

        List<Assembly> usersAssemblies =
        [
            typeof(User).Assembly,
            Modules.Users.Application.AssemblyReference.Assembly,
            Modules.Users.Presentation.AssemblyReference.Assembly,
            typeof(UsersModule).Assembly
        ];

        Types.InAssemblies(usersAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void EventsModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [UsersNamespace, TicketingNamespace, AttendanceNamespace, NewsNamespace, MatchesNamespace];
        string[] integrationEventsModules =
        [
            UsersIntegrationEventsNamespace,
            TicketingIntegrationEventsNamespace,
            AttendanceIntegrationEventsNamespace,
            NewsIntegrationEventsNamespace,
            MatchesIntegrationEventsNamespace
        ];

        List<Assembly> eventsAssemblies =
        [
            typeof(Event).Assembly,
            Modules.Events.Application.AssemblyReference.Assembly,
            Modules.Events.Presentation.AssemblyReference.Assembly,
            typeof(EventsModule).Assembly
        ];

        Types.InAssemblies(eventsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void TicketingModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [EventsNamespace, UsersNamespace, AttendanceNamespace, NewsNamespace, MatchesNamespace];
        string[] integrationEventsModules =
        [
            EventsIntegrationEventsNamespace,
            UsersIntegrationEventsNamespace,
            AttendanceIntegrationEventsNamespace,
            NewsIntegrationEventsNamespace,
            MatchesIntegrationEventsNamespace
        ];

        List<Assembly> ticketingAssemblies =
        [
            typeof(Order).Assembly,
            Modules.Ticketing.Application.AssemblyReference.Assembly,
            Modules.Ticketing.Presentation.AssemblyReference.Assembly,
            typeof(TicketingModule).Assembly
        ];

        Types.InAssemblies(ticketingAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void AttendanceModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [UsersNamespace, TicketingNamespace, EventsNamespace, NewsNamespace, MatchesNamespace];
        string[] integrationEventsModules =
        [
            UsersIntegrationEventsNamespace,
            TicketingIntegrationEventsNamespace,
            EventsIntegrationEventsNamespace,
            NewsIntegrationEventsNamespace,
            MatchesIntegrationEventsNamespace
        ];

        List<Assembly> attendanceAssemblies =
        [
            typeof(Attendee).Assembly,
            Modules.Attendance.Application.AssemblyReference.Assembly,
            Modules.Attendance.Presentation.AssemblyReference.Assembly,
            typeof(AttendanceModule).Assembly
        ];

        Types.InAssemblies(attendanceAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void NewsModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [UsersNamespace, EventsNamespace, TicketingNamespace, AttendanceNamespace, MatchesNamespace];
        string[] integrationEventsModules =
        [
            UsersIntegrationEventsNamespace,
            EventsIntegrationEventsNamespace,
            TicketingIntegrationEventsNamespace,
            AttendanceIntegrationEventsNamespace,
            MatchesIntegrationEventsNamespace
        ];

        List<Assembly> newsAssemblies =
        [
            typeof(Article).Assembly,
            Modules.News.Application.AssemblyReference.Assembly,
            Modules.News.Presentation.AssemblyReference.Assembly,
            typeof(NewsModule).Assembly
        ];

        Types.InAssemblies(newsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void MatchesModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules =
        [
            UsersNamespace, EventsNamespace, TicketingNamespace, AttendanceNamespace, NewsNamespace,
            "SilverbridgeWeb.Modules.Bookings", "SilverbridgeWeb.Modules.Foireann"
        ];
        string[] integrationEventsModules =
        [
            UsersIntegrationEventsNamespace, EventsIntegrationEventsNamespace, TicketingIntegrationEventsNamespace,
            AttendanceIntegrationEventsNamespace, NewsIntegrationEventsNamespace,
            "SilverbridgeWeb.Modules.Bookings.IntegrationEvents"
        ];
        List<Assembly> matchesAssemblies =
        [
            typeof(Modules.Matches.Domain.Matches.MatchCompletion).Assembly,
            Modules.Matches.Application.AssemblyReference.Assembly,
            Modules.Matches.Presentation.AssemblyReference.Assembly,
            typeof(Modules.Matches.Infrastructure.MatchesModule).Assembly
        ];

        Types.InAssemblies(matchesAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void BookingsAndFoireannModules_ShouldNotDependOn_MatchesImplementation()
    {
        List<Assembly> moduleAssemblies =
        [
            typeof(Modules.Bookings.Domain.Bookings.Booking).Assembly,
            Modules.Bookings.Application.AssemblyReference.Assembly,
            Modules.Bookings.Presentation.AssemblyReference.Assembly,
            typeof(Modules.Bookings.Infrastructure.BookingsModule).Assembly,
            Modules.Foireann.Application.AssemblyReference.Assembly,
            Modules.Foireann.Presentation.AssemblyReference.Assembly,
            typeof(Modules.Foireann.Infrastructure.FoireannModule).Assembly
        ];

        Types.InAssemblies(moduleAssemblies)
            .Should()
            .NotHaveDependencyOnAny(
                "SilverbridgeWeb.Modules.Matches.Domain",
                "SilverbridgeWeb.Modules.Matches.Application",
                "SilverbridgeWeb.Modules.Matches.Infrastructure",
                "SilverbridgeWeb.Modules.Matches.Presentation")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
