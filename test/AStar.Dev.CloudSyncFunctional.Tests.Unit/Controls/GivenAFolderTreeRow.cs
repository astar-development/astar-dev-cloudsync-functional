using System;
using AStar.Dev.CloudSyncFunctional.Controls;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Controls;

public class GivenAFolderTreeRow
{
    [Fact]
    public void when_zero_bytes_then_format_size_returns_dash() =>
        FolderTreeRow.FormatSize(0).ShouldBe("—");

    [Fact]
    public void when_bytes_below_one_megabyte_then_format_size_returns_dash() =>
        FolderTreeRow.FormatSize(500_000).ShouldBe("—");

    [Fact]
    public void when_exactly_one_megabyte_then_format_size_returns_one_mb() =>
        FolderTreeRow.FormatSize(1_048_576L).ShouldBe("1 MB");

    [Fact]
    public void when_five_megabytes_then_format_size_returns_five_mb() =>
        FolderTreeRow.FormatSize(5_242_880L).ShouldBe("5 MB");

    [Fact]
    public void when_exactly_one_gigabyte_then_format_size_returns_one_point_zero_gb() =>
        FolderTreeRow.FormatSize(1_073_741_824L).ShouldBe("1.0 GB");

    [Fact]
    public void when_four_gigabytes_then_format_size_returns_four_point_zero_gb() =>
        FolderTreeRow.FormatSize(4_294_967_296L).ShouldBe("4.0 GB");

    [Fact]
    public void when_last_sync_is_min_value_then_format_last_sync_returns_dash() =>
        FolderTreeRow.FormatLastSync(DateTimeOffset.MinValue).ShouldBe("—");

    [Fact]
    public void when_last_sync_was_thirty_seconds_ago_then_format_last_sync_returns_now() =>
        FolderTreeRow.FormatLastSync(DateTimeOffset.UtcNow.AddSeconds(-30)).ShouldBe("now");

    [Fact]
    public void when_last_sync_was_five_minutes_ago_then_format_last_sync_returns_five_m() =>
        FolderTreeRow.FormatLastSync(DateTimeOffset.UtcNow.AddMinutes(-5)).ShouldBe("5 m");

    [Fact]
    public void when_last_sync_was_three_hours_ago_then_format_last_sync_returns_three_h() =>
        FolderTreeRow.FormatLastSync(DateTimeOffset.UtcNow.AddHours(-3)).ShouldBe("3h");

    [Fact]
    public void when_last_sync_was_two_days_ago_then_format_last_sync_returns_two_d() =>
        FolderTreeRow.FormatLastSync(DateTimeOffset.UtcNow.AddDays(-2)).ShouldBe("2 d");
}
