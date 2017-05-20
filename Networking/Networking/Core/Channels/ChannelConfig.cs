namespace DeJong.Networking.Core.Channels
{
    using Messages;
    using System;

    public struct ChannelConfig : IEquatable<ChannelConfig>
    {
        public readonly int Id;
        public readonly DeliveryMethod Type;
        public readonly OrderChannelBehaviour Behavior;

        public ChannelConfig(int id)
        {
            Id = id;
            Type = DeliveryMethod.Unknown;
            Behavior = OrderChannelBehaviour.None;
        }

        public ChannelConfig(int id, DeliveryMethod type, OrderChannelBehaviour behaviour)
        {
            Id = id;
            Type = type;
            Behavior = behaviour;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(ChannelConfig) ? Equals((ChannelConfig)obj) : false;
        }

        public bool Equals(ChannelConfig other)
        {
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Id} : {Type}";
        }
    }
}
