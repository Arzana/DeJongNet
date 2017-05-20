namespace DeJong.Networking.Core.Channels
{
    using Messages;
    using Receiver;
    using Sender;
    using System;
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
        private UnreliableSenderChannel ackSender;

        public ReceiverChannelBase this[int index]
        {
            get
            {
                LoggedException.RaiseIf(index >= Size || index < 0, nameof(ReceiverController), "index ot of range");
                return channels[index];
            }
        }

        public ReceiverController(IPEndPoint ep, UnreliableSenderChannel libSender)
        {
            remote = ep;
            this.ep = remote;
            channels = new ReceiverChannelBase[15];
            ackSender = libSender;

            channels[Size++] = new UnreliableReceiverChannel(ep);
        }

        public void AddUnreliable(int id)
        {
            CheckNewChannel(id);
            channels[Size++] = new UnreliableReceiverChannel(remote) { ID = id };
        }

        public void AddOrdered(int id, OrderChannelBehaviour behaviour)
        {
            CheckNewChannel(id);
            channels[Size++] = new OrderedReceiverChannel(remote, behaviour) { ID = id };
        }

        public void AddReliable(int id)
        {
            CheckNewChannel(id);
            channels[Size++] = new ReliableReceiverChannel(remote, ackSender) { ID = id };
        }

        public void AddReliableOrdered(int id, OrderChannelBehaviour behaviour)
        {
            CheckNewChannel(id);
            channels[Size++] = new ReliableOrderedReceiverChannel(remote, ackSender, behaviour) { ID = id };
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
            byte[] data = new byte[e.PacketSize];
            Array.Copy(sender.ReceiveBuffer, 0, data, 0, e.PacketSize);
            IncommingMsg msg = new IncommingMsg(data);

            for (int i = 0; i < Size; i++)
            {
                if (channels[i].ID == msg.Header.Channel)
                {
                    channels[i].EnqueueMessage(msg);
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