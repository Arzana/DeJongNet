namespace DeJong.Networking.Core
{
    using System;
    using System.Diagnostics;
    using static Utilities.Utils;

    [DebuggerDisplay("{ToString()}")]
    internal struct Pair<A, B> : IEquatable<Pair<A, B>>
    {
        public A Item1;
        public B Item2;

        public static bool operator ==(Pair<A, B> left, Pair<A, B> right) { return left.Equals(right); }
        public static bool operator !=(Pair<A, B> left, Pair<A, B> right) { return !left.Equals(right); }

        public Pair(A item1, B item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public bool Equals(Pair<A, B> other)
        {
            bool result1 = false;
            if (Item1 == null && other.Item1 == null) result1 = true;
            else if (Item1.Equals(other.Item1)) result1 = true;

            bool result2 = false;
            if (Item2 == null && other.Item2 == null) result2 = true;
            else if (Item2.Equals(other.Item2)) result2 = true;

            return result1 && result2;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(Pair<A, B>) ? Equals((Pair<A, B>)obj) : false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = HASH_BASE;
                hash *= ComputeHash(hash, Item1);
                hash *= ComputeHash(hash, Item2);
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{{A={Item1}, B={Item2}}}";
        }
    }
}