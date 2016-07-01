using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.JSON.Expressions;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class Offset
    {
        public IntegerConstantValue Skip { get; set; }
        public IntegerConstantValue Limit { get; set; }
    }
}
