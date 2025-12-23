namespace KINESIS.Manager;

// Message sent to a manager by the ChatServer that describes some of the configurable ChatServer properties,
// such as whether it supports on-demand replay uploads (via FTP or HTTP), etc.
public class UpdateManagerOptionsResponse : ProtocolResponse
{
    // Whether to submit stats.
    private readonly bool SubmitStatsEnabled;

    // Whether to upload replays.
    private readonly bool UploadReplaysEnabled;

    // Whether to upload replay to FTP on demand.
    private readonly bool UploadToFTPOnDemandEnabled;

    // Whether to upload replay to HTTP on demand.
    private readonly bool UploadToHTTPOnDemandEnabled;

    // Whether to re-submit stats.
    private readonly bool ResubmitStatsEnabled;

    // Unclear, but related to quests.
    private readonly int StatsResubmitMatchIDCutoff;

    public UpdateManagerOptionsResponse(bool submitStatsEnabled, bool uploadReplaysEnabled, bool uploadToFTPOnDemandEnabled, bool uploadToHTTPOnDemandEnabled, bool resubmitStatsEnabled, int statsResubmitMatchIDCutoff)
    {
        SubmitStatsEnabled = submitStatsEnabled;
        UploadReplaysEnabled = uploadReplaysEnabled;
        UploadToFTPOnDemandEnabled = uploadToFTPOnDemandEnabled;
        UploadToHTTPOnDemandEnabled = uploadToHTTPOnDemandEnabled;
        ResubmitStatsEnabled = resubmitStatsEnabled;
        StatsResubmitMatchIDCutoff = statsResubmitMatchIDCutoff;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.UpdateManagerOptions);
        buffer.WriteInt8(Convert.ToByte(SubmitStatsEnabled));
        buffer.WriteInt8(Convert.ToByte(UploadReplaysEnabled));
        buffer.WriteInt8(Convert.ToByte(UploadToFTPOnDemandEnabled));
        buffer.WriteInt8(Convert.ToByte(UploadToHTTPOnDemandEnabled));
        buffer.WriteInt8(Convert.ToByte(ResubmitStatsEnabled));
        buffer.WriteInt32(StatsResubmitMatchIDCutoff);
        return buffer;
    }
}
