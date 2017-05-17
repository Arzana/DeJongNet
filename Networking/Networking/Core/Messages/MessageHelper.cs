namespace DeJong.Networking.Core.Messages
{
    internal static class MessageHelper
    {
        public static OutgoingMsg Ack(MsgType type, int channel, int sequenceNum)
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.Acknowledge);
            msg.WritePartial((byte)type, 4);
            msg.WritePartial((byte)(channel & 255), 4);
            msg.Write((short)sequenceNum);
            return msg;
        }
    }
}