namespace DeJong.Networking.Xna
{
    using Core.Messages;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics.PackedVector;

    /// <summary>
    /// Contains extension functions for reading and writing general game related values to a networking message.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Reads the next 16 bits from the buffer as a half precision float and increases the position by 16.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 16 bits as a half precision float. </returns>
        public static float ReadHalfSingle(this ReadableBuffer msg)
        {
            HalfSingle hs = new HalfSingle { PackedValue = msg.ReadUInt16() };
            return hs.ToSingle();
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a point and increases the position by 64.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 64 bits as a point. </returns>
        public static Point ReadPoint(this ReadableBuffer msg)
        {
            int x = msg.ReadInt32();
            int y = msg.ReadInt32();
            return new Point(x, y);
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a vector2 and increases the position by 64.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 64 bits as a vector2. </returns>
        public static Vector2 ReadVector2(this ReadableBuffer msg)
        {
            float x = msg.ReadSingle();
            float y = msg.ReadSingle();
            return new Vector2(x, y);
        }

        /// <summary>
        /// Reads the next 96 bits from the buffer as a vector3 and increases the position by 96.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 96 bits as a vector3. </returns>
        public static Vector3 ReadVector3(this ReadableBuffer msg)
        {
            float x = msg.ReadSingle();
            float y = msg.ReadSingle();
            float z = msg.ReadSingle();
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Reads the next 128 bits from the buffer as a vector4 and increases the position by 128.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 128 bits as a vector4. </returns>
        public static Vector4 ReadVector4(this ReadableBuffer msg)
        {
            float x = msg.ReadSingle();
            float y = msg.ReadSingle();
            float z = msg.ReadSingle();
            float w = msg.ReadSingle();
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Reads the next 32 bits from the buffer as a vector2 and increases the position by 32.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 32 bits as a half precision vector2. </returns>
        public static Vector2 ReadHalfVector2(this ReadableBuffer msg)
        {
            float x = msg.ReadHalfSingle();
            float y = msg.ReadHalfSingle();
            return new Vector2(x, y);
        }

        /// <summary>
        /// Reads the next 48 bits from the buffer as a vector3 and increases the position by 48.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 48 bits as a half precision vector3. </returns>
        public static Vector3 ReadHalfVector3(this ReadableBuffer msg)
        {
            float x = msg.ReadHalfSingle();
            float y = msg.ReadHalfSingle();
            float z = msg.ReadHalfSingle();
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Reads the next 64 bits from the buffer as a vector4 and increases the position by 64.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 64 bits as a half precision vector4. </returns>
        public static Vector4 ReadHalfVector4(this ReadableBuffer msg)
        {
            float x = msg.ReadHalfSingle();
            float y = msg.ReadHalfSingle();
            float z = msg.ReadHalfSingle();
            float w = msg.ReadHalfSingle();
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Reads the next 128 bits from the buffer as a quaternion and increases the position by 128.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 128 bits as a quaternion. </returns>
        public static Quaternion ReadQuaternion(this ReadableBuffer msg)
        {
            float x = msg.ReadSingle();
            float y = msg.ReadSingle();
            float z = msg.ReadSingle();
            float w = msg.ReadSingle();
            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Reads the next 224 bits from the buffer as a matrix and increases the position by 224.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 224 bits as a matrix. </returns>
        public static Matrix ReadMatrix(this ReadableBuffer msg)
        {
            Quaternion rot = msg.ReadQuaternion();
            Matrix result = Matrix.CreateFromQuaternion(rot);
            result.M41 = msg.ReadSingle();
            result.M42 = msg.ReadSingle();
            result.M43 = msg.ReadSingle();
            return result;
        }

        /// <summary>
        /// Reads the next 128 bits from the buffer as a bounding sphere and increases the position by 128.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 128 bits as a bounding sphere. </returns>
        public static BoundingSphere ReadSphere(this ReadableBuffer msg)
        {
            Vector3 center = msg.ReadVector3();
            float radius = msg.ReadSingle();
            return new BoundingSphere(center, radius);
        }

        /// <summary>
        /// Reads the next 192 bits from the buffer as a bounding box and increases the position by 192.
        /// </summary>
        /// <param name="msg"> The buffer to read from. </param>
        /// <returns> The next 192 bits as a bounding box. </returns>
        public static BoundingBox ReadBox(this ReadableBuffer msg)
        {
            Vector3 min = msg.ReadVector3();
            Vector3 max = msg.ReadVector3();
            return new BoundingBox(min, max);
        }
    }
}