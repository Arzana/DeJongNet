namespace DeJong.Networking.Core.Messages
{
    internal enum MsgType : byte
    {
        LibraryError = 0,
        Unreliable = 1,
        Ordered = 2,
        Reliable = 3,
        ReliableOrdered = 4,
        Ping = 5,
        Pong = 6,
        Connect = 7,     
        ConnectResponse = 8,
        ConnectionEstablished = 9,
        Acknowledge = 10,
        Disconnect = 11,
        Discovery = 12,
        DiscoveryResponse = 13,
        // TODO: Add NAT intro and MTU expand
    }
}