using System;
using System.Runtime.Serialization;

namespace Alachisoft.NosDB.Common.Queries.Parser
{
    [Serializable]
    public class TypeIndexNotDefined : Exception, ISerializable
    {
        public TypeIndexNotDefined(String error) : base(error)
        {
        }

        public TypeIndexNotDefined(String error, Exception exception) : base(error, exception)
        {
        }

        public TypeIndexNotDefined(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}