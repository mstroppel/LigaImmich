using FL.LigaImmich.Folders;

namespace FL.LigaImmich.Tests;

public class EventTagResolverTests
{
    [Theory]
    [InlineData("Digitalfoto/2025/F-Film-Liga/F_2025-03-08_GV", "Veranstaltungen/GV")]
    [InlineData("Digitalfoto/2025/F-Film-Liga/F_2025-07-20_Dorffest", "Veranstaltungen/Dorffest")]
    [InlineData("Digitalfoto/2025/F-Film-Liga/F_2025-11-15_Theater", "Veranstaltungen/Theater")]
    [InlineData("Digitalfoto\\2025\\F-Film-Liga\\F_2025-11-15_theater", "Veranstaltungen/Theater")]
    public void TryResolveTag_ReturnsNestedEventTag_WhenFolderContainsSupportedEventToken(string path, string expected)
    {
        Assert.True(EventTagResolver.TryResolveTag(path, out var tag));
        Assert.Equal(expected, tag);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Digitalfoto/2025/F-Film-Liga/Sommerfest")]
    [InlineData("Digitalfoto/2025/F-Film-Liga/Konzert")]
    public void TryResolveTag_ReturnsFalse_WhenNoSupportedEventTokenExists(string path)
    {
        Assert.False(EventTagResolver.TryResolveTag(path, out var tag));
        Assert.Equal(string.Empty, tag);
    }

    [Fact]
    public void TagValues_AreAllNestedUnderVeranstaltungenParent()
    {
        Assert.Contains("Veranstaltungen/GV", EventTagResolver.TagValues);
        Assert.Contains("Veranstaltungen/Dorffest", EventTagResolver.TagValues);
        Assert.Contains("Veranstaltungen/Theater", EventTagResolver.TagValues);
        Assert.All(EventTagResolver.TagValues, value => Assert.StartsWith("Veranstaltungen/", value));
    }
}
