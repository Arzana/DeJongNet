namespace DeJong.Networking.Core.Messages
{
    internal enum MsgType : byte
    {
        Unconnected,
        Unreliable,
        UnreliableOrdered,
        Reliable,
        ReliableOrdered,
        LibraryError,
        Ping, // used for RTT calculation
        Pong, // used for RTT calculation
        Connect,
        ConnectResponse,
        ConnectionEstablished,
        Acknowledge,
        Disconnect,
        Discovery,
        DiscoveryResponse,
        NatPunchMessage, // send between peers
        NatIntroduction, // send to master server
        NatIntroductionConfirmRequest,
        NatIntroductionConfirmed,
        ExpandMTURequest,
        ExpandMTUSuccess
    }
}