using AStar.Dev.CloudSyncFunctional.Sync;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Sync;

public class GivenASyncJob
{
    [Fact]
    public void when_download_job_created_via_factory_then_properties_set()
    {
        var remoteModified = DateTimeOffset.UtcNow;

        var job = SyncJobFactory.CreateDownload("id1", "/remote/file.txt", "/local/file.txt", "etag123", remoteModified);

        job.ShouldBeOfType<DownloadJob>();
        var download = (DownloadJob)job;
        download.ItemId.ShouldBe("id1");
        download.RemotePath.ShouldBe("/remote/file.txt");
    }

    [Fact]
    public void when_upload_job_created_via_factory_then_properties_set()
    {
        var job = SyncJobFactory.CreateUpload("/local/file.txt", "/remote/file.txt", "parentId");

        job.ShouldBeOfType<UploadJob>();
        var upload = (UploadJob)job;
        upload.LocalPath.ShouldBe("/local/file.txt");
    }
}
