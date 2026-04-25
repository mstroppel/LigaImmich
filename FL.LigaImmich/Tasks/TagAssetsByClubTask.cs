using FL.LigaImmich.Clubs;
using FL.LigaImmich.ImmichClient;
using FL.LigaImmich.Scheduling;
using Microsoft.Extensions.Logging;

namespace FL.LigaImmich.Tasks;

internal sealed class TagAssetsByClubTask : IScheduledTask
{
    private const int BulkTagBatchSize = 500;
    private const int UpsertTagBatchSize = 100;

    private readonly IImmichClient _immichClient;
    private readonly ILogger<TagAssetsByClubTask> _logger;

    public TagAssetsByClubTask(IImmichClient immichClient, ILogger<TagAssetsByClubTask> logger)
    {
        _immichClient = immichClient;
        _logger = logger;
    }

    public string Name => "TagAssetsByClub";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await RemoveLegacyFlatClubTagsAsync(cancellationToken);

        var tagIdByValue = await EnsureClubTagsAsync(cancellationToken);

        var folderPaths = await _immichClient.GetUniqueOriginalPathsAsync(cancellationToken);
        _logger.LogInformation("Fetched {Count} unique folder paths from Immich.", folderPaths.Count);

        var assetsByTag = new Dictionary<string, HashSet<Guid>>(StringComparer.Ordinal);

        foreach (var folder in folderPaths)
        {
            if (!ClubFolderMap.TryResolveTag(folder, out var tagValue))
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

    private async Task<Dictionary<string, Guid>> EnsureClubTagsAsync(CancellationToken cancellationToken)
    {
        var byValue = new Dictionary<string, Guid>(StringComparer.Ordinal);
        foreach (var batch in ClubFolderMap.TagValues
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Chunk(UpsertTagBatchSize))
        {
            var upsert = new TagUpsertDto();
            foreach (var value in batch)
            {
                upsert.Tags.Add(value);
            }

            var tags = await _immichClient.UpsertTagsAsync(upsert, cancellationToken);

            foreach (var tag in tags)
            {
                if (Guid.TryParse(tag.Id, out var id))
                {
                    byValue[tag.Value] = id;
                }
            }
        }

        return byValue;
    }

    private async Task RemoveLegacyFlatClubTagsAsync(CancellationToken cancellationToken)
    {
        var allTags = await _immichClient.GetAllTagsAsync(cancellationToken);
        var legacyLeafNames = new HashSet<string>(ClubFolderMap.LeafTagNames, StringComparer.Ordinal);

        foreach (var tag in allTags)
        {
            if (!string.IsNullOrEmpty(tag.ParentId))
            {
                continue;
            }

            if (!legacyLeafNames.Contains(tag.Value))
            {
                continue;
            }

            if (!Guid.TryParse(tag.Id, out var id))
            {
                continue;
            }

            await _immichClient.DeleteTagAsync(id, cancellationToken);
            _logger.LogInformation("Removed legacy flat club tag {TagValue} ({TagId}); assets will be re-tagged under {ParentTag}.", tag.Value, id, ClubFolderMap.ParentTag);
        }
    }
}
