using DeJong.Networking.Core.Messages;
using DeJong.Utilities.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DeJong.Networking.Core.Peers
{
    public abstract class Peer
    {
        public NetID ID { get; private set; }
        public PeerConfig Config { get; private set; }
        public PeerStatus Status { get; private set; }

        protected bool CanReceiveConnections { get; set; }

        private RawSocket socket;
        private List<Connection> connections;

        protected ThreadSafeQueue<KeyValuePair<IPEndPoint, OutgoingMsg>> unconnectedToSend;
        private Dictionary<IPEndPoint, Connection> handshakes;

        private WriteableBuffer packetWriteHelper;
        private ReadableBuffer packetReadHelper;
        private int groupCount;

        internal Peer(PeerConfig config)
        {
            Config = config;
            Status = PeerStatus.NotRunning;

            connections = new List<Connection>();
            unconnectedToSend = new ThreadSafeQueue<KeyValuePair<IPEndPoint, OutgoingMsg>>();
            handshakes = new Dictionary<IPEndPoint, Connection>();
            socket = new RawSocket(config);

            packetWriteHelper = new WriteableBuffer(socket.SendBuffer);
            packetReadHelper = new ReadableBuffer(socket.ReceiveBuffer);
        }

        public override string ToString()
        {
            return $"[{ID}: {Status}]";
        }

        protected void Initialize()
        {
            Config.Lock();
            if (Status == PeerStatus.Running) return;

            unconnectedToSend.Clear();
            handshakes.Clear();
            connections.Clear();

            socket.Bind(false);
            ID = new NetID(NetUtils.GetID(socket.BoundEP));
        }

        private void SendMsg(IPEndPoint target, OutgoingMsg msg, int sequenceNum)
        {
            LibHeader libHeader = msg.GenerateHeader(sequenceNum, Config.MTU);
            if (libHeader.Fragment)
            {
                Dictionary<FragmentHeader, OutgoingMsg> fragments = msg.ToFragments(groupCount++, Config.MTU);
                for (int i = 0; i < fragments.Count; i++)
                {
                    KeyValuePair<FragmentHeader, OutgoingMsg> cur = fragments.ElementAt(i);
                    SendPacket(target, cur.Value, libHeader, cur.Key);
                }
            }
            else SendPacket(target, msg, libHeader, default(FragmentHeader));
        }

        private void SendPacket(IPEndPoint target, OutgoingMsg msg, LibHeader libHeader, FragmentHeader fragHeader)
        {
            packetWriteHelper.LengthBits = 0;

            libHeader.WriteToBuffer(packetWriteHelper);
            if (libHeader.Fragment) fragHeader.WriteToBuffer(packetWriteHelper);
            msg.CopyData(socket.SendBuffer, packetWriteHelper.LengthBytes);

            bool connReset;
            socket.SendPacket(packetWriteHelper.LengthBytes + msg.LengthBytes, target, out connReset);
        }
    }
}