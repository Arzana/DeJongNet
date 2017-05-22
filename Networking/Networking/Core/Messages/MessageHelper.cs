namespace DeJong.Networking.Core.Messages
{
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class MessageHelper
    {
        public static OutgoingMsg Ack(OutgoingMsg msg, MsgType type, int channel, int sequenceNum)
        {
            msg.WritePartial((byte)type, 4);
            msg.WritePartial((byte)(channel & 255), 4);
            msg.Write((short)sequenceNum);
            return msg;
        }

        public static OutgoingMsg Ping(OutgoingMsg msg, int pingNum)
        {
            msg.Write(pingNum);
            msg.Write((float)NetTime.Now);
            return msg;
        }

        public static OutgoingMsg Pong(OutgoingMsg msg, int pingNum)
        {
            msg.Write(pingNum);
            msg.Write((float)NetTime.Now);
            return msg;
        }

        public static OutgoingMsg Connect(OutgoingMsg msg, string app, long id, OutgoingMsg hail)
        {
            msg.Write(app);
            msg.Write(id);
            msg.Write((float)NetTime.Now);
            hail?.CopyData(msg);
            return msg;
        }

        public static OutgoingMsg ConnectResponse(OutgoingMsg msg, string app, long id, OutgoingMsg hail)
        {
            msg.Write(app);
            msg.Write(id);
            hail?.CopyData(msg);
            return msg;
        }

        public static OutgoingMsg ConnectionEstablished(OutgoingMsg msg)
        {
            msg.Write((float)NetTime.Now);
            return msg;
        }

        public static OutgoingMsg Disconnect(OutgoingMsg msg, string reason)
        {
            msg.Write(reason);
            return msg;
        }

        public static OutgoingMsg DiscoveryResponse(OutgoingMsg msg, OutgoingMsg sec)
        {
            sec?.CopyData(msg);
            return msg;
        }
    }
}