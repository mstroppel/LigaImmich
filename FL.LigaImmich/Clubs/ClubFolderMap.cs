namespace FL.LigaImmich.Clubs;

internal static class ClubFolderMap
{
    // Source of truth: https://github.com/filmliga66/archivstruktur/blob/main/reference/clubs.json
    private static readonly Dictionary<string, string> FolderToTag = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A-Albverein"] = "Albverein",
        ["C-Gemischter_Chor"] = "Gemischter Chor",
        ["D-Dorffest"] = "Dorffest",
        ["F-Film-Liga"] = "Film-Liga",
        ["G-Gemeinde"] = "Gemeinde",
        ["H-Hochzeiten"] = "Hochzeiten",
        ["K-Kirche"] = "Kirche",
        ["L-Ledige"] = "Ledige",
        ["M-Musikverein"] = "Musikverein",
        ["N-Narrenzunft"] = "Narrenzunft",
        ["P-Personen_und_Begebenheiten"] = "Personen und Begebenheiten",
        ["R-Fischerverein"] = "Fischerverein",
        ["S-Schuetzenverein"] = "Schützenverein",
        ["T-TSV"] = "TSV",
        ["V-Ansichten"] = "Ansichten",
        ["W-Feuerwehr"] = "Feuerwehr",
        ["Z-Kegler"] = "Kegler",
    };

    public static IReadOnlyCollection<string> TagNames { get; } =
        FolderToTag.Values.Distinct(StringComparer.Ordinal).ToArray();

    public static bool TryResolveTag(string path, out string tagName)
    {
        if (!string.IsNullOrEmpty(path))
        {
            foreach (var segment in path.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
            {
                if (FolderToTag.TryGetValue(segment, out var resolved))
                {
                    tagName = resolved;
                    return true;
                }
            }
        }

        tagName = string.Empty;
        return false;
    }

    private static readonly char[] Separators = ['/', '\\'];
}
