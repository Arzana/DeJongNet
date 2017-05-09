namespace DeJong.Networking.Core.Messages
{
    using System;
    using System.Text;
    using Utilities.Core;

    /// <summary>
    /// Defines a fixed size vector of booleans.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class NetFlags
    {
        /// <summary>
        /// Gets the number of bits that can be stored in this vector.
        /// </summary>
        public int Capacity { get; private set; }
        /// <summary>
        /// Gets the number of bits that are set to true.
        /// </summary>
        public int Count { get; private set; }

        private readonly byte[] data;

        /// <summary>
        /// Gets or sets a boolean at a specified index.
        /// </summary>
        /// <param name="index"> The specific index in bits. </param>
        /// <returns> The value of the boolean at the specified index. </returns>
        /// <exception cref="LoggedException"> The index was out of bounds. </exception>
        public bool this[int index]
        {
            get { return Get(index); }
            set { Set(index, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetFlags"/> class with a fixed capacity.
        /// </summary>
        /// <param name="capacity"> The fixed capacity of the vector. </param>
        /// <exception cref="LoggedException"> The capacity was lower than one. </exception>
        public NetFlags(int capacity)
        {
            LoggedException.RaiseIf(capacity < 1, nameof(NetFlags), "Capacity must be at least 1");
            Capacity = capacity;
            data = new byte[(capacity + 7) / 8];
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index"> The specified index. </param>
        /// <returns> The value at the specified index. </returns>
        /// <exception cref="LoggedException"> The index was out of bounds. </exception>
        public bool Get(int index)
        {
            CheckIndex(index);
            return (data[index / 8] & (1 << (index % 8))) != 0;
        }

        /// <summary>
        /// Gets the index of the first boolean set to true.
        /// </summary>
        /// <returns> The index of the first boolean set to true, otherwise; -1. </returns>
        public int GetFirstSetIndex()
        {
            int byteIndex = 0;
            byte container = data[0];

            while (container == 0)
            {
                if (++byteIndex >= data.Length) return -1;
                container = data[byteIndex];
            }

            int bitIndex = 0;
            while (((container >> bitIndex) & 1) == 0) ++bitIndex;

            return (byteIndex << 3) + bitIndex;
        }

        /// <summary>
        /// Sets a boolean at the specified index.
        /// </summary>
        /// <param name="index"> The specified index. </param>
        /// <param name="value"> The new value of the boolean. </param>
        /// <exception cref="LoggedException"> The index was out of bounds. </exception>
        public void Set(int index, bool value)
        {
            CheckIndex(index);

            if (value)
            {
                if (!Get(index)) ++Count;
                data[index / 8] |= (byte)(1 << (index % 8));
            }
            else
            {
                if (Get(index)) --Count;
                data[index / 8] |= (byte)(~(1 << (index % 8)));
            }
        }

        /// <summary>
        /// Clears the vector.
        /// </summary>
        public void Clear()
        {
            Array.Clear(data, 0, Capacity);
            Count = 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Capacity + 2).Append('[');
            for (int i = 0; i < Capacity; i++) sb.Append(Get(Capacity - i - 1) ? '1' : '0');
            return sb.Append(']').ToString();
        }

        private void CheckIndex(int requestedIndex)
        {
            LoggedException.RaiseIf(requestedIndex < 0 || requestedIndex >= Capacity, nameof(NetFlags), "Index must be higher than zero and lower than the capacity");
        }
    }
}