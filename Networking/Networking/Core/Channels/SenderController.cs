namespace DeJong.Networking.Core.Channels
{
    using Sender;
    using System.Net;
    using Utilities.Core;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class SenderController
    {
        public LibSenderChannel LibSender { get { return (LibSenderChannel)this[0]; } }

        private RawSocket socket;
        private IPEndPoint remote;
        private SenderChannelBase[] channels;
        private int size;
        private PeerConfig config;

        public SenderChannelBase this[int id]
        {
            get
            {
                for (int i = 0; i < size; i++)
                {
                    if (channels[i].ID == id) return channels[i];
                }

                LoggedException.Raise(nameof(ReceiverController), $"Cannot find channel with id {id}");
                return null;
            }
        }

        public SenderController(RawSocket socket, IPEndPoint ep, PeerConfig config)
        {
            this.socket = socket;
            remote = ep;
            channels = new SenderChannelBase[15];
            this.config = config;

            channels[size++] = new LibSenderChannel(socket, ep, config);
        }

        public void AddUnreliable(int id)
        {
            CheckNewChannel(id);
            channels[size++] = new UnreliableSenderChannel(socket, remote, config) { ID = id };
        }

        public void AddOrdered(int id)
        {
            CheckNewChannel(id);
            channels[size++] = new OrderedSenderChannel(socket, remote, config) { ID = id };
        }

        public void AddReliable(int id)
        {
            CheckNewChannel(id);
            channels[size++] = new ReliableSenderChannel(socket, remote, config);
        }

        public void AddReliableOrdered(int id)
        {
            CheckNewChannel(id);
            channels[size++] = new ReliableOrderedSenderChannel(socket, remote, config);
        }

        public void HeartBeat()
        {
            for (int i = 0; i < size; i++)
            {
                channels[i].Heartbeat();
            }
        }

        public override string ToString()
        {
            return $"Sender controller with {size} channels";
        }

        private void CheckNewChannel(int id)
        {
            LoggedException.RaiseIf(id <= 0 || id > 14, nameof(ReceiverController), "Id must be between zero and 15");
            LoggedException.RaiseIf(size > 15, nameof(ReceiverController), "Maximum of 15 channels reached");
        }
    }
}