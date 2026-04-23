namespace FL.LigaImmich.Folders;

internal static class EventTagResolver
{
    public const string ParentTag = "Veranstaltungen";

    private static readonly char[] Separators = ['/', '\\'];

    private static readonly IReadOnlyCollection<KeyValuePair<string, string>> TokenToLeafTag =
    [
        new("GV", "GV"),
        new("Dorffest", "Dorffest"),
        new("Theater", "Theater"),
    ];

    public static IReadOnlyCollection<string> TagValues { get; } =
        TokenToLeafTag.Select(kvp => ToTagValue(kvp.Value)).ToArray();

    public static bool TryResolveTag(string path, out string tagValue)
    {
        if (!string.IsNullOrEmpty(path))
        {
            var segments = path.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

            // Start at the deepest folder segment to prioritize the folder that actually contains the files.
            for (var i = segments.Length - 1; i >= 0; i--)
            {
                foreach (var (token, leafTag) in TokenToLeafTag)
                {
                    if (segments[i].Contains(token, StringComparison.OrdinalIgnoreCase))
                    {
                        tagValue = ToTagValue(leafTag);
                        return true;
                    }
                }
            }
        }

        tagValue = string.Empty;
        return false;
    }

    public static string ToTagValue(string leaf) => $"{ParentTag}/{leaf}";
}
