namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

# nullable disable

/// <summary>
///     TCP Server Is Used To Connect, Disconnect And Manage TCP Sessions
/// </summary>
/// <remarks>Thread-Safe</remarks>
public class TCPServer : IDisposable
{
    /// <summary>
    ///     Initialize TCP Server With A Given IP Address And Port Number
    /// </summary>
    /// <param name="address">IP Address</param>
    /// <param name="port">Port Number</param>
    public TCPServer(IPAddress address, int port) : this(new IPEndPoint(address, port)) { }

    /// <summary>
    ///     Initialize TCP Server With A Given IP Address And Port Number
    /// </summary>
    /// <param name="address">IP Address</param>
    /// <param name="port">Port Number</param>
    public TCPServer(string address, int port) : this(new IPEndPoint(IPAddress.Parse(address), port)) { }

    /// <summary>
    ///     Initialize TCP Server With A Given DNS Endpoint
    /// </summary>
    /// <param name="endpoint">DNS Endpoint</param>
    public TCPServer(DnsEndPoint endpoint) : this(endpoint as EndPoint, endpoint.Host, endpoint.Port) { }

    /// <summary>
    ///     Initialize TCP Server With A Given IP Endpoint
    /// </summary>
    /// <param name="endpoint">IP Endpoint</param>
    public TCPServer(IPEndPoint endpoint) : this(endpoint as EndPoint, endpoint.Address.ToString(), endpoint.Port) { }

    /// <summary>
    ///     Initialize TCP Server With A Given Endpoint, Address And Port
    /// </summary>
    /// <param name="endpoint">Endpoint</param>
    /// <param name="address">Server Address</param>
    /// <param name="port">Server Port</param>
    private TCPServer(EndPoint endpoint, string address, int port)
    {
        ID = Guid.CreateVersion7();
        Address = address;
        Port = port;
        Endpoint = endpoint;
    }

    /// <summary>
    ///     Server ID
    /// </summary>
    public Guid ID { get; }

    /// <summary>
    ///     TCP Server Address
    /// </summary>
    public string Address { get; }

    /// <summary>
    ///     TCP Server Port
    /// </summary>
    public int Port { get; }

    /// <summary>
    ///     Endpoint
    /// </summary>
    public EndPoint Endpoint { get; private set; }

    /// <summary>
    ///     Number Of Sessions Connected To The Server
    /// </summary>
    public long ConnectedSessions { get { return Sessions.Count; } }

    /// <summary>
    ///     Number Of Bytes Pending Sent By The Server
    /// </summary>
    public long BytesPending { get { return _bytesPending; } }

    /// <summary>
    ///     Number Of Bytes Sent By The Server
    /// </summary>
    public long BytesSent { get { return _bytesSent; } }

    /// <summary>
    ///     Number Of Bytes Received By The Server
    /// </summary>
    public long BytesReceived { get { return _bytesReceived; } }

    /// <summary>
    ///     Option: Acceptor Backlog Size
    /// </summary>
    /// <remarks>
    ///     This Option Will Set The Listening Socket's Backlog Size
    /// </remarks>
    public int OptionAcceptorBacklog { get; set; } = 1024;

    /// <summary>
    ///     Option: Dual Mode Socket
    /// </summary>
    /// <remarks>
    ///     Specifies Whether The Socket Is A Dual-Mode Socket Used For Both IPv4 And IPv6
    ///     <br/>
    ///     Will Work Only If Socket Is Bound On IPv6 Address
    /// </remarks>
    public bool OptionDualMode { get; set; }

    /// <summary>
    ///     Option: Keep Alive
    /// </summary>
    /// <remarks>
    ///     This Option Will Setup SO_KEEPALIVE If The Operating System Supports This Feature
    /// </remarks>
    public bool OptionKeepAlive { get; set; }

    /// <summary>
    ///     Option: TCP Keep Alive Time
    /// </summary>
    /// <remarks>
    ///     The Number Of Seconds A TCP Connection Will Remain Alive/Idle Before KeepAlive Probes Are Sent To The Remote
    /// </remarks>
    public int OptionTCPKeepAliveTime { get; set; } = -1;

    /// <summary>
    ///     Option: TCP Keep Alive Interval
    /// </summary>
    /// <remarks>
    ///     The Number Of Seconds A TCP Connection Will Wait For A KeepAlive Response Before Sending Another KeepAlive Probe
    /// </remarks>
    public int OptionTCPKeepAliveInterval { get; set; } = -1;

    /// <summary>
    ///     Option: TCP Keep Alive Retry Count
    /// </summary>
    /// <remarks>
    ///     The Number Of TCP Keep Alive Probes That Will Be Sent Before The Connection Is Terminated
    /// </remarks>
    public int OptionTCPKeepAliveRetryCount { get; set; } = -1;

    /// <summary>
    ///     Option: No Delay
    /// </summary>
    /// <remarks>
    ///     This Option Will Enable/Disable Nagle's Algorithm For TCP Protocol
    /// </remarks>
    public bool OptionNoDelay { get; set; }

    /// <summary>
    ///     Option: Reuse Address
    /// </summary>
    /// <remarks>
    ///     This Option Will Enable/Disable SO_REUSEADDR If The Operating System Supports This Feature
    /// </remarks>
    public bool OptionReuseAddress { get; set; }

    /// <summary>
    ///     Option: Enables A Socket To Be Bound For Exclusive Access
    /// </summary>
    /// <remarks>
    ///     This Option Will Enable/Disable SO_EXCLUSIVEADDRUSE If The Operating System Supports This Feature
    /// </remarks>
    public bool OptionExclusiveAddressUse { get; set; }

    /// <summary>
    ///     Option: Receive Buffer Size
    /// </summary>
    public int OptionReceiveBufferSize { get; set; } = 8192;

    /// <summary>
    ///     Option: Send Buffer Size
    /// </summary>
    public int OptionSendBufferSize { get; set; } = 8192;

    # region Start/Stop Server

    // Server Acceptor
    private Socket _acceptorSocket;
    private SocketAsyncEventArgs _acceptorEventArg;

    // Server Statistic
    internal long _bytesPending;
    internal long _bytesSent;
    internal long _bytesReceived;

    /// <summary>
    ///     Is The Server Started?
    /// </summary>
    public bool IsStarted { get; private set; }

    /// <summary>
    ///     Is The Server Accepting New Clients?
    /// </summary>
    public bool IsAccepting { get; private set; }

    /// <summary>
    ///     Create A New Socket Object
    /// </summary>
    /// <remarks>
    ///     Method May Be Override If You Need To Prepare Some Specific Socket Object In Your Implementation
    /// </remarks>
    /// <returns>Socket Object</returns>
    protected virtual Socket CreateSocket()
    {
        return new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    /// <summary>
    ///     Start The Server
    /// </summary>
    /// <returns>TRUE If The Server Was Successfully Started, Or FALSE If The Server Failed To Start</returns>
    public virtual bool Start()
    {
        Debug.Assert(!IsStarted, "TCP Server Is Already Started");
        if (IsStarted)
            return false;

        // Setup Acceptor Event Arg
        _acceptorEventArg = new SocketAsyncEventArgs();
        _acceptorEventArg.Completed += OnAsyncCompleted;

        // Create A New Acceptor Socket
        _acceptorSocket = CreateSocket();

        // Update The Acceptor Socket Disposed Flag
        IsSocketDisposed = false;

        // Apply The Option: Reuse Address
        _acceptorSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, OptionReuseAddress);

        // Apply The Option: Exclusive Address Use
        _acceptorSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, OptionExclusiveAddressUse);

        // Apply The Option: Dual Mode (This Option Must Be Applied Before Listening)
        if (_acceptorSocket.AddressFamily == AddressFamily.InterNetworkV6)
            _acceptorSocket.DualMode = OptionDualMode;

        // Bind The Acceptor Socket To The Endpoint
        _acceptorSocket.Bind(Endpoint);

        // Refresh The Endpoint Property Based On The Actual Endpoint Created
        Endpoint = _acceptorSocket.LocalEndPoint;

        // Call The Server Starting Handler
        OnStarting();

        // Start Listen To The Acceptor Socket With The Given Accepting Backlog Size
        _acceptorSocket.Listen(OptionAcceptorBacklog);

        // Reset Statistic
        _bytesPending = 0;
        _bytesSent = 0;
        _bytesReceived = 0;

        // Update The Started Flag
        IsStarted = true;

        // Call The Server Started Handler
        OnStarted();

        // Perform The First Server Accept
        IsAccepting = true;
        StartAccept(_acceptorEventArg);

        return true;
    }

    /// <summary>
    ///     Stop The Server
    /// </summary>
    /// <returns>TRUE If The Server Was Successfully Stopped, Or FALSE If The Server Is Already Stopped</returns>
    public virtual bool Stop()
    {
        Debug.Assert(IsStarted, "TCP Server Is Not Started");

        if (!IsStarted)
            return false;

        // Stop Accepting New Clients
        IsAccepting = false;

        // Reset Acceptor Event Arg
        _acceptorEventArg.Completed -= OnAsyncCompleted;

        // Call The Server Stopping Handler
        OnStopping();

        try
        {
            // Close The Acceptor Socket
            _acceptorSocket.Close();

            // Dispose The Acceptor Socket
            _acceptorSocket.Dispose();

            // Dispose Event Arguments
            _acceptorEventArg.Dispose();

            // Update The Acceptor Socket Disposed Flag
            IsSocketDisposed = true;
        }

        catch (ObjectDisposedException) { }

        // Disconnect All Sessions
        DisconnectAll();

        // Update The Started Flag
        IsStarted = false;

        // Call The Server Stopped Handler
        OnStopped();

        return true;
    }

    /// <summary>
    ///     Restart The Server
    /// </summary>
    /// <returns>TRUE If The Server Was Successfully Restarted, Or FALSE If The Server Failed To Restart</returns>
    public virtual bool Restart()
    {
        if (!Stop())
            return false;

        while (IsStarted)
            Thread.Yield();

        return Start();
    }

    # endregion

    # region Accepting Clients

    /// <summary>
    ///     Start Accept A New Client Connection
    /// </summary>
    private void StartAccept(SocketAsyncEventArgs e)
    {
        // Socket Must Be Cleared Since The Context Object Is Being Reused
        e.AcceptSocket = null;

        // Async Accept A New Client Connection
        if (!_acceptorSocket.AcceptAsync(e))
            ProcessAccept(e);
    }

    /// <summary>
    ///     Process Accepted Client Connection
    /// </summary>
    private void ProcessAccept(SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            // Create A New Session To Register
            TCPSession session = CreateSession();

            // Register The Session
            RegisterSession(session);

            // Connect New Session
            session.Connect(e.AcceptSocket);
        }

        else SendError(e.SocketError);

        // Accept The Next Client Connection
        if (IsAccepting)
            StartAccept(e);
    }

    /// <summary>
    /// This Method Is The Callback Method Associated With The Socket.AcceptAsync() Operations And Is Invoked When An Accept Operation Is Complete
    /// </summary>
    private void OnAsyncCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (IsSocketDisposed)
            return;

        ProcessAccept(e);
    }

    # endregion

    # region Session Factory

    /// <summary>
    ///     Create TCP Session Factory Method
    /// </summary>
    /// <returns>TCP Session</returns>
    protected virtual TCPSession CreateSession() { return new TCPSession(this); }

    # endregion

    # region Session Management

    /// <summary>
    ///     Server Sessions
    /// </summary>
    protected readonly ConcurrentDictionary<Guid, TCPSession> Sessions = new ConcurrentDictionary<Guid, TCPSession>();

    /// <summary>
    ///     Disconnect All Connected Sessions
    /// </summary>
    /// <returns>TRUE If All Sessions Were Successfully Disconnected, Or FALSE If The Server Is Not Started</returns>
    public virtual bool DisconnectAll()
    {
        if (!IsStarted)
            return false;

        // Disconnect All Sessions
        foreach (TCPSession session in Sessions.Values)
            session.Disconnect();

        return true;
    }

    /// <summary>
    ///     Find A Session With A Given ID
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <returns>Session With A Given ID, Or NULL If The Session It Not Connected</returns>
    public TCPSession FindSession(Guid id)
    {
        // Try To Find The Required Session
        return Sessions.TryGetValue(id, out TCPSession result) ? result : null;
    }

    /// <summary>
    ///     Register A New Session
    /// </summary>
    /// <param name="session">Session To Register</param>
    internal void RegisterSession(TCPSession session)
    {
        // Register A New Session
        Sessions.TryAdd(session.ID, session);
    }

    /// <summary>
    ///     Unregister Session By ID
    /// </summary>
    /// <param name="id">Session ID</param>
    internal void UnregisterSession(Guid id)
    {
        // Unregister Session By ID
        Sessions.TryRemove(id, out TCPSession _);
    }

    # endregion

    # region Multicasting

    /// <summary>
    ///     Multicast Data To All Connected Sessions
    /// </summary>
    /// <param name="buffer">Buffer To Multicast</param>
    /// <returns>TRUE If The Data Was Successfully Multicasted, Or FALSE If The Data Was Not Multicasted</returns>
    public virtual bool Multicast(byte[] buffer) => Multicast(buffer.AsSpan());

    /// <summary>
    ///     Multicast Data To All Connected Clients
    /// </summary>
    /// <param name="buffer">Buffer To Multicast</param>
    /// <param name="offset">Buffer Offset</param>
    /// <param name="size">Buffer Size</param>
    /// <returns>TRUE If The Data Was Successfully Multicasted, Or FALSE If The Data Was Not Multicasted</returns>
    public virtual bool Multicast(byte[] buffer, long offset, long size) => Multicast(buffer.AsSpan((int)offset, (int)size));

    /// <summary>
    ///     Multicast Data To All Connected Clients
    /// </summary>
    /// <param name="buffer">Buffer To Send As A Span Of Bytes</param>
    /// <returns>TRUE If The Data Was Successfully Multicasted, Or FALSE If The Data Was Not Multicasted</returns>
    public virtual bool Multicast(ReadOnlySpan<byte> buffer)
    {
        if (!IsStarted)
            return false;

        if (buffer.IsEmpty)
            return true;

        // Multicast Data To All Sessions
        foreach (TCPSession session in Sessions.Values)
            session.SendAsync(buffer);

        return true;
    }

    /// <summary>
    ///     Multicast Text To All Connected Clients
    /// </summary>
    /// <param name="text">Text String To Multicast</param>
    /// <returns>TRUE If The Text Was Successfully Multicasted, Or FALSE If The Text Was Not Multicasted</returns>
    public virtual bool Multicast(string text) => Multicast(Encoding.UTF8.GetBytes(text));

    /// <summary>
    ///     Multicast Text To All Connected Clients
    /// </summary>
    /// <param name="text">Text To Multicast As A Span Of Characters</param>
    /// <returns>TRUE If The Text Was Successfully Multicasted, Or FALSE If The Text Was Not Multicasted</returns>
    public virtual bool Multicast(ReadOnlySpan<char> text) => Multicast(Encoding.UTF8.GetBytes(text.ToArray()));

    # endregion

    # region Server Handlers

    /// <summary>
    ///     Handle Server Starting Notification
    /// </summary>
    protected virtual void OnStarting() { }

    /// <summary>
    ///     Handle Server Started Notification
    /// </summary>
    protected virtual void OnStarted() { }

    /// <summary>
    ///     Handle Server Stopping Notification
    /// </summary>
    protected virtual void OnStopping() { }

    /// <summary>
    ///     Handle Server Stopped Notification
    /// </summary>
    protected virtual void OnStopped() { }

    /// <summary>
    ///     Handle Session Connecting Notification
    /// </summary>
    /// <param name="session">Connecting Session</param>
    protected virtual void OnConnecting(TCPSession session) { }

    /// <summary>
    ///     Handle Session Connected Notification
    /// </summary>
    /// <param name="session">Connected Session</param>
    protected virtual void OnConnected(TCPSession session) { }

    /// <summary>
    ///     Handle Session Disconnecting Notification
    /// </summary>
    /// <param name="session">Disconnecting Session</param>
    protected virtual void OnDisconnecting(TCPSession session) { }

    /// <summary>
    ///     Handle Session Disconnected Notification
    /// </summary>
    /// <param name="session">Disconnected Session</param>
    protected virtual void OnDisconnected(TCPSession session) { }

    /// <summary>
    ///     Handle Error Notification
    /// </summary>
    /// <param name="error">Socket Error Code</param>
    protected virtual void OnError(SocketError error) { }

    internal void OnConnectingInternal(TCPSession session) { OnConnecting(session); }
    internal void OnConnectedInternal(TCPSession session) { OnConnected(session); }
    internal void OnDisconnectingInternal(TCPSession session) { OnDisconnecting(session); }
    internal void OnDisconnectedInternal(TCPSession session) { OnDisconnected(session); }

    # endregion

    # region Error handling

    /// <summary>
    ///     Send Error Notification
    /// </summary>
    /// <param name="error">Socket Error Code</param>
    private void SendError(SocketError error)
    {
        // Skip Disconnect Errors
        if ((error == SocketError.ConnectionAborted) ||
            (error == SocketError.ConnectionRefused) ||
            (error == SocketError.ConnectionReset) ||
            (error == SocketError.OperationAborted) ||
            (error == SocketError.Shutdown))
            return;

        OnError(error);
    }

    # endregion

    # region IDisposable Implementation

    /// <summary>
    ///     Disposed Flag
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    ///     Acceptor Socket Disposed Flag
    /// </summary>
    public bool IsSocketDisposed { get; private set; } = true;

    // Implement IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposingManagedResources)
    {
        // The idea here is that Dispose(Boolean) knows whether it is being called to do explicit clean-up (the Boolean is true) versus being called due to a garbage collection event (the Boolean is false).
        // This distinction is useful because, when being disposed explicitly, the Dispose(Boolean) method can safely execute code using reference type fields that refer to other objects knowing for sure that these other objects have not been finalized or disposed of yet.
        // When the Boolean is false, the Dispose(Boolean) method should not execute code that refers to reference type fields, because those objects may have already been finalized.

        if (!IsDisposed)
        {
            if (disposingManagedResources)
            {
                // Dispose Managed Resources Here ...
                Stop();
            }

            // Dispose Unmanaged Resources Here ...

            // Set Large Fields To NULL Here ...

            // Mark As Disposed
            IsDisposed = true;
        }
    }

    # endregion
}
