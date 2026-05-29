using AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects;
using AStar.Dev.CloudSyncFunctional.Sync;
using AStar.Dev.FunctionalParadigm;
using PersistenceDriveId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.DriveId;

namespace AStar.Dev.CloudSyncFunctional.Graph;

/// <summary>Provides access to OneDrive via the Microsoft Graph API.</summary>
public interface IGraphService
{
    /// <summary>Returns the OneDrive drive identifier for the given account.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="PersistenceDriveId"/> on success, or a <see cref="GraphError"/> on failure.</returns>
    Task<Result<PersistenceDriveId, GraphError>> GetDriveIdAsync(string accountId, string accessToken, CancellationToken cancellationToken = default);

    /// <summary>Returns the root-level folders for the given account's OneDrive.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of root folders, or a <see cref="GraphError"/> on failure.</returns>
    Task<Result<List<DriveFolder>, GraphError>> GetRootFoldersAsync(string accountId, string accessToken, CancellationToken cancellationToken = default);

    /// <summary>Recursively enumerates all items under the given folder in OneDrive.</summary>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="driveId">The OneDrive drive identifier.</param>
    /// <param name="folderId">The Graph item identifier of the folder to enumerate.</param>
    /// <param name="remotePath">The remote base path used when building item paths.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="DeltaItem"/> representing all items in the subtree, or a <see cref="GraphError"/> on failure.</returns>
    Task<Result<List<DeltaItem>, GraphError>> EnumerateFolderAsync(string accessToken, PersistenceDriveId driveId, string folderId, string remotePath, CancellationToken cancellationToken = default);

    /// <summary>Resolves a remote folder path to its Graph item identifier.</summary>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="driveId">The OneDrive drive identifier.</param>
    /// <param name="remotePath">The remote path relative to the drive root (e.g. <c>Documents/Work</c>).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Some{T}"/> containing the folder item ID when found; <see cref="None{T}"/> when the folder does not exist.</returns>
    Task<Option<string>> GetFolderIdByPathAsync(string accessToken, PersistenceDriveId driveId, string remotePath, CancellationToken cancellationToken = default);

    /// <summary>Returns a pre-signed download URL for the given item.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="itemId">The Graph item identifier of the file to download.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The download URL on success, or a <see cref="GraphError"/> on failure.</returns>
    Task<Result<string, GraphError>> GetDownloadUrlAsync(string accountId, string accessToken, string itemId, CancellationToken cancellationToken = default);

    /// <summary>Uploads a local file to OneDrive using a resumable upload session.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="localPath">The local path of the file to upload.</param>
    /// <param name="remotePath">The destination remote path (filename only, relative to parent folder).</param>
    /// <param name="parentFolderId">The Graph item identifier of the destination parent folder.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The Graph item ID of the uploaded file on success, or a <see cref="GraphError"/> on failure.</returns>
    Task<Result<string, GraphError>> UploadFileAsync(string accountId, string accessToken, string localPath, string remotePath, string parentFolderId, CancellationToken cancellationToken = default);

    /// <summary>Deletes an item from OneDrive.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    /// <param name="accessToken">The OAuth2 bearer token for Graph API calls.</param>
    /// <param name="itemId">The Graph item identifier of the item to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="Unit"/> on success, or a <see cref="GraphError"/> on failure.</returns>
    Task<Result<Unit, GraphError>> DeleteItemAsync(string accountId, string accessToken, string itemId, CancellationToken cancellationToken = default);

    /// <summary>Removes the cached drive context for the given account, forcing a fresh lookup on next use.</summary>
    /// <param name="accountId">The MSAL HomeAccountId identifier.</param>
    void EvictCachedDriveContext(string accountId);
}
