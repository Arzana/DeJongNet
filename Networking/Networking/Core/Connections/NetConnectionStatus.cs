namespace DeJong.Networking.Core.Connections
{
    public enum NetConnectionStatus : byte
    {
        None,
        InitiatedConnect,
        ReceivedInitiation,
        RespondedAwaitingApproval,
        RespondedConnect,
        Connected,
        Disconnecting,
        Disconnected
    }
}