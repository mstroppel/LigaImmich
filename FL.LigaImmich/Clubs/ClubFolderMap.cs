namespace FL.LigaImmich.Clubs;

internal static class ClubFolderMap
{
    public const string ParentTag = "_Vereine";

    // Source of truth: https://github.com/filmliga66/archivstruktur/blob/main/reference/clubs.json
    private static readonly Dictionary<string, string> FolderToLeafTag = new(StringComparer.OrdinalIgnoreCase)
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

    public static IReadOnlyCollection<string> LeafTagNames { get; } =
        FolderToLeafTag.Values.Distinct(StringComparer.Ordinal).ToArray();

    public static IReadOnlyCollection<string> TagValues { get; } =
        LeafTagNames.Select(ToTagValue).ToArray();

    public static bool TryResolveTag(string path, out string tagValue)
    {
        if (!string.IsNullOrEmpty(path))
        {
            foreach (var segment in path.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
            {
                if (FolderToLeafTag.TryGetValue(segment, out var leaf))
                {
                    tagValue = ToTagValue(leaf);
                    return true;
                }
            }
        }

        tagValue = string.Empty;
        return false;
    }

    public static string ToTagValue(string leaf) => $"{ParentTag}/{leaf}";

    private static readonly char[] Separators = ['/', '\\'];
}
