using System.Net;
using System.Text;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SilverbridgeWeb.WebUI.Components.ScoreConnect;
using SilverbridgeWeb.WebUI.Pages;
using SilverbridgeWeb.WebUI.Services.ScoreConnect;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class LiveScoreTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 10, 13, 19, 30, 0, TimeSpan.Zero);

    private readonly BunitContext _context = new();
    private readonly FakeStatusProvider _provider = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly List<HttpClient> _httpClients = [];

    public LiveScoreTests()
    {
        _context.Services.AddMudServices();
        _context.Services.AddSingleton<ILiveScoreStatusProvider>(_provider);
        _context.Services.AddSingleton(TimeProvider.System);
        _context.Services.AddSingleton(Options.Create(new ScoreConnectOptions()));
        _context.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData("""[{"status":"live","online":true}]""", "Live", true)]
    [InlineData("""[{"status":"live","online":false}]""", "Offline", false)]
    [InlineData("""[{"status":"finished","online":true}]""", "FullTime", true)]
    [InlineData("""[{"status":"finished","online":false}]""", "Offline", false)]
    [InlineData("""[{"status":"idle","online":true}]""", "NotStarted", true)]
    [InlineData("""[]""", "Unavailable", false)]
    public async Task MapsBoardRowToState(string json, string expectedState, bool expectedOnline)
    {
        using var handler = new StubHandler(HttpStatusCode.OK, json);

        LiveScoreSnapshot snapshot = await CreateService(handler).GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);

        snapshot.State.ToString().Should().Be(expectedState);
        snapshot.BoardOnline.Should().Be(expectedOnline);
    }

    [Fact]
    public async Task ReadsTeamsScoresAndClockWithAuthenticatedRequest()
    {
        const string json = """
            [{"status":"live","online":true,"home_name":"Silverbridge","away_name":"Cullyhanna",
              "home_goals":1,"home_points":"5","away_goals":0,"away_points":7,
              "clock_seconds":1425,"clock_running":true,"period":"H2"}]
            """;
        using var handler = new StubHandler(HttpStatusCode.OK, json);

        LiveScoreSnapshot snapshot = await CreateService(handler).GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);

        snapshot.Home.Should().Be(new LiveScoreTeam("Silverbridge", 1, 5));
        snapshot.Away.Should().Be(new LiveScoreTeam("Cullyhanna", 0, 7));
        snapshot.Home.Score.Should().Be("1-05");
        snapshot.Home.Total.Should().Be(8);
        snapshot.Period.Should().Be("H2");
        snapshot.ClockRunning.Should().BeTrue();
        snapshot.GetElapsedMinutes(Now.AddSeconds(60)).Should().Be(24);
        handler.RequestUri.Should().Be(new Uri("https://fsl.test/rest/v1/live_scores?mac=eq.SILVERBRIDGESB&select=*"));
        handler.ApiKey.Should().Be(new ScoreConnectOptions().PublishableKey);
    }

    [Fact]
    public async Task ReturnsUnavailableWhenProviderFailsAndCachesResult()
    {
        using var handler = new StubHandler(HttpStatusCode.InternalServerError, "oops");
        LiveScoreStatusService service = CreateService(handler);

        LiveScoreSnapshot first = await service.GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);
        LiveScoreSnapshot second = await service.GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);

        first.State.Should().Be(LiveScoreState.Unavailable);
        second.Should().BeSameAs(first);
        handler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task CoalescesConcurrentCacheMissesAcrossProviderInstances()
    {
        var responseReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new StubHandler(HttpStatusCode.OK, """[{"status":"live","online":true}]""")
        {
            BeforeResponse = cancellationToken => responseReady.Task.WaitAsync(cancellationToken)
        };
        LiveScoreStatusService firstService = CreateService(handler);
        LiveScoreStatusService secondService = CreateService(handler);

        Task<LiveScoreSnapshot> first = firstService.GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);
        Task<LiveScoreSnapshot> second = secondService.GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);
        int requestsWhilePending = handler.RequestCount;
        responseReady.SetResult();
        LiveScoreSnapshot[] snapshots = await Task.WhenAll(first, second);

        requestsWhilePending.Should().Be(1);
        handler.RequestCount.Should().Be(1);
        snapshots[0].State.Should().Be(LiveScoreState.Live);
        snapshots[1].Should().BeSameAs(snapshots[0]);

        _cache.Compact(1);
        responseReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        first = firstService.GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);
        second = secondService.GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);
        requestsWhilePending = handler.RequestCount;
        responseReady.SetResult();
        snapshots = await Task.WhenAll(first, second);

        requestsWhilePending.Should().Be(2);
        handler.RequestCount.Should().Be(2);
        snapshots[1].Should().BeSameAs(snapshots[0]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReturnsUnavailableWhenResiliencePolicyRejectsRequest(bool timeout)
    {
        Exception exception = timeout ? new TimeoutRejectedException() : new BrokenCircuitException();
        using var handler = new StubHandler(HttpStatusCode.OK, "[]")
        {
            BeforeResponse = _ => Task.FromException(exception)
        };
        LiveScoreStatusService service = CreateService(handler);

        LiveScoreSnapshot first = await service.GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);
        LiveScoreSnapshot second = await service.GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);

        first.State.Should().Be(LiveScoreState.Unavailable);
        second.Should().BeSameAs(first);
        handler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task PropagatesFetchCancellationWithoutCachingAndReleasesLock()
    {
        var responseReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Xunit.TestContext.Current.CancellationToken);
        using var handler = new StubHandler(HttpStatusCode.OK, """[{"status":"live","online":true}]""")
        {
            BeforeResponse = cancellationToken => responseReady.Task.WaitAsync(cancellationToken)
        };
        LiveScoreStatusService service = CreateService(handler);
        Task<LiveScoreSnapshot> pending = service.GetSnapshotAsync(cancellation.Token);

        await cancellation.CancelAsync();

        Func<Task> fetch = () => pending;
        await fetch.Should().ThrowAsync<OperationCanceledException>();
        responseReady.SetResult();
        LiveScoreSnapshot snapshot = await CreateService(handler).GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);
        snapshot.State.Should().Be(LiveScoreState.Live);
        handler.RequestCount.Should().Be(2);
    }

    [Fact]
    public async Task CancellingWaitingCallerDoesNotCancelSharedFetch()
    {
        var responseReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new StubHandler(HttpStatusCode.OK, """[{"status":"live","online":true}]""")
        {
            BeforeResponse = cancellationToken => responseReady.Task.WaitAsync(cancellationToken)
        };
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Xunit.TestContext.Current.CancellationToken);
        Task<LiveScoreSnapshot> first = CreateService(handler).GetSnapshotAsync(Xunit.TestContext.Current.CancellationToken);
        Task<LiveScoreSnapshot> second = CreateService(handler).GetSnapshotAsync(cancellation.Token);

        await cancellation.CancelAsync();
        responseReady.SetResult();

        Func<Task> fetch = () => second;
        await fetch.Should().ThrowAsync<OperationCanceledException>();
        LiveScoreSnapshot snapshot = await first;
        snapshot.State.Should().Be(LiveScoreState.Live);
        handler.RequestCount.Should().Be(1);
    }

    [Theory]
    [InlineData("Offline")]
    [InlineData("Unavailable")]
    public void PageShowsEmptyStateWhenNoGameIsOn(string state)
    {
        _provider.Snapshot = CreateSnapshot(Enum.Parse<LiveScoreState>(state));

        IRenderedComponent<LiveScore> cut = _context.Render<LiveScore>();

        cut.Find(".state-panel h2").TextContent.Should().Contain("No game on right now");
        cut.Find(".state-panel a[href='/fixtures']").Should().NotBeNull();
        cut.FindAll(".live-scoreboard").Should().BeEmpty();
        cut.Markup.Should().NotContainEquivalentOf("trial");
        cut.Markup.Should().NotContain("Scoreboard status");
        cut.FindAll("h1").Should().ContainSingle();
    }

    [Theory]
    [InlineData("NotStarted")]
    [InlineData("Live")]
    [InlineData("FullTime")]
    public void PageShowsScoreboardWhenBoardHasAGame(string state)
    {
        _provider.Snapshot = CreateSnapshot(Enum.Parse<LiveScoreState>(state));

        IRenderedComponent<LiveScore> cut = _context.Render<LiveScore>();

        cut.FindAll(".live-scoreboard").Should().ContainSingle();
        cut.FindAll(".club-scoreboard").Should().ContainSingle();
        cut.FindAll(".state-panel").Should().BeEmpty();
    }

    [Fact]
    public void BannerAndNavBadgeShowWhileLive()
    {
        _provider.Snapshot = CreateSnapshot(LiveScoreState.Live);

        IRenderedComponent<LiveNowBanner> banner = _context.Render<LiveNowBanner>();
        IRenderedComponent<LiveScoreNavLink> navLink = _context.Render<LiveScoreNavLink>();

        AngleSharp.Dom.IElement link = banner.Find("a.live-now-banner__link");
        link.GetAttribute("href").Should().Be("/live-score");
        link.TextContent.Should().Contain("Silverbridge").And.Contain("1-05").And.Contain("0-07")
            .And.Contain("Cullyhanna").And.Contain("23'");
        navLink.FindAll(".live-dot").Should().ContainSingle();
        navLink.Markup.Should().Contain("game in progress");
    }

    [Theory]
    [InlineData("FullTime")]
    [InlineData("NotStarted")]
    [InlineData("Offline")]
    public void BannerAndNavBadgeHiddenWhenNotLive(string state)
    {
        _provider.Snapshot = CreateSnapshot(Enum.Parse<LiveScoreState>(state));

        IRenderedComponent<LiveNowBanner> banner = _context.Render<LiveNowBanner>();
        IRenderedComponent<LiveScoreNavLink> navLink = _context.Render<LiveScoreNavLink>();

        banner.FindAll(".live-now-banner").Should().BeEmpty();
        navLink.FindAll(".live-dot").Should().BeEmpty();
        navLink.Find("a").GetAttribute("href").Should().Be("/live-score");
    }

    [Fact]
    public void ClubScoreboardShowsScoresTotalsAndLeader()
    {
        _provider.Snapshot = CreateSnapshot(LiveScoreState.Live) with { Period = "H2" };

        IRenderedComponent<ClubScoreboard> cut = _context.Render<ClubScoreboard>();

        cut.Find(".club-scoreboard").GetAttribute("data-state").Should().Be("live");
        cut.Find(".club-scoreboard__status").TextContent.Should().Contain("Live");
        cut.FindAll(".club-scoreboard__period").Should().BeEmpty();
        cut.Find(".club-scoreboard__clock").TextContent.Should().Be("23:45");
        IReadOnlyList<AngleSharp.Dom.IElement> teams = cut.FindAll(".club-scoreboard__team");
        teams[0].TextContent.Should().Contain("Silverbridge").And.Contain("1-05").And.Contain("(8)");
        teams[0].GetAttribute("data-leading").Should().Be("true");
        teams[1].TextContent.Should().Contain("Cullyhanna").And.Contain("0-07").And.Contain("(7)");
        teams[1].GetAttribute("data-leading").Should().Be("false");
        cut.Find(".club-scoreboard__teams").GetAttribute("aria-live").Should().Be("polite");
    }

    [Fact]
    public void ClubScoreboardShowsPeriodWhenEnabled()
    {
        _context.Services.AddSingleton(Options.Create(new ScoreConnectOptions { ShowPeriod = true }));
        _provider.Snapshot = CreateSnapshot(LiveScoreState.Live) with { Period = "H2" };

        IRenderedComponent<ClubScoreboard> cut = _context.Render<ClubScoreboard>();

        cut.Find(".club-scoreboard__period").TextContent.Should().Be("2nd half");
    }

    [Theory]
    [InlineData("FullTime", true, "finished", "Full time", "false")]
    [InlineData("NotStarted", true, "idle", "Not started", "false")]
    [InlineData("Live", false, "reconnecting", "Reconnecting", "true")]
    public void ClubScoreboardReflectsBoardState(string state, bool online, string expectedKey, string expectedText, string expectedStale)
    {
        _provider.Snapshot = CreateSnapshot(Enum.Parse<LiveScoreState>(state)) with { BoardOnline = online };

        IRenderedComponent<ClubScoreboard> cut = _context.Render<ClubScoreboard>();

        AngleSharp.Dom.IElement board = cut.Find(".club-scoreboard");
        board.GetAttribute("data-state").Should().Be(expectedKey);
        board.GetAttribute("data-stale").Should().Be(expectedStale);
        cut.Find(".club-scoreboard__status").TextContent.Should().Contain(expectedText);
    }

    public void Dispose()
    {
        _context.Dispose();
        _cache.Dispose();
        foreach (HttpClient httpClient in _httpClients)
        {
            httpClient.Dispose();
        }
    }

    private static LiveScoreSnapshot CreateSnapshot(LiveScoreState state) => new(
        state,
        state != LiveScoreState.Offline,
        new LiveScoreTeam("Silverbridge", 1, 5),
        new LiveScoreTeam("Cullyhanna", 0, 7),
        1425,
        false,
        Now);

    private LiveScoreStatusService CreateService(StubHandler handler)
    {
        var options = new ScoreConnectOptions { ApiBaseUrl = new("https://fsl.test/") };
        var httpClient = new HttpClient(handler, disposeHandler: false) { BaseAddress = options.ApiBaseUrl };
        _httpClients.Add(httpClient);
        return new LiveScoreStatusService(
            httpClient,
            _cache,
            Options.Create(options),
            new FixedTimeProvider(Now),
            NullLogger<LiveScoreStatusService>.Instance);
    }

    private sealed class FakeStatusProvider : ILiveScoreStatusProvider
    {
        public LiveScoreSnapshot Snapshot { get; set; } = LiveScoreSnapshot.Unavailable(Now);

        public TimeSpan RefreshInterval => TimeSpan.FromHours(1);

        public TimeSpan ScoreboardRefreshInterval => TimeSpan.FromHours(1);

        public Task<LiveScoreSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Snapshot);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class StubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public Func<CancellationToken, Task>? BeforeResponse { get; init; }

        public Uri? RequestUri { get; private set; }

        public string? ApiKey { get; private set; }

        public int RequestCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            RequestUri = request.RequestUri;
            ApiKey = request.Headers.TryGetValues("apikey", out IEnumerable<string>? values) ? values.Single() : null;
            if (BeforeResponse is not null)
            {
                await BeforeResponse(cancellationToken);
            }

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }
    }
}
