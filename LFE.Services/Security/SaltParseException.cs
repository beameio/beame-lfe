using System;
using System.Runtime.Serialization;

namespace LFE.Application.Services.Security
{
    /// <summary>Exception for signalling parse errors. </summary>
    internal class SaltParseException : ApplicationException
    {
        /// <summary>Default constructor. </summary>
        public SaltParseException()
        {
        }

        /// <summary>Initializes a new instance of <see cref="SaltParseException"/>.</summary>
        /// <param name="message">The message.</param>
        public SaltParseException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of <see cref="SaltParseException"/>.</summary>
        /// <param name="message">       The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SaltParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of <see cref="SaltParseException"/>.</summary>
        /// <param name="info">   The information.</param>
        /// <param name="context">The context.</param>
        protected SaltParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
