using Alachisoft.NoSQL.Common.Enum;
using Alachisoft.NoSQL.Common.JSON;
using Alachisoft.NoSQL.Common.JSON.Expressions;
using Alachisoft.NoSQL.Common.Server.Engine;
using Alachisoft.NoSQL.Core.Storage.Queries.Util;

namespace Alachisoft.NoSQL.Core.Storage.Queries.Filters.Aggregations
{
    public class LAST : IAggregateFunction
    {
        private object last;
        private IEvaluable _fieldName;

        public LAST(IEvaluable fieldName)
        {
            _fieldName = fieldName;
        }

        public void ApplyValue(IJSONDocument value)
        {
            IJsonValue evaluatedValue = _fieldName.Evaluate(value);
            last = evaluatedValue.Value;
        }

        public string Tag
        {
            get { return Statics.LAST; }
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
            get { return AggregateFunctionType.LAST; }
        }

        public string MethodName
        {
            get { return Tag; }
        }

        public object Value
        {
            get { return last; }
            set { last = value; }
        }

        public object Clone()
        {
            LAST target = new LAST(_fieldName);
            target.last = last;
            return target;
        }
    }
}
