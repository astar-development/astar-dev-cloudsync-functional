using AStar.Dev.CloudSyncFunctional.Persistence.Entities;
using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Sync;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Sync;

public class GivenASyncRuleEvaluator
{
    private static SyncRuleEntity BuildRule(string path, RuleType ruleType) =>
        new()
        {
            Id = new SyncRuleId(Guid.NewGuid().ToString()),
            AccountId = new AccountId("acc"),
            RemotePath = path,
            RuleType = ruleType
        };

    [Fact]
    public void when_no_rules_match_then_excluded()
    {
        var result = SyncRuleEvaluator.IsIncluded("/SomeFolder", []);

        result.ShouldBeFalse();
    }

    [Fact]
    public void when_include_rule_matches_path_then_included()
    {
        var rules = new[] { BuildRule("/Documents", RuleType.Include) };

        var result = SyncRuleEvaluator.IsIncluded("/Documents/Report.docx", rules);

        result.ShouldBeTrue();
    }

    [Fact]
    public void when_exclude_rule_matches_path_then_excluded()
    {
        var rules = new[] { BuildRule("/Documents", RuleType.Exclude) };

        var result = SyncRuleEvaluator.IsIncluded("/Documents/file.txt", rules);

        result.ShouldBeFalse();
    }

    [Fact]
    public void when_most_specific_rule_wins_include()
    {
        var rules = new[]
        {
            BuildRule("/Documents", RuleType.Include),
            BuildRule("/Documents/Private", RuleType.Exclude)
        };

        var result = SyncRuleEvaluator.IsIncluded("/Documents/Private/secret.txt", rules);

        result.ShouldBeFalse();
    }

    [Fact]
    public void when_most_specific_rule_wins_exclude()
    {
        var rules = new[]
        {
            BuildRule("/Documents", RuleType.Exclude),
            BuildRule("/Documents/Work", RuleType.Include)
        };

        var result = SyncRuleEvaluator.IsIncluded("/Documents/Work/report.txt", rules);

        result.ShouldBeTrue();
    }

    [Fact]
    public void when_path_prefix_matches_but_not_boundary_then_excluded()
    {
        var rules = new[] { BuildRule("/Documents", RuleType.Include) };

        var result = SyncRuleEvaluator.IsIncluded("/DocumentsBackup/file.txt", rules);

        result.ShouldBeFalse();
    }

    [Fact]
    public void when_exact_path_matches_include_rule_then_included()
    {
        var rules = new[] { BuildRule("/Documents", RuleType.Include) };

        var result = SyncRuleEvaluator.IsIncluded("/Documents", rules);

        result.ShouldBeTrue();
    }

    [Fact]
    public void when_tie_on_length_exclude_wins()
    {
        var rules = new[]
        {
            BuildRule("/Documents/Work", RuleType.Include),
            BuildRule("/Documents/Work", RuleType.Exclude)
        };

        var result = SyncRuleEvaluator.IsIncluded("/Documents/Work", rules);

        result.ShouldBeFalse();
    }

    [Fact]
    public void when_default_deny_no_rules_then_excluded()
    {
        var result = SyncRuleEvaluator.IsIncluded("/anything", []);

        result.ShouldBeFalse();
    }
}
