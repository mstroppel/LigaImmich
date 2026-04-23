using FL.LigaImmich.Folders;
using FL.LigaImmich.ImmichClient;
using FL.LigaImmich.Scheduling;
using Microsoft.Extensions.Logging;

namespace FL.LigaImmich.Tasks;

internal sealed class TagAssetsByYearTask : IScheduledTask
{
    private const int BulkTagBatchSize = 500;

    private readonly IImmichClient _immichClient;
    private readonly ILogger<TagAssetsByYearTask> _logger;

    public TagAssetsByYearTask(IImmichClient immichClient, ILogger<TagAssetsByYearTask> logger)
    {
        _immichClient = immichClient;
        _logger = logger;
    }

    public string Name => "TagAssetsByYear";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var folderPaths = await _immichClient.GetUniqueOriginalPathsAsync(cancellationToken);
        _logger.LogInformation("Fetched {Count} unique folder paths from Immich.", folderPaths.Count);

        var assetsByTag = new Dictionary<string, HashSet<Guid>>(StringComparer.Ordinal);

        foreach (var folder in folderPaths)
        {
            if (!YearTagResolver.TryResolveTag(folder, out var tagValue))
            {
                continue;
            }

            var assets = await _immichClient.GetAssetsByOriginalPathAsync(folder, cancellationToken);
            if (assets.Count == 0)
            {
                continue;
            }

            if (!assetsByTag.TryGetValue(tagValue, out var set))
            {
                assetsByTag[tagValue] = set = [];
            }

            foreach (var asset in assets)
            {
                if (Guid.TryParse(asset.Id, out var assetId)
                    && !(asset.Tags?.Any(t => string.Equals(t.Value, tagValue, StringComparison.Ordinal)) ?? false))
                {
                    set.Add(assetId);
                }
            }
        }

        if (assetsByTag.Count == 0)
        {
            return;
        }

        var tagIdByValue = await EnsureTagsAsync(assetsByTag.Keys, cancellationToken);

        foreach (var (tagValue, assetIds) in assetsByTag)
        {
            if (assetIds.Count == 0)
            {
                continue;
            }

            if (!tagIdByValue.TryGetValue(tagValue, out var tagId))
            {
                _logger.LogWarning("Skipping tag {TagValue}: no id resolved after upsert.", tagValue);
                continue;
            }

            var tagged = 0;
            foreach (var batch in assetIds.Chunk(BulkTagBatchSize))
            {
                var dto = new TagBulkAssetsDto
                {
                    TagIds = { tagId },
                };
                foreach (var id in batch)
                {
                    dto.AssetIds.Add(id);
                }

                var response = await _immichClient.BulkTagAssetsAsync(dto, cancellationToken);
                tagged += response.Count;
            }

            _logger.LogInformation("Tagged {Tagged}/{Total} assets with {TagValue}.", tagged, assetIds.Count, tagValue);
        }
    }

    private async Task<Dictionary<string, Guid>> EnsureTagsAsync(IEnumerable<string> tagValues, CancellationToken cancellationToken)
    {
        var upsert = new TagUpsertDto();
        foreach (var value in tagValues)
        {
            upsert.Tags.Add(value);
        }

        var tags = await _immichClient.UpsertTagsAsync(upsert, cancellationToken);

        var byValue = new Dictionary<string, Guid>(StringComparer.Ordinal);
        foreach (var tag in tags)
        {
            if (Guid.TryParse(tag.Id, out var id))
            {
                byValue[tag.Value] = id;
            }
        }

        return byValue;
    }
}
