namespace Mentula.Networking.Core
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Defines a container for flag bits within a dynamic amount of bytes.
    /// </summary>
#if !DEBUG
    [DebuggerStepThrough]
#endif
    [DebuggerDisplay("{ToString()}")]
    public struct BitFlags : IEquatable<BitFlags>
    {
        /// <summary>
        /// The capacity in bits of the container.
        /// </summary>
        public int Capacity { get { return capacity; } }
        /// <summary>
        /// The amount of bit set to <see langword="true"/>.
        /// </summary>
        public int Count { get { return bitsSet; } }
        /// <summary>
        /// Whether all bits are set to <see langword="false"/>.
        /// </summary>
        public bool IsEmpty { get { return bitsSet == 0; } }
        /// <summary>
        /// Whether all bits are set to <see langword="true"/>.
        /// </summary>
        public bool IsFull { get { return bitsSet >= capacity; } }

        /// <summary>
        /// Gets a empty instance of the <see cref="BitFlags"/> struct (capacity 0).
        /// </summary>
        public static readonly BitFlags Empty = new BitFlags(0);

        private readonly byte[] data;
        private readonly int capacity;
        private int bitsSet;

        /// <summary>
        /// Returns whether two instances of the <see cref="BitFlags"/> struct are equal.
        /// </summary>
        /// <param name="left"> The first parameter to check. </param>
        /// <param name="right"> The second parameter to check. </param>
        /// <returns> <see langword="true"/> if the structs are equal; otherwise, <see langword="false"/>. </returns>
        public static bool operator ==(BitFlags left, BitFlags right) { return left.Equals(right); }
        /// <summary>
        /// Returns whether two instances of the <see cref="BitFlags"/> struct are different.
        /// </summary>
        /// <param name="left"> The first parameter to check. </param>
        /// <param name="right"> The second parameter to check. </param>
        /// <returns> <see langword="true"/> if the structs are different; otherwise, <see langword="false"/>. </returns>
        public static bool operator !=(BitFlags left, BitFlags right) { return !left.Equals(right); }

        /// <summary>
        /// Gets or sets a bit at a specified index.
        /// </summary>
        /// <param name="bitIndex"> The index of the bit. </param>
        /// <returns> The current value of the bit. </returns>
        /// <exception cref="NetException"> <paramref name="bitIndex"/> was smaller than zero or greater than the capacity. </exception>
        [IndexerName("Bit")]
        public bool this[int bitIndex]
        {
            get { return Get(bitIndex); }
            set { Set(bitIndex, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitFlags"/> struct with a specified capacity.
        /// </summary>
        /// <param name="capacity"> The maximum capacity in bits. </param>
        public BitFlags(int capacity)
        {
            data = new byte[(capacity + 7) / 8];
            this.capacity = capacity;
            bitsSet = 0;
        }

        /// <summary>
        /// Gets a bit at a specified index.
        /// </summary>
        /// <param name="bitIndex"> The index of the bit. </param>
        /// <returns> The current value of the bit. </returns>
        /// <exception cref="NetException"> <paramref name="bitIndex"/> was smaller than zero or greater than the capacity. </exception>
        public bool Get(int bitIndex)
        {
            NetException.RaiseIf(bitIndex < 0 || bitIndex >= capacity, "Attempting to read outside the designated buffer!");
            return (data[bitIndex / 8] & (1 << (bitIndex % 8))) != 0;
        }

        /// <summary>
        /// Sets a bit at a specified index.
        /// </summary>
        /// <param name="bitIndex"> The index of the bit. </param>
        /// <param name="value"> The value to assign to the specified bit. </param>
        /// <exception cref="NetException"> <paramref name="bitIndex"/> was smaller than zero or greater than the capacity. </exception>
        public void Set(int bitIndex, bool value)
        {
            NetException.RaiseIf(bitIndex < 0 || bitIndex >= capacity, "Attempting to write outside the designated buffer!");

            int byteIndex = bitIndex / 8;
            bitIndex %= 8;

            if (value)
            {
                if ((data[byteIndex] & (1 << bitIndex)) == 0) ++bitsSet;
                data[byteIndex] |= (byte)(1 << bitIndex);
            }
            else
            {
                if ((data[byteIndex] & (1 << bitIndex)) != 0) --bitsSet;
                data[byteIndex] &= (byte)(~(1 << bitIndex));
            }
        }

        /// <summary>
        /// Attempts to clear the data buffer.
        /// </summary>
        /// <exception cref="NetException"> The buffer clear failed. </exception>
        public void Clear()
        {
            Array.Clear(data, 0, data.Length);
            bitsSet = 0;
            NetException.RaiseIf(!IsEmpty, "Buffer clear failed!");
        }

        /// <summary>
        /// Returns whether this instance of the <see cref="BitFlags"/> struct is equal to a specified instance of the <see cref="BitFlags"/> struct.
        /// </summary>
        /// <param name="other"> The instance to evaluate. </param>
        /// <returns> <see langword="true"/> if the structs are equal; otherwise, <see langword="false"/>. </returns>
        public bool Equals(BitFlags other)
        {
            if (other.capacity != capacity) return false;

            for (int i = 0; i < data.Length; i++)
            {
                if (other.data[i] != data[i]) return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(BitFlags) ? Equals((BitFlags)obj) : false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(data.Length + 2);

            sb.Append('{');
            for (int i = 0; i < capacity; i++) sb.Append(Get(capacity - i - 1) ? '1' : '0');
            sb.Append('}');

            return sb.ToString();
        }
    }
}