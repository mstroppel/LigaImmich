using FL.LigaImmich.Clubs;

namespace FL.LigaImmich.Tests;

public class ClubFolderMapTests
{
    [Theory]
    [InlineData("Digitalfoto/2025/M-Musikverein/M_2025-01-11_Konzert/M_2025-01-11_001.JPG", "_Vereine/Musikverein")]
    [InlineData("/Digitalfoto/2024/N-Narrenzunft/Umzug", "_Vereine/Narrenzunft")]
    [InlineData("Digitalfoto\\2025\\C-Gemischter_Chor\\Auftritt", "_Vereine/Gemischter Chor")]
    [InlineData("Digitalfoto/2025/P-Personen_und_Begebenheiten/event", "_Vereine/Personen und Begebenheiten")]
    [InlineData("Digitalfoto/2025/T-TSV", "_Vereine/TSV")]
    public void TryResolveTag_ReturnsNestedTagValue_ForKnownClubFolder(string path, string expected)
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
        Assert.Equal("_Vereine/Musikverein", tag);
    }

    [Fact]
    public void LeafTagNames_IncludesAllExpectedClubs()
    {
        Assert.Contains("Musikverein", ClubFolderMap.LeafTagNames);
        Assert.Contains("Feuerwehr", ClubFolderMap.LeafTagNames);
        Assert.Contains("Kegler", ClubFolderMap.LeafTagNames);
        Assert.Equal(17, ClubFolderMap.LeafTagNames.Count);
    }

    [Fact]
    public void TagValues_AreAllNestedUnderVereineParent()
    {
        Assert.Equal(ClubFolderMap.LeafTagNames.Count, ClubFolderMap.TagValues.Count);
        Assert.All(ClubFolderMap.TagValues, value => Assert.StartsWith("_Vereine/", value));
    }
}
