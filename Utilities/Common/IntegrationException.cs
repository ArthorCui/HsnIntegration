using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Utilities
{
    [Serializable()]
    public class IntegrationException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationException"/> class.
        /// </summary>
        public IntegrationException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public IntegrationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"></see> is zero (0). </exception>
        /// <exception cref="T:System.ArgumentNullException">The info parameter is null. </exception>
        protected IntegrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public IntegrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
