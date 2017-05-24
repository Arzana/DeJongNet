namespace DeJong.Networking.Core.Peers
{
    using System;

    /// <summary>
    /// Represents a unique indentifier.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class NetID : IEquatable<NetID>
    {
        /// <summary>
        /// The unique indentifier.
        /// </summary>
        public long ID { get; private set; }
        private readonly string hexId;

        internal static readonly NetID Unknown = new NetID();

        /// <summary>
        /// Returns whether two <see cref="NetID"/> are equal.
        /// </summary>
        /// <param name="left"> The first <see cref="NetID"/>. </param>
        /// <param name="right"> The second <see cref="NetID"/>. </param>
        /// <returns> <see langword="true"/> if the two values are equal, otherwise; <see langword="false"/>. </returns>
        public static bool operator ==(NetID left, NetID right) { return left?.ID == right?.ID; }
        /// <summary>
        /// Returns whether two <see cref="NetID"/> are differend.
        /// </summary>
        /// <param name="left"> The first <see cref="NetID"/>. </param>
        /// <param name="right"> The second <see cref="NetID"/>. </param>
        /// <returns> <see langword="true"/> if the two values are differend, otherwise; <see langword="false"/>. </returns>
        public static bool operator !=(NetID left, NetID right) { return left?.ID != right?.ID; }

        /// <summary>
        /// Returns whether a <see cref="NetID"/> and a indentifier are equal.
        /// </summary>
        /// <param name="left"> The specified <see cref="NetID"/>. </param>
        /// <param name="right"> The specified indentifier. </param>
        /// <returns> <see langword="true"/> if the two values are equal, otherwise; <see langword="false"/>. </returns>
        public static bool operator ==(NetID left, long right) { return left?.ID == right; }
        /// <summary>
        /// Retruns whether a <see cref="NetID"/> and a indentifier are differend.
        /// </summary>
        /// <param name="left"> The specified <see cref="NetID"/>. </param>
        /// <param name="right"> The specified indentifier. </param>
        /// <returns> <see langword="true"/> if the two values are differend, otherwise; <see langword="false"/>. </returns>
        public static bool operator !=(NetID left, long right) { return left?.ID != right; }

        /// <summary>
        /// Returns whether a <see cref="NetID"/> and a indentifier are equal.
        /// </summary>
        /// <param name="left"> The specified indentifier. </param>
        /// <param name="right"> The specified <see cref="NetID"/>. </param>
        /// <returns> <see langword="true"/> if the two values are equal, otherwise; <see langword="false"/>. </returns>
        public static bool operator ==(long left, NetID right) { return right?.ID == left; }
        /// <summary>
        /// Retruns whether a <see cref="NetID"/> and a indentifier are differend.
        /// </summary>
        /// <param name="left"> The specified indentifier. </param>
        /// <param name="right"> The specified <see cref="NetID"/>. </param>
        /// <returns> <see langword="true"/> if the two values are differend, otherwise; <see langword="false"/>. </returns>
        public static bool operator !=(long left, NetID right) { return right?.ID == left; }

        /// <summary>
        /// Returns whether a <see cref="NetID"/> and a hex indentifier are equal.
        /// </summary>
        /// <param name="left"> The specified <see cref="NetID"/>. </param>
        /// <param name="right"> The specified hex indentifier. </param>
        /// <returns> <see langword="true"/> if the two values are equal, otherwise; <see langword="false"/>. </returns>
        public static bool operator ==(NetID left, string right) { return left?.hexId == right; }
        /// <summary>
        /// Returns whether a <see cref="NetID"/> and a hex indentifier are differend.
        /// </summary>
        /// <param name="left"> The specified <see cref="NetID"/>. </param>
        /// <param name="right"> The specified hex indentifier. </param>
        /// <returns> <see langword="true"/> if the two values are differend, otherwise; <see langword="false"/>. </returns>
        public static bool operator !=(NetID left, string right) { return left?.hexId == right; }

        /// <summary>
        /// Returns whether a <see cref="NetID"/> and a hex indentifier are equal.
        /// </summary>
        /// <param name="left"> The specified hex indentifier. </param>
        /// <param name="right"> The specified <see cref="NetID"/>. </param>
        /// <returns> <see langword="true"/> if the two values are equal, otherwise; <see langword="false"/>. </returns>
        public static bool operator ==(string left, NetID right) { return right?.hexId == left; }
        /// <summary>
        /// Returns whether a <see cref="NetID"/> and a hex indentifier are differend.
        /// </summary>
        /// <param name="left"> The specified hex indentifier. </param>
        /// <param name="right"> The specified <see cref="NetID"/>. </param>
        /// <returns> <see langword="true"/> if the two values are differend, otherwise; <see langword="false"/>. </returns>
        public static bool operator !=(string left, NetID right) { return right?.hexId != left; }

        internal NetID(long id)
        {
            ID = id;
            hexId = NetUtils.ToHexString(id);
        }

        private NetID()
        {
            ID = 0;
            hexId = "Unknown";
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(NetID) ? Equals((NetID)obj) : false;
        }

        /// <summary>
        /// Returns whether a <see cref="NetID"/> is equal to this <see cref="NetID"/>.
        /// </summary>
        /// <param name="other"> The specified <see cref="NetID"/> to check. </param>
        /// <returns> <see langword="true"/> if the two values are equal, otherwise; <see langword="false"/>. </returns>
        public bool Equals(NetID other)
        {
            return other.ID == ID;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return hexId;
        }
    }
}