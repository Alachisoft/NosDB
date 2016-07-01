// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.NosDBPS
{
    public class MultiLineRow:List<String[]>,ITableRow,IComparable
    {
        private PrintableTable printableTable;

        public int LinesCount
        {
            get
            {
                int maxLength = 0;
                foreach (string[] column in this)
                {
                    if (column.Length > maxLength)
                    {
                        maxLength = column.Length;
                    }
                }
                return maxLength;
            }
        }

        public MultiLineRow(PrintableTable printableTable)
        {
            // TODO: Complete member initialization
            this.printableTable = printableTable;
        }

        public string GetRowValue()
        {
            StringBuilder sb = new StringBuilder();
            SetStringFormat(sb);
            return sb.ToString();
        }


        public void SetStringFormat(StringBuilder sb)
        {

            for (int x = 0; x < LinesCount; x++)
            {
                List<string> rowLine = new List<string>();
                foreach (string[] column in this)
                {    
                    rowLine.Add((x < column.Length?  column[x]:string.Empty ));
                }
                sb.AppendFormat(printableTable.FormatString, rowLine.ToArray());
            }
            if (this.Count>1)
                sb.AppendFormat(printableTable.FormatString, new string[this.Count]);
        }

        public int CompareTo(object obj)
        {
            if(this.Count>0)
            {
                if (obj is MultiLineRow)
                {
                    if (((MultiLineRow)obj).Count > 0)
                    {
                        return this.First()[0].CompareTo(((MultiLineRow)obj).First()[0]);
                    }
                }
            }
            return 0;
        }
    }
}
