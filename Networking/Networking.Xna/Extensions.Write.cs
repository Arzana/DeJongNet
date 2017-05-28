namespace DeJong.Networking.Xna
{
    using Core.Messages;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics.PackedVector;

    public static partial class Extensions
    {
        /// <summary>
        /// Writes a half precision single to the buffer as 16 bits and increases the lenght if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void WriteHalf(this WriteableBuffer msg, float value)
        {
            msg.Write(new HalfSingle(value).PackedValue);
        }

        /// <summary>
        /// Writes a point to the buffer as 64 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void Write(this WriteableBuffer msg, Point value)
        {
            msg.Write(value.X);
            msg.Write(value.Y);
        }

        /// <summary>
        /// Writes a vector2 to the buffer as 64 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void Write(this WriteableBuffer msg, Vector2 value)
        {
            msg.Write(value.X);
            msg.Write(value.Y);
        }

        /// <summary>
        /// Write a vector3 to the buffer as 96 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void Write(this WriteableBuffer msg, Vector3 value)
        {
            msg.Write(value.X);
            msg.Write(value.Y);
            msg.Write(value.Z);
        }

        /// <summary>
        /// Writes a vector4 to the buffer as 128 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void Write(this WriteableBuffer msg, Vector4 value)
        {
            msg.Write(value.X);
            msg.Write(value.Y);
            msg.Write(value.Z);
            msg.Write(value.W);
        }

        /// <summary>
        /// Writes a half precision vector2 to the buffer as 32 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void WriteHalf(this WriteableBuffer msg, Vector2 value)
        {
            msg.WriteHalf(value.X);
            msg.WriteHalf(value.Y);
        }

        /// <summary>
        /// Writes a half precision vector3 to the buffer as 48 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void WriteHalf(this WriteableBuffer msg, Vector3 value)
        {
            msg.WriteHalf(value.X);
            msg.WriteHalf(value.Y);
            msg.WriteHalf(value.Z);
        }

        /// <summary>
        /// Writes a half precision vector4 to the buffer as 64 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void WriteHalf(this WriteableBuffer msg, Vector4 value)
        {
            msg.WriteHalf(value.X);
            msg.WriteHalf(value.Y);
            msg.WriteHalf(value.Z);
            msg.WriteHalf(value.W);
        }

        /// <summary>
        /// Writes a quaternion to the buffer as 128 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void Write(this WriteableBuffer msg, Quaternion value)
        {
            msg.Write(value.X);
            msg.Write(value.Y);
            msg.Write(value.Z);
            msg.Write(value.W);
        }

        /// <summary>
        /// Writes a matrix to the buffer as 224 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void Write(this WriteableBuffer msg, Matrix value)
        {
            msg.Write(Quaternion.CreateFromRotationMatrix(value));
            msg.Write(value.M41);
            msg.Write(value.M42);
            msg.Write(value.M43);
        }

        /// <summary>
        /// Writes a bounding sphere to the buffer as 128 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void Write(this WriteableBuffer msg, BoundingSphere value)
        {
            msg.Write(value.Center);
            msg.Write(value.Radius);
        }

        /// <summary>
        /// Writes a bounding box to the buffer as 192 bits and increases the length if needed.
        /// </summary>
        /// <param name="msg"> The buffer to write to. </param>
        /// <param name="value"> The value to write. </param>
        public static void Write(this WriteableBuffer msg, BoundingBox value)
        {
            msg.Write(value.Min);
            msg.Write(value.Max);
        }
    }
}