using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

// C# Translation of GoldParser, by Marcus Klimstra <klimstra@home.nl>.
// Based on GOLDParser by Devin Cook <http://www.devincook.com/goldparser>.
namespace Alachisoft.NosDB.Common.Queries.Parser
{
	/// Thrown by the parser when an error occures.
	/// Specialized exceptions may be added at a later time. 
    [Serializable]
    public class ParserException : Exception, ISerializable
	{
		/// Creates a new ParserException with the specified error string.
		public ParserException(String p_error)
		:	base(p_error)
		{
		}
		
		/// Creates a new ParserException with the specified error string and inner-exception.
		internal ParserException(String p_error, Exception p_exception)
		:	base(p_error, p_exception)
		{
		}

        /// <summary> 
        /// overloaded constructor, manual serialization. 
        /// </summary>
        protected ParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// manual serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
	}
}
