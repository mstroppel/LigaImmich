using FL.LigaImmich.Clubs;

namespace FL.LigaImmich.Tests;

public class ClubFolderMapTests
{
    [Theory]
    [InlineData("Digitalfoto/2025/M-Musikverein/M_2025-01-11_Konzert/M_2025-01-11_001.JPG", "Musikverein")]
    [InlineData("/Digitalfoto/2024/N-Narrenzunft/Umzug", "Narrenzunft")]
    [InlineData("Digitalfoto\\2025\\C-Gemischter_Chor\\Auftritt", "Gemischter Chor")]
    [InlineData("Digitalfoto/2025/P-Personen_und_Begebenheiten/event", "Personen und Begebenheiten")]
    [InlineData("Digitalfoto/2025/T-TSV", "TSV")]
    public void TryResolveTag_ReturnsTagName_ForKnownClubFolder(string path, string expected)
    {
        Assert.True(ClubFolderMap.TryResolveTag(path, out var tag));
        Assert.Equal(expected, tag);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Digitalfoto/2025/Sonstiges/file.jpg")]
    [InlineData("Digitalfoto/2025/Musikverein/file.jpg")]
    public void TryResolveTag_ReturnsFalse_WhenNoClubFolderPresent(string path)
    {
        Assert.False(ClubFolderMap.TryResolveTag(path, out var tag));
        Assert.Equal(string.Empty, tag);
    }

    [Fact]
    public void TryResolveTag_IsCaseInsensitive_OnFolderToken()
    {
        Assert.True(ClubFolderMap.TryResolveTag("Digitalfoto/2025/m-musikverein/img.jpg", out var tag));
        Assert.Equal("Musikverein", tag);
    }

    [Fact]
    public void TagNames_IncludesAllExpectedClubs()
    {
        Assert.Contains("Musikverein", ClubFolderMap.TagNames);
        Assert.Contains("Feuerwehr", ClubFolderMap.TagNames);
        Assert.Contains("Kegler", ClubFolderMap.TagNames);
        Assert.Equal(17, ClubFolderMap.TagNames.Count);
    }
}
