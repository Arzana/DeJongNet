namespace Mentula.Networking.Core
{
    using Utilities.Core;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an error that occured during networking.
    /// </summary>
#if !DEBUG
    [System.Diagnostics.DebuggerStepThrough]
#endif
    [Serializable]
    public class NetException : LoggedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetException"/> class with default parameters.
        /// </summary>
        public NetException()
            : base("Networking", "An unhandled exception occured during networking!")
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetException"/> class with a specified message.
        /// </summary>
        /// <param name="message"> The message to display. </param>
        public NetException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetException"/> class with a specified message an an inner exception.
        /// </summary>
        /// <param name="message"> The message to display. </param>
        /// <param name="inner"> The exception that caused this exception. </param>
        public NetException(string message, NetException inner)
            : base("Networking", message, inner)
        { }

        /// <inheritdoc/>
        protected NetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Throws a <see cref="NetException"/>.
        /// </summary>
        public static void Raise()
        {
            throw new NetException();
        }

        /// <summary>
        /// Throws a <see cref="NetException"/> with a specified message.
        /// </summary>
        /// <param name="message"> The message to display. </param>
        new public static void Raise(string message)
        {
            throw new NetException(message);
        }

        /// <summary>
        /// Throws a <see cref="NetException"/> with a specified message and an inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public static void Raise(string message, NetException inner)
        {
            throw new NetException(message, inner);
        }

        /// <summary>
        /// Raises a empty <see cref="NetException"/> if the condition is <see langword="true"/>.
        /// </summary>
        /// <param name="condition"> The condition to evaluate. </param>
        public static void RaiseIf(bool condition)
        {
            if (condition) Raise();
        }

        /// <summary>
        /// Raises a <see cref="NetException"/> with a specified message if the condition is <see langword="true"/>.
        /// </summary>
        /// <param name="condition"> The condition to evaluate. </param>
        /// <param name="message"> The message to display. </param>
        new public static void RaiseIf(bool condition, string message)
        {
            if (condition) Raise(message);
        }

        /// <summary>
        /// Raises a <see cref="NetException"/> with a specified message and an inner exception if the condition is <see langword="true"/>.
        /// </summary>
        /// <param name="condition"> The condition to evaluate. </param>
        /// <param name="message"> The message to display. </param>
        /// <param name="inner"> The exception that caused this exception. </param>
        public static void RaiseIf(bool condition, string message, NetException inner)
        {
            if (condition) Raise(message, inner);
        }
    }
}
