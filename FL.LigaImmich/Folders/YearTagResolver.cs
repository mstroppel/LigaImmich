namespace FL.LigaImmich.Folders;

internal static class YearTagResolver
{
    public const string TagPrefix = "Jahr";

    private static readonly char[] Separators = ['/', '\\'];

    public static bool TryResolveTag(string path, out string tagValue)
    {
        if (!string.IsNullOrEmpty(path))
        {
            var segments = path.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (!FolderTagResolver.ArchiveRoots.Contains(segments[i], StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                var yearSegment = segments[i + 1];
                if (yearSegment.Length == 4 && int.TryParse(yearSegment, out _))
                {
                    tagValue = $"{TagPrefix}/{yearSegment}";
                    return true;
                }

                break;
            }
        }

        tagValue = string.Empty;
        return false;
    }
}
