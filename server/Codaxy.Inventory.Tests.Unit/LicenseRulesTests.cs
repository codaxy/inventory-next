using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.Extensions.Time.Testing;

namespace Codaxy.Inventory.Tests.Unit;

public class LicenseRulesTests
{
    [Theory]
    [InlineData(3, "Low")]
    [InlineData(4, "Low")]
    [InlineData(5, "Medium")]
    [InlineData(7, "Medium")]
    [InlineData(8, "High")]
    [InlineData(9, "High")]
    public void Importance_bands_the_summed_weights_as_the_original(int weight, string level) =>
        Assert.Equal(level, AssetWrites.LevelFor(weight));

    [Fact]
    public void Now_is_kept_to_the_microsecond_postgres_stores()
    {
        var clock = new FakeTimeProvider(
            new DateTimeOffset(2026, 9, 26, 10, 0, 0, TimeSpan.Zero).AddTicks(1234567)
        );

        var now = AssetWrites.Now(clock);

        Assert.Equal(0, now.Ticks % 10);
        Assert.Equal(1234560, now.Ticks % TimeSpan.TicksPerSecond);
    }

    private static readonly DateOnly Today = new(2026, 9, 26);

    [Theory]
    [InlineData(null, null)]
    [InlineData(-1, "expired")]
    [InlineData(0, "soon")]
    [InlineData(14, "soon")]
    [InlineData(15, "regular")]
    [InlineData(400, "regular")]
    public void Expiry_is_expired_before_today_soon_within_fifteen_days_regular_after(
        int? days,
        string? status
    ) =>
        Assert.Equal(status, Expiry.Status(days is null ? null : Today.AddDays(days.Value), Today));

    [Fact]
    public void Today_is_the_clocks_utc_date()
    {
        var clock = new FakeTimeProvider(
            new DateTimeOffset(2026, 9, 26, 23, 30, 0, TimeSpan.FromHours(-2))
        );

        Assert.Equal(new DateOnly(2026, 9, 27), Expiry.Today(clock));
    }
}
