namespace DeJong.Networking.Core.Peers
{
    using Messages;

    public sealed partial class Connection
    {
        internal void Ping()
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.Ping);
            msg.Write(pingCount++);
            msg.Write((float)NetTime.Now);
            toSend.Enqueue(msg);
        }

        internal void Pong(int pingNum)
        {
            OutgoingMsg msg = new OutgoingMsg(MsgType.Pong);
            msg.Write(pingNum);
            msg.Write((float)NetTime.Now);
            toSend.Enqueue(msg);
        }
    }
}
