using Codaxy.Inventory.App.Dashboard;

namespace Codaxy.Inventory.Tests.Unit;

public class DashboardOptionsTests
{
    [Fact]
    public void The_default_windows_are_valid() => Assert.Null(new DashboardOptions().Problem);

    [Fact]
    public void A_window_under_a_day_stops_the_start_naming_it() =>
        Assert.Equal(
            "Dashboard:WarrantiesEndedDays must be at least 1.",
            new DashboardOptions { WarrantiesEndedDays = 0 }.Problem
        );
}
