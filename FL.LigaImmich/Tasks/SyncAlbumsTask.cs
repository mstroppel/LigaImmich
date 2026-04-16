using FL.LigaImmich.ImmichClient;
using FL.LigaImmich.Scheduling;
using Microsoft.Extensions.Logging;

namespace FL.LigaImmich.Tasks;

internal sealed class SyncAlbumsTask : IScheduledTask
{
    private readonly IImmichClient _immichClient;
    private readonly ILogger<SyncAlbumsTask> _logger;

    public SyncAlbumsTask(IImmichClient immichClient, ILogger<SyncAlbumsTask> logger)
    {
        _immichClient = immichClient;
        _logger = logger;
    }

    public string Name => "SyncAlbums";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var albums = await _immichClient.GetAllAlbumsAsync(assetId: null, shared: null, cancellationToken);
        _logger.LogInformation("Fetched {Count} albums from Immich.", albums.Count);
    }
}
