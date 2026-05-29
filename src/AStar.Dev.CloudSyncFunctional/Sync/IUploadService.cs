using AStar.Dev.FunctionalParadigm;

namespace AStar.Dev.CloudSyncFunctional.Sync;

/// <summary>Uploads local files to OneDrive via the Graph API.</summary>
public interface IUploadService
{
    /// <summary>Uploads a local file to the specified remote location.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="localPath">The local path of the file to upload.</param>
    /// <param name="remotePath">The destination remote OneDrive path (filename only, relative to parent folder).</param>
    /// <param name="parentFolderId">The Graph item identifier of the destination parent folder.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The Graph item ID of the uploaded file on success, or a <see cref="SyncError"/> on failure.</returns>
    Task<Result<string, SyncError>> UploadAsync(string accountId, string accessToken, string localPath, string remotePath, string parentFolderId, CancellationToken cancellationToken = default);
}
