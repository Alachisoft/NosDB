using Alachisoft.NoSQL.Common.Enum;
using Alachisoft.NoSQL.Common.JSON;
using Alachisoft.NoSQL.Common.JSON.Expressions;
using Alachisoft.NoSQL.Common.Queries;
using Alachisoft.NoSQL.Common.Server.Engine;
using Alachisoft.NoSQL.Core.Storage.Queries.Util;

namespace Alachisoft.NoSQL.Core.Storage.Queries.Filters.Aggregations
{
    internal class FIRST : IAggregation
    {
        private object first;

      
        public void ApplyValue(IJSONDocument value)
        {
            IJsonValue evaluatedValue = _fieldName.Evaluate(value);
            if (first == null)
                first = evaluatedValue.Value;
        }

        public string Tag
        {
            get { return Statics.FIRST; }
        }

        public string TargetField
        {
            get { return Tag + "(" + _fieldName + ")"; }
        }

        public IEvaluable FieldName
        {
            get { return _fieldName; }
        }

        public bool Distinct { get; set; }

        public Common.Enum.AggregateFunctionType Type
        {
            get { return AggregateFunctionType.FIRST; }
        }

        public string MethodName
        {
            get { return Tag; }
        }

        public object Value
        {
            get { return first; }
            set { first = value; }
        }

        public object Clone()
        {
            FIRST target = new FIRST(_fieldName);
            target.first = first;
            return target;
        }

        public string Name
        {
            get { throw new System.NotImplementedException(); }
        }

        public void ApplyValue(params object[] values)
        {
            throw new System.NotImplementedException();
        }
    }
}
