using System.Text.Json;
using Codaxy.Inventory.App.Shared.ExpiryAlerts;
using Codaxy.Inventory.App.Shared.Links;

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
    [InlineData("00:00:00", "ExpiryAlerts:Interval must be longer than zero.")]
    [InlineData("01:00:00", null)]
    public void Options_say_what_stops_the_start(string interval, string? problem) =>
        Assert.Equal(
            problem,
            new ExpiryAlertOptions { Interval = TimeSpan.Parse(interval) }.Problem
        );

    [Theory]
    [InlineData(
        "inventory.codaxy.com",
        "ApplicationUrl is not an http or https address: inventory.codaxy.com."
    )]
    [InlineData(
        "ftp://inventory.codaxy.com",
        "ApplicationUrl is not an http or https address: ftp://inventory.codaxy.com."
    )]
    [InlineData("https://inventory.codaxy.com", null)]
    [InlineData("", null)]
    public void The_application_s_address_is_an_http_address_or_none(string url, string? problem) =>
        Assert.Equal(problem, new ApplicationOptions { ApplicationUrl = url }.Problem);

    private static readonly Guid Rider = Guid.CreateVersion7();
    private static readonly Guid Laptop = Guid.CreateVersion7();
    private static readonly Disposal Otpis = new(Guid.CreateVersion7(), "Otpis");

    private static DueAlert Subscription(DateOnly date, bool renews = false) =>
        new(Rider, DueKind.Subscription, "Rider", 100875, date, renews, null);

    private static DueAlert Warranty(DateOnly date, Disposal? disposedAt = null) =>
        new(Laptop, DueKind.Warranty, "lap-dub-gc", 100893, date, false, disposedAt);

    private static AlertEmail Compose(string url, params DueAlert[] due) =>
        ExpiryAlertMessage.Compose(due, Today, new AlertLead(2, "months"), url);

    [Fact]
    public void The_subject_counts_what_ends_by_kind()
    {
        Assert.Equal(
            "Expiry alert: 1 warranty ending",
            Compose("", Warranty(new(2026, 11, 2))).Subject
        );
        Assert.Equal(
            "Expiry alert: 2 subscriptions and 1 warranty ending",
            Compose(
                "",
                Subscription(Today),
                Subscription(Today.AddDays(1)),
                Warranty(Today.AddDays(2))
            ).Subject
        );
        Assert.Equal(
            "Expiry alert: 2 warranties ending",
            Compose("", Warranty(Today), Warranty(Today.AddDays(1))).Subject
        );
    }

    [Fact]
    public void Each_record_says_when_it_ends_counted_from_today()
    {
        var email = Compose(
            "",
            Subscription(Today),
            Warranty(Today.AddDays(1)),
            Subscription(Today.AddDays(61))
        );

        Assert.StartsWith(
            "2 subscriptions and 1 warranty ending\n\nThese end within the next 2 months.\n",
            email.Text
        );
        Assert.Contains(
            "Rider #100875\nSubscription expires on 19 Oct 2026 (today).\n",
            email.Text
        );
        Assert.Contains(
            "lap-dub-gc #100893\nWarranty ends on 20 Oct 2026 (tomorrow).\n",
            email.Text
        );
        Assert.Contains("Subscription expires on 19 Dec 2026 (in 61 days).\n", email.Text);
        Assert.Contains(">2 subscriptions and 1 warranty ending</h1>", email.Html);
        Assert.Contains(">These end within the next 2 months.</p>", email.Html);
        Assert.Contains("Subscription expires on 19 Dec 2026 (in 61 days).", email.Html);
    }

    [Fact]
    public void Says_a_subscription_renews_automatically_and_where_a_disposed_asset_is()
    {
        var email = Compose("", Subscription(Today, renews: true), Warranty(Today, Otpis));

        Assert.Contains("Renews automatically on that date unless it is cancelled.\n", email.Text);
        Assert.Contains("Disposed of, at Otpis.\n", email.Text);
        Assert.Contains("Renews automatically on that date unless it is cancelled.", email.Html);
        Assert.Contains("Disposed of, at Otpis.", email.Html);
    }

    [Fact]
    public void Links_each_record_its_disposal_location_and_the_application()
    {
        var email = Compose(
            "https://inventory.codaxy.com/",
            Subscription(Today),
            Warranty(Today, Otpis)
        );

        Assert.Contains($"https://inventory.codaxy.com/licenses/{Rider}\n", email.Text);
        Assert.Contains($"https://inventory.codaxy.com/electronic-devices/{Laptop}\n", email.Text);
        Assert.Contains($"href=\"https://inventory.codaxy.com/licenses/{Rider}\"", email.Html);
        Assert.Contains(
            $"href=\"https://inventory.codaxy.com/electronic-devices/{Laptop}\"",
            email.Html
        );
        Assert.Contains(
            $"href=\"https://inventory.codaxy.com/company/locations/{Otpis.LocationId}\"",
            email.Html
        );
        Assert.Contains("href=\"https://inventory.codaxy.com/\"", email.Html);
    }

    [Fact]
    public void Links_nothing_where_the_application_s_address_is_not_known()
    {
        var email = Compose("", Subscription(Today), Warranty(Today, Otpis));

        Assert.DoesNotContain("http", email.Text);
        Assert.DoesNotContain("href", email.Html);
    }

    [Fact]
    public void Sets_the_page_and_its_text_together_for_a_light_and_a_dark_theme()
    {
        var html = Compose("", Subscription(Today)).Html;

        Assert.Contains("<meta name=\"color-scheme\" content=\"light dark\">", html);
        Assert.Contains(".card { background: #ffffff; color: #101a2b; }", html);
        Assert.Contains(
            ".card { background: #1b1f27 !important; color: #e7eaf0 !important; }",
            html
        );
        // Inline, a colour would hold in a dark theme too, where the stylesheet cannot undo it.
        Assert.DoesNotMatch("<body[^>]*(color|background)", html);
    }

    [Fact]
    public void One_record_is_it()
    {
        var email = Compose("", Warranty(Today));

        Assert.StartsWith("1 warranty ending\n\nIt ends within the next 2 months.\n", email.Text);
    }

    [Fact]
    public void Ends_with_who_sent_it()
    {
        var email = Compose("", Subscription(Today));

        Assert.EndsWith("\nSent by Inventory\n", email.Text);
        Assert.Contains(">Sent by Inventory</td>", email.Html);
    }

    [Fact]
    public void Names_are_escaped_in_the_html() =>
        Assert.Contains(
            "R&amp;D &lt;tools&gt;",
            Compose("", Subscription(Today) with { Name = "R&D <tools>" }).Html
        );

    [Fact]
    public void A_record_without_a_number_is_named_alone() =>
        Assert.Contains(
            "\nRider\nSubscription expires",
            Compose("", Subscription(Today) with { Number = null }).Text
        );

    [Fact]
    public void No_reminders_by_default_and_at_most_two() =>
        Assert.Equal(
            (null, "At most two reminders."),
            (
                ExpiryAlertSettings.Reminders.Accept(Json(Array.Empty<object>())).Problem,
                ExpiryAlertSettings
                    .Reminders.Accept(
                        Json(
                            new[]
                            {
                                new { amount = 3, unit = "weeks" },
                                new { amount = 2, unit = "weeks" },
                                new { amount = 1, unit = "weeks" },
                            }
                        )
                    )
                    .Problem
            )
        );

    [Fact]
    public void A_reminder_is_held_to_a_lead_s_rules() =>
        Assert.Equal(
            "Days, weeks or months.",
            ExpiryAlertSettings
                .Reminders.Accept(Json(new[] { new { amount = 2, unit = "fortnights" } }))
                .Problem
        );

    [Fact]
    public void Reminders_out_of_order_are_left_out_from_the_first_that_breaks_it() =>
        Assert.Equal(
            [new AlertLead(2, "weeks")],
            ExpiryAlertSettings.InOrder(
                new AlertLead(2, "months"),
                [new(2, "weeks"), new(3, "weeks")]
            )
        );

    [Fact]
    public void A_reminder_s_line_says_so() =>
        Assert.Contains(
            "Rider #100875\nReminder: subscription expires on 19 Oct 2026 (today).\n",
            Compose("", Subscription(Today) with { Stage = 1 }).Text
        );

    private static JsonElement Json(object value) => JsonSerializer.SerializeToElement(value);
}
