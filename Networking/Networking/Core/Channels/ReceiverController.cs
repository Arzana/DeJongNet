namespace DeJong.Networking.Core.Channels
{
    using Messages;
    using Receiver;
    using Sender;
    using System.Net;
    using Utilities.Core;
    using Utilities.Logging;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class ReceiverController
    {
        public int Size { get; private set; }

        private IPEndPoint remote;
        private EndPoint ep;
        private ReceiverChannelBase[] channels;
        private SenderController sendController;
        private ReadableBuffer readHelper;
        private PeerConfig config;
        private RawSocket socket;

        public ReceiverChannelBase this[int index]
        {
            get
            {
                LoggedException.RaiseIf(index >= Size || index < 0, nameof(ReceiverController), "index ot of range");
                return channels[index];
            }
        }

        public ReceiverController(RawSocket socket, IPEndPoint ep, PeerConfig config, SenderController sender)
        {
            remote = ep;
            this.ep = remote;
            this.config = config;
            this.socket = socket;
            channels = new ReceiverChannelBase[15];
            sendController = sender;
            readHelper = new ReadableBuffer(socket.ReceiveBuffer);

            channels[Size++] = new UnreliableReceiverChannel(socket, ep, config) { ID = 0 };
        }

        public void AddUnreliable(int id)
        {
            CheckNewChannel(id);
            channels[Size++] = new UnreliableReceiverChannel(socket, remote, config) { ID = id };
        }

        public void AddOrdered(int id, OrderChannelBehaviour behaviour)
        {
            CheckNewChannel(id);
            channels[Size++] = new OrderedReceiverChannel(socket, remote, config, behaviour) { ID = id };
        }

        public void AddReliable(int id)
        {
            CheckNewChannel(id);
            channels[Size++] = new ReliableReceiverChannel(socket, remote, config, sendController.LibSender) { ID = id };
        }

        public void AddReliableOrdered(int id, OrderChannelBehaviour behaviour)
        {
            CheckNewChannel(id);
            channels[Size++] = new ReliableOrderedReceiverChannel(socket, remote, config, sendController.LibSender, behaviour) { ID = id };
        }

        public void Heartbeat()
        {
            for (int i = 0; i < Size; i++)
            {
                channels[i].Heartbeat();
            }
        }

        public void ReceivedPacket(RawSocket sender, PacketReceiveEventArgs e)
        {
            readHelper.PositionBits = 0;
            LibHeader header = new LibHeader(readHelper);
            IncommingMsg msg = this[header.Channel].CreateMessage(header);

            for (int i = 0; i < Size; i++)
            {
                ReceiverChannelBase cur = channels[i];

                if (cur.ID == msg.Header.Channel)
                {
                    cur.EnqueueMessage(msg);
                    return;
                }
            }

            Log.Warning(nameof(ReceiverController), $"Message received on unknown channel({msg.Header.Channel}), message dropped");
        }

        public override string ToString()
        {
            return $"Receiver controller with {Size} channels";
        }

        private void CheckNewChannel(int id)
        {
            LoggedException.RaiseIf(id <= 0 || id > 14, nameof(ReceiverController), "Id must be between zero and 15");
            LoggedException.RaiseIf(Size > 15, nameof(ReceiverController), "Maximum of 15 channels reached");
        }
    }
}