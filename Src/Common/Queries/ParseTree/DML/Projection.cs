using System;
using System.Collections.Generic;

using Alachisoft.NosDB.Common.JSON.Expressions;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class Projection
    {
        public bool Distinct { get; set; }
        public IntegerConstantValue Limit { get; set; } 
        public List<IEvaluable> Evaluables { get; set; }
    }
}
