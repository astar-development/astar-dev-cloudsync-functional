using AStar.Dev.CloudSyncFunctional.Sync;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Sync;

public class GivenAFileClassifier
{
    [Fact]
    public void when_token_exactly_matches_keyword_then_classified()
    {
        var classification = new FileClassification("Photos", ["Photos", "image"]);
        var rule = new FileClassificationRule(classification, ["Photos"]);
        var rules = new[] { rule };

        var result = FileClassifier.Classify("/Photos/holiday.jpg", rules);

        result.ShouldContain(c => c.Name == "Photos");
    }

    [Fact]
    public void when_keyword_is_substring_of_token_then_not_classified()
    {
        var classification = new FileClassification("Photos", ["photo"]);
        var rule = new FileClassificationRule(classification, ["photo"]);
        var rules = new[] { rule };

        var result = FileClassifier.Classify("/photographs/img.jpg", rules);

        result.ShouldHaveSingleItem();
        result[0].Name.ShouldBe("Unclassified");
    }

    [Fact]
    public void when_no_rules_match_then_unclassified_returned()
    {
        var result = FileClassifier.Classify("/Videos/movie.mp4", []);

        result.ShouldHaveSingleItem();
        result[0].Name.ShouldBe("Unclassified");
    }

    [Fact]
    public void when_path_matches_multiple_rules_then_all_returned()
    {
        var photosClassification = new FileClassification("Photos", ["photo"]);
        var workClassification = new FileClassification("Work", ["work"]);
        var rules = new[]
        {
            new FileClassificationRule(photosClassification, ["photo"]),
            new FileClassificationRule(workClassification, ["work"])
        };

        var result = FileClassifier.Classify("/work/photo-archive.zip", rules);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public void when_keyword_match_is_case_insensitive_then_classified()
    {
        var classification = new FileClassification("Photos", ["PHOTOS"]);
        var rule = new FileClassificationRule(classification, ["PHOTOS"]);
        var rules = new[] { rule };

        var result = FileClassifier.Classify("/photos/img.jpg", rules);

        result.ShouldContain(c => c.Name == "Photos");
    }
}
