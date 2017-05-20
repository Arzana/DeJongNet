namespace DeJong.Networking.Core
{
    using Messages;
    using System;

    public sealed class DataMessageEventArgs : EventArgs
    {
        public readonly IncommingMsgType Type;
        public readonly DeliveryMethod Method;
        public readonly int Channel;
        public readonly IncommingMsg Message;

        internal DataMessageEventArgs(IncommingMsg msg)
        {
            Type = IncommingMsgType.Data;
            Method = (DeliveryMethod)msg.Header.Type;
            Channel = msg.Header.Channel;
            Message = msg;
        }
    }
}