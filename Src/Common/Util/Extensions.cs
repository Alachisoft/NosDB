using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NoSQL.Common.Util
{
    public static class objExtensions
    {
        public static object ToJsonValue(this object obj)
        {
            if(obj is string || obj is DateTime)
                return string.Format("\"{0}\"", obj);
            return obj.ToString();
        }

    }
}
