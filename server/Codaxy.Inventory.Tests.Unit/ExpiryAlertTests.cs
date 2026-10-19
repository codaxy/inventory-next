using System.Text.Json;
using Codaxy.Inventory.App.Shared.ExpiryAlerts;

namespace Codaxy.Inventory.Tests.Unit;

public class ExpiryAlertTests
{
    private static readonly DateOnly Today = new(2026, 10, 19);

    [Theory]
    [InlineData(10, "days", "2026-10-29")]
    [InlineData(2, "weeks", "2026-11-02")]
    [InlineData(2, "months", "2026-12-19")]
    public void The_lead_reaches_that_many_days_weeks_or_calendar_months_ahead(
        int amount,
        string unit,
        string until
    ) => Assert.Equal(DateOnly.Parse(until), new AlertLead(amount, unit).Until(Today));

    [Fact]
    public void A_month_from_its_last_day_ends_on_the_shorter_month_s_last() =>
        Assert.Equal(new DateOnly(2027, 2, 28), new AlertLead(1, "months").Until(new(2027, 1, 31)));

    [Theory]
    [InlineData(1, "weeks", "1 week")]
    [InlineData(2, "months", "2 months")]
    public void The_lead_reads_as_a_count(int amount, string unit, string text) =>
        Assert.Equal(text, new AlertLead(amount, unit).ToString());

    [Fact]
    public void Saving_writes_each_recipient_trimmed_lower_case_once() =>
        Assert.Equal(
            ("[\"it@codaxy.com\",\"ops@codaxy.com\"]", null),
            ExpiryAlertSettings.Recipients.Accept(
                Json(new[] { " IT@codaxy.com ", "it@codaxy.com", "", "ops@codaxy.com" })
            )
        );

    [Theory]
    [InlineData("it")]
    [InlineData("it@codaxy")]
    [InlineData("it @codaxy.com")]
    [InlineData("it@@codaxy.com")]
    public void Refuses_what_is_not_an_address(string address) =>
        Assert.Equal(
            $"\"{address}\" is not an email address.",
            ExpiryAlertSettings.Recipients.Accept(Json(new[] { address })).Problem
        );

    [Fact]
    public void Saving_a_lead_writes_its_unit_in_lower_case() =>
        Assert.Equal(
            ("{\"amount\":3,\"unit\":\"weeks\"}", null),
            ExpiryAlertSettings.Lead.Accept(Json(new { amount = 3, unit = " Weeks " }))
        );

    [Theory]
    [InlineData(0, "days", "From 1 to 999.")]
    [InlineData(1000, "days", "From 1 to 999.")]
    [InlineData(2, "years", "Days, weeks or months.")]
    public void Refuses_a_lead_out_of_range_or_of_another_unit(
        int amount,
        string unit,
        string problem
    ) => Assert.Equal(problem, ExpiryAlertSettings.Lead.Accept(Json(new { amount, unit })).Problem);

    [Fact]
    public void The_lead_is_two_months_and_nobody_receives_alerts_by_default()
    {
        Assert.Equal(new AlertLead(2, "months"), ExpiryAlertSettings.Lead.Default);
        Assert.Empty(ExpiryAlertSettings.Recipients.Default);
    }

    [Theory]
    [InlineData("00:00:00", "", "ExpiryAlerts:Interval must be longer than zero.")]
    [InlineData(
        "01:00:00",
        "inventory.codaxy.com",
        "ExpiryAlerts:ApplicationUrl is not an http or https address: inventory.codaxy.com."
    )]
    [InlineData("01:00:00", "https://inventory.codaxy.com", null)]
    [InlineData("01:00:00", "", null)]
    public void Options_say_what_stops_the_start(string interval, string url, string? problem) =>
        Assert.Equal(
            problem,
            new ExpiryAlertOptions
            {
                Interval = TimeSpan.Parse(interval),
                ApplicationUrl = url,
            }.Problem
        );

    private static readonly Guid Rider = Guid.CreateVersion7();
    private static readonly Guid Laptop = Guid.CreateVersion7();

    private static DueAlert Subscription(
        DateOnly date,
        bool renews = false,
        string? disposedAt = null
    ) => new(Rider, DueKind.Subscription, "Rider", 100875, date, renews, disposedAt);

    private static DueAlert Warranty(DateOnly date, string? disposedAt = null) =>
        new(Laptop, DueKind.Warranty, "lap-dub-gc", 100893, date, false, disposedAt);

    private static (string Subject, string Body) Compose(string url, params DueAlert[] due) =>
        ExpiryAlertMessage.Compose(due, Today, new AlertLead(2, "months"), url);

    [Fact]
    public void One_record_is_named_in_the_subject()
    {
        Assert.Equal(
            "Rider #100875: subscription expires on 19 Dec 2026",
            Compose("", Subscription(new(2026, 12, 19))).Subject
        );
        Assert.Equal(
            "lap-dub-gc #100893: warranty ends on 2 Nov 2026",
            Compose("", Warranty(new(2026, 11, 2))).Subject
        );
    }

    [Fact]
    public void Several_are_counted_by_kind_in_the_subject()
    {
        Assert.Equal(
            "2 subscriptions and 1 warranty ending",
            Compose(
                "",
                Subscription(Today),
                Subscription(Today.AddDays(1)),
                Warranty(Today.AddDays(2))
            ).Subject
        );
        Assert.Equal(
            "2 warranties ending",
            Compose("", Warranty(Today), Warranty(Today.AddDays(1))).Subject
        );
    }

    [Fact]
    public void Each_record_says_when_it_ends_counted_from_today()
    {
        var body = Compose(
            "",
            Subscription(Today),
            Warranty(Today.AddDays(1)),
            Subscription(Today.AddDays(61))
        ).Body;

        Assert.StartsWith("Ending within 2 months:\n", body);
        Assert.Contains("Rider #100875\nSubscription expires on 19 Oct 2026, today.\n", body);
        Assert.Contains("lap-dub-gc #100893\nWarranty ends on 20 Oct 2026, tomorrow.\n", body);
        Assert.Contains("Subscription expires on 19 Dec 2026, in 61 days.\n", body);
    }

    [Fact]
    public void Says_a_subscription_renews_automatically_and_where_a_disposed_asset_is()
    {
        var body = Compose(
            "",
            Subscription(Today, renews: true),
            Warranty(Today, disposedAt: "Otpis")
        ).Body;

        Assert.Contains("Renews automatically on that date unless it is cancelled.\n", body);
        Assert.Contains("Disposed of, at Otpis.\n", body);
    }

    [Fact]
    public void Links_each_record_only_where_the_application_s_address_is_known()
    {
        var linked = Compose(
            "https://inventory.codaxy.com/",
            Subscription(Today),
            Warranty(Today)
        ).Body;

        Assert.Contains($"https://inventory.codaxy.com/licenses/{Rider}\n", linked);
        Assert.Contains($"https://inventory.codaxy.com/electronic-devices/{Laptop}\n", linked);
        Assert.DoesNotContain("http", Compose("", Subscription(Today)).Body);
    }

    [Fact]
    public void A_record_without_a_number_is_named_alone() =>
        Assert.Equal(
            "Rider: subscription expires on 19 Oct 2026",
            Compose("", Subscription(Today) with { Number = null }).Subject
        );

    private static JsonElement Json(object value) => JsonSerializer.SerializeToElement(value);
}
