namespace FL.LigaImmich.Folders;

internal static class FolderTagResolver
{
    public const string ParentTag = "Ordnerstruktur";

    private static readonly char[] Separators = ['/', '\\'];

    // Top-level folders that anchor the tag hierarchy. When one of these
    // segments appears in an asset's original path, the tag is built from
    // that segment through the end of the folder path (case-sensitive output,
    // case-insensitive match to tolerate filesystem casing).
    public static IReadOnlyCollection<string> ArchiveRoots { get; } =
    [
        "Digitalfoto",
        "Dia",
        "Negativ",
        "Ton",
        "Video",
    ];

    // Maps the case-insensitive root key to its canonical (configured) casing
    // so the emitted tag is stable regardless of how the filesystem spells it.
    private static readonly Dictionary<string, string> CanonicalRoot =
        ArchiveRoots.ToDictionary(r => r, r => r, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<string> TagValues { get; } =
        ArchiveRoots.Select(ToTagValue).ToArray();

    public static bool TryResolveTag(string path, out string tagValue)
    {
        if (!string.IsNullOrEmpty(path))
        {
            var segments = path.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < segments.Length; i++)
            {
                if (!CanonicalRoot.TryGetValue(segments[i], out var canonical))
                {
                    continue;
                }

                if (i == segments.Length - 1)
                {
                    break;
                }

                segments[i] = canonical;
                tagValue = ToTagValue(string.Join('/', segments[i..]));
                return true;
            }
        }

        tagValue = string.Empty;
        return false;
    }

    public static string ToTagValue(string value) => $"{ParentTag}/{value}";
}
