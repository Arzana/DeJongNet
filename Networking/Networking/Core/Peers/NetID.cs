namespace DeJong.Networking.Core.Peers
{
    using System;

#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class NetID : IEquatable<NetID>
    {
        public long ID { get; private set; }
        private readonly string hexId;

        public static bool operator ==(NetID left, NetID right) { return left?.ID == right?.ID; }
        public static bool operator !=(NetID left, NetID right) { return left?.ID != right?.ID; }

        public static bool operator ==(NetID left, long right) { return left?.ID == right; }
        public static bool operator !=(NetID left, long right) { return left?.ID != right; }

        public static bool operator ==(long left, NetID right) { return right?.ID == left; }
        public static bool operator !=(long left, NetID right) { return right?.ID == left; }

        public static bool operator ==(NetID left, string right) { return left?.hexId == right; }
        public static bool operator !=(NetID left, string right) { return left?.hexId == right; }

        public static bool operator ==(string left, NetID right) { return right?.hexId == left; }
        public static bool operator !=(string left, NetID right) { return right?.hexId != left; }

        internal NetID(long id)
        {
            ID = id;
            hexId = NetUtils.ToHexString(id);
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(NetID) ? Equals((NetID)obj) : false;
        }

        public bool Equals(NetID other)
        {
            return other.ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return hexId;
        }
    }
}