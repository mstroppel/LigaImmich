using FL.LigaImmich.Folders;

namespace FL.LigaImmich.Tests;

public class YearTagResolverTests
{
    [Theory]
    [InlineData("archiv/Digitalfoto/2019/F-Film-Liga/F_2019-07-20_Sommerfest", "Jahr/2019")]
    [InlineData("/usr/src/app/external/archiv/Digitalfoto/2025/M-Musikverein", "Jahr/2025")]
    [InlineData("Digitalfoto/2025/C-Gemischter_Chor/Auftritt", "Jahr/2025")]
    [InlineData("Digitalfoto\\2025\\C-Gemischter_Chor", "Jahr/2025")]
    [InlineData("archiv/Dia/1975/F-Film-Liga/event", "Jahr/1975")]
    [InlineData("archiv/Negativ/1980/M-Musikverein/Konzert", "Jahr/1980")]
    [InlineData("archiv/Ton/1990/F-Film-Liga/recording", "Jahr/1990")]
    [InlineData("archiv/Video/2000/N-Narrenzunft/Umzug", "Jahr/2000")]
    public void TryResolveTag_ReturnsJahrTag_ForKnownArchiveRootWithYear(string path, string expected)
    {
        Assert.True(YearTagResolver.TryResolveTag(path, out var tag));
        Assert.Equal(expected, tag);
    }

    [Theory]
    [InlineData("")]
    [InlineData("archiv/Digitalfoto")]
    [InlineData("some/other/tree/2025/without/anchor")]
    [InlineData("archiv/Audio/1990/F-Film-Liga")]
    [InlineData("archiv/Digitalfoto/notayear/F-Film-Liga")]
    public void TryResolveTag_ReturnsFalse_WhenNoValidYearAfterArchiveRoot(string path)
    {
        Assert.False(YearTagResolver.TryResolveTag(path, out var tag));
        Assert.Equal(string.Empty, tag);
    }

    [Fact]
    public void TryResolveTag_IsCaseInsensitive_OnArchiveRoot()
    {
        Assert.True(YearTagResolver.TryResolveTag("archiv/digitalfoto/2019/F-Film-Liga", out var tag));
        Assert.Equal("Jahr/2019", tag);
    }

    [Fact]
    public void TagPrefix_IsJahr()
    {
        Assert.Equal("Jahr", YearTagResolver.TagPrefix);
    }
}
