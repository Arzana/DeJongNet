namespace DeJong.Networking.Core.Messages
{
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
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

        public static OutgoingMsg Ping(int pingNum)
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.Ping);
            msg.Write(pingNum);
            msg.Write((float)NetTime.Now);
            return msg;
        }

        public static OutgoingMsg Pong(int pingNum)
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.Pong);
            msg.Write(pingNum);
            msg.Write((float)NetTime.Now);
            return msg;
        }

        public static OutgoingMsg Connect(string app, long id, OutgoingMsg hail)
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.Connect);
            msg.Write(app);
            msg.Write(id);
            msg.Write((float)NetTime.Now);
            hail?.CopyData(msg);
            return msg;
        }

        public static OutgoingMsg ConnectResponse(string app, long id, OutgoingMsg hail)
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.ConnectResponse);
            msg.Write(app);
            msg.Write(id);
            hail?.CopyData(msg);
            return msg;
        }

        public static OutgoingMsg ConnectionEstablished()
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.ConnectionEstablished);
            msg.Write((float)NetTime.Now);
            return msg;
        }

        public static OutgoingMsg Disconnect(string reason)
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.Disconnect);
            msg.Write(reason);
            return msg;
        }

        public static OutgoingMsg Discovery()
        {
            return new OutgoingMsg(MsgType.Discovery);
        }

        public static OutgoingMsg DiscoveryResponse(OutgoingMsg sec)
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.DiscoveryResponse);
            sec?.CopyData(msg);
            return msg;
        }
    }
}