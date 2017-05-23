namespace DeJong.Networking.Core.Channels.Receiver
{
    using Messages;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Utilities.Threading;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal abstract class ReceiverChannelBase : ChannelBase<IncommingMsg>
    {
        public IPEndPoint Sender { get; set; }
        public virtual bool HasMessages { get { return received.Count > 0; } }

        protected ThreadSafeQueue<IncommingMsg> received;

        private Dictionary<int, List<KeyValuePair<FragmentHeader, IncommingMsg>>> receivedFragments;
        private ThreadSafeQueue<IncommingMsg> receivedPackets;

        protected ReceiverChannelBase(RawSocket socket, IPEndPoint remote, PeerConfig config)
            : base(socket, config)
        {
            Sender = remote;
            receivedFragments = new Dictionary<int, List<KeyValuePair<FragmentHeader, IncommingMsg>>>();
            receivedPackets = new ThreadSafeQueue<IncommingMsg>();
            received = new ThreadSafeQueue<IncommingMsg>();
        }

        public IncommingMsg CreateMessage(LibHeader header)
        {
            int byteSize = (header.PacketSize + 7) >> 3;
            byte[] buffer = cache.Get(byteSize);
            Array.Copy(socket.ReceiveBuffer, Constants.HEADER_BYTE_SIZE, buffer, 0, byteSize);
            return new IncommingMsg(buffer) { Header = header };
        }

        public override void Heartbeat()
        {
            while (receivedPackets.Count > 0)
            {
                ProcessMsg(receivedPackets.Dequeue());
            }
        }

        public void EnqueueMessage(IncommingMsg msg)
        {
            receivedPackets.Enqueue(msg);
        }

        public virtual IncommingMsg DequeueMessage()
        {
            return received.Dequeue();
        }

        public override string ToString()
        {
            return $"Receiver channel {ID}";
        }

        protected virtual void ReceiveMsg(IncommingMsg msg)
        {
            received.Enqueue(msg);
        }

        private void ProcessMsg(IncommingMsg msg)
        {
            if (msg.Header.Fragment) ProcessFragment(msg);
            else ReceiveMsg(msg);
        }

        private void ProcessFragment(IncommingMsg fragment)
        {
            FragmentHeader header = new FragmentHeader(fragment);

            if (receivedFragments.ContainsKey(header.Group))
            {
                List<KeyValuePair<FragmentHeader, IncommingMsg>> group = receivedFragments[header.Group];
                int totalBytes = header.FragmentSize;

                for (int i = 0; i < group.Count; i++)
                {
                    totalBytes += group[i].Key.FragmentSize;
                }

                if (totalBytes >= (header.TotalBits + 7) >> 3)
                {
                    IncommingMsg msg = CreateMessage(LibHeader.Empty);
                    group.Sort((f, s) => 
                    {
                        if (f.Key.FragmentNum < s.Key.FragmentNum) return -1;
                        else if (f.Key.FragmentNum > s.Key.FragmentNum) return 1;
                        return 0;
                    });

                    for (int i = 0; i < group.Count; i++)
                    {
                        group[i].Value.CopyData(msg);
                    }

                    receivedFragments.Remove(header.Group);
                    ReceiveMsg(msg);
                }
                else receivedFragments[header.Group].Add(new KeyValuePair<FragmentHeader, IncommingMsg>(header, fragment));
            }
            else receivedFragments.Add(header.Group, new List<KeyValuePair<FragmentHeader, IncommingMsg>> { new KeyValuePair<FragmentHeader, IncommingMsg>(header, fragment) });
        }
    }
}