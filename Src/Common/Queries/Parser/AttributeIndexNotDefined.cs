using System;
using System.Runtime.Serialization;

namespace Alachisoft.NosDB.Common.Queries.Parser
{
    [Serializable]
    public class AttributeIndexNotDefined : Exception, ISerializable
    {
        public AttributeIndexNotDefined(String error)
            : base(error)
        {
        }

        public AttributeIndexNotDefined(String error, Exception exception)
            : base(error, exception)
        {
        }

        public AttributeIndexNotDefined(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}