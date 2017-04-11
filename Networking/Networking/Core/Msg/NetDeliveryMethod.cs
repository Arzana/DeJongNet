namespace DeJong.Networking.Core.Msg
{
    public enum NetDeliveryMethod : byte
    {
        Unnknown = 0,
        Unreliable = 1,
        UnreliableSequenced,
        ReliableUnordered,
        ReliableSequenced,
        ReliableOrdered
    }
}