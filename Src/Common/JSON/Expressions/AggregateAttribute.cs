using System;
using Alachisoft.NoSQL.Common.Enum;
using Alachisoft.NoSQL.Common.Queries;
using Alachisoft.NoSQL.Common.Server.Engine;

namespace Alachisoft.NoSQL.Common.JSON.Expressions
{
    public class AggregateAttribute : Attribute
    {
        public AggregateAttribute(string aggregation) : base(aggregation) { }
        
        public override IJsonValue Evaluate(IJSONDocument document)
        {
            JSONResult result = (JSONResult) document;
            if (result != null)
            {
                IAggregation aggregation = result.GetAggregation(Name);
                if (aggregation != null)
                {
                    FieldDataType dataType = JSONType.GetJSONType(aggregation.Value);
                    switch (dataType)
                    {
                        case FieldDataType.Number:
                            return new NumberJsonValue(aggregation.Value);
                        case FieldDataType.String:
                            return new StringJsonValue((string) aggregation.Value);
                        case FieldDataType.Null:
                            return new NullValue();
                        case FieldDataType.Object:
                            return new ObjectJsonValue((IJSONDocument) aggregation.Value);
                        case FieldDataType.DateTime:
                            return new DateTimeJsonValue((DateTime)aggregation.Value);
                        case FieldDataType.Bool:
                            return new BooleanJsonValue((bool) aggregation.Value);
                        case FieldDataType.Array:
                            return new ArrayJsonValue((IJsonValue[]) aggregation.Value);
                    }
                }
            }
            return base.Evaluate(document);
        }
    }
}
