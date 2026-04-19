using FL.LigaImmich.Clubs;
using FL.LigaImmich.ImmichClient;
using FL.LigaImmich.Scheduling;
using Microsoft.Extensions.Logging;

namespace FL.LigaImmich.Tasks;

internal sealed class TagAssetsByClubTask : IScheduledTask
{
    private const int BulkTagBatchSize = 500;

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
        var tagIdByName = await EnsureClubTagsAsync(cancellationToken);

        var folderPaths = await _immichClient.GetUniqueOriginalPathsAsync(cancellationToken);
        _logger.LogInformation("Fetched {Count} unique folder paths from Immich.", folderPaths.Count);

        var assetsByTag = new Dictionary<string, HashSet<Guid>>(StringComparer.Ordinal);

        foreach (var folder in folderPaths)
        {
            if (!ClubFolderMap.TryResolveTag(folder, out var tagName))
            {
                continue;
            }

            var assets = await _immichClient.GetAssetsByOriginalPathAsync(folder, cancellationToken);
            if (assets.Count == 0)
            {
                continue;
            }

            if (!assetsByTag.TryGetValue(tagName, out var set))
            {
                assetsByTag[tagName] = set = [];
            }

            foreach (var asset in assets)
            {
                if (Guid.TryParse(asset.Id, out var assetId))
                {
                    set.Add(assetId);
                }
            }
        }

        foreach (var (tagName, assetIds) in assetsByTag)
        {
            if (assetIds.Count == 0)
            {
                continue;
            }

            if (!tagIdByName.TryGetValue(tagName, out var tagId))
            {
                _logger.LogWarning("Skipping tag {TagName}: no id resolved after upsert.", tagName);
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

            _logger.LogInformation("Tagged {Tagged}/{Total} assets with {TagName}.", tagged, assetIds.Count, tagName);
        }
    }

    private async Task<Dictionary<string, Guid>> EnsureClubTagsAsync(CancellationToken cancellationToken)
    {
        var upsert = new TagUpsertDto();
        foreach (var name in ClubFolderMap.TagNames)
        {
            upsert.Tags.Add(name);
        }

        var tags = await _immichClient.UpsertTagsAsync(upsert, cancellationToken);

        var byName = new Dictionary<string, Guid>(StringComparer.Ordinal);
        foreach (var tag in tags)
        {
            if (Guid.TryParse(tag.Id, out var id))
            {
                byName[tag.Value] = id;
            }
        }

        return byName;
    }
}
