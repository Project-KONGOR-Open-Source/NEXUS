namespace KINESIS;

public interface IConnectedSubject
{
    /// <summary>
    ///     Disconnects the Socket associated with this Subject and cleans up any remaining references to it.
    /// </summary>
    void Disconnect(string disconnectReason);
}
