using FL.LigaImmich.Folders;

namespace FL.LigaImmich.Tests;

public class FolderTagResolverTests
{
    [Theory]
    [InlineData(
        "archiv/Digitalfoto/2019/F-Film-Liga/F_2019-07-20_Sommerfest",
        "Digitalfoto/2019/F-Film-Liga/F_2019-07-20_Sommerfest")]
    [InlineData(
        "/usr/src/app/external/archiv/Digitalfoto/2019/M-Musikverein/M_2019-05-00_Probenwoche",
        "Digitalfoto/2019/M-Musikverein/M_2019-05-00_Probenwoche")]
    [InlineData(
        "Digitalfoto\\2025\\C-Gemischter_Chor\\Auftritt",
        "Digitalfoto/2025/C-Gemischter_Chor/Auftritt")]
    [InlineData(
        "Digitalfoto/2025",
        "Digitalfoto/2025")]
    public void TryResolveTag_BuildsTagFromArchiveRoot_OnwardsThroughFolderPath(string path, string expected)
    {
        Assert.True(FolderTagResolver.TryResolveTag(path, out var tag));
        Assert.Equal(expected, tag);
    }

    [Theory]
    [InlineData("")]
    [InlineData("archiv/Scan-Dia/2019/F-Film-Liga")]
    [InlineData("some/other/tree/without/the/anchor")]
    public void TryResolveTag_ReturnsFalse_WhenArchiveRootNotInPath(string path)
    {
        Assert.False(FolderTagResolver.TryResolveTag(path, out var tag));
        Assert.Equal(string.Empty, tag);
    }

    [Fact]
    public void TryResolveTag_ReturnsFalse_WhenArchiveRootIsTheLastSegment()
    {
        // Nothing to tag below the anchor — the resolver should not emit a bare
        // "Digitalfoto" tag when no year/club/event folder follows.
        Assert.False(FolderTagResolver.TryResolveTag("archiv/Digitalfoto", out var tag));
        Assert.Equal(string.Empty, tag);
    }

    [Fact]
    public void TryResolveTag_MatchesArchiveRootCaseInsensitively_AndNormalizesToCanonicalCasing()
    {
        Assert.True(FolderTagResolver.TryResolveTag("archiv/digitalfoto/2019/F-Film-Liga/event", out var tag));
        Assert.Equal("Digitalfoto/2019/F-Film-Liga/event", tag);
    }

    [Fact]
    public void ArchiveRoots_IncludesDigitalfoto()
    {
        Assert.Contains("Digitalfoto", FolderTagResolver.ArchiveRoots);
    }
}
