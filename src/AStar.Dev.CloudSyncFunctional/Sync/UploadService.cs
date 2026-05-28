using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <inheritdoc />
public sealed class UploadService(IGraphService graphService) : IUploadService
{
    /// <inheritdoc />
    public Task<Result<string, SyncError>> UploadAsync(string accountId, string accessToken, string localPath, string remotePath, string parentFolderId, CancellationToken cancellationToken = default)
        => graphService.UploadFileAsync(accountId, accessToken, localPath, remotePath, parentFolderId, cancellationToken)
            .MatchAsync<string, GraphError, Result<string, SyncError>>(
                itemId => new Ok<string, SyncError>(itemId),
                error => new Fail<string, SyncError>(SyncErrorFactory.GraphFailed(error)));
}
