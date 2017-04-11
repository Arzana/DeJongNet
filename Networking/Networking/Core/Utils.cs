namespace DeJong.Networking.Core
{
    using Msg;

    internal static class Utils
    {
        public static NetDeliveryMethod GetDeliveryMethod(NetMsgType mtp)
        {
            if (mtp >= NetMsgType.UserReliableOrdered1) return NetDeliveryMethod.ReliableOrdered;
            else if (mtp >= NetMsgType.UserReliableSequenced1) return NetDeliveryMethod.ReliableSequenced;
            else if (mtp >= NetMsgType.UserReliableUnordered) return NetDeliveryMethod.ReliableUnordered;
            else if (mtp >= NetMsgType.UserSequenced1) return NetDeliveryMethod.UnreliableSequenced;
            else return NetDeliveryMethod.Unreliable;
        }
    }
}