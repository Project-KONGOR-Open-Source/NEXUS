namespace KINESIS;

public abstract class ProtocolResponse
{
    // Response size can not exceed 16384 bytes.
    public const int MAXIMUM_RESPONSE_SIZE = 16384;

    private CommandBuffer? _cachedInstance;

    public abstract CommandBuffer Encode();

    public CommandBuffer CommandBuffer
    {
        get
        {
            _cachedInstance ??= Encode();
            return _cachedInstance;
        }
    }
}
