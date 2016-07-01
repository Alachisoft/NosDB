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
    public class PrintableTable
    {
        protected List<ITableRow> rows = new List<ITableRow>();
        protected List<ITableRow> heading = new List<ITableRow>();
        protected List<int> colLength = new List<int>();
        protected String _fmtString = null;

        public String FormatString
        {
            get
            {
                if (_fmtString == null)
                {
                    String format = "";
                    int i = 0;
                    foreach (int len in colLength)
                    {
                        format += String.Format("{{{0},-{1}}}{2}", i++, len, Separator);
                    }
                    format += "\r\n";
                    _fmtString = format;
                }
                return _fmtString;
            }
        }

        public string Separator { get; set; }
        
        public PrintableTable(string separator)
        {
            Separator = separator;
        }

        public PrintableTable()
        {
            Separator = "  ";
        }

        public void AddHeader(params string[] header)
        {
            List<string> underline = new List<string>();
            foreach(string x in header)
            {
                underline.Add(new string('-', x.Length));
            }
            AddHeaderRow(header);
            AddHeaderRow(underline.ToArray());
        }

        public void AddRow(params string [] cols)
        {
            TableRow row = new TableRow(this);
            foreach (string o in cols)
            {
                string str = o.ToString().Trim();
                row.Add(str);
                if (colLength.Count >= row.Count)
                {
                    int curLength = colLength[row.Count - 1];
                    if (str.Length > curLength) colLength[row.Count - 1] = str.Length;
                }
                else
                {
                    colLength.Add(str.Length);
                }
            }
            rows.Add(row);
        }

        public void AddMultiLineRow(params string[] [] cols)
        {
            MultiLineRow row = new MultiLineRow(this);
            foreach(string[] col in cols)
            {
                row.Add(col);
                if (colLength.Count >= row.Count)
                {
                    int curLength = colLength[row.Count - 1];
                    int maxLength = 0;
                    foreach(string str in col)
                    {
                        if(str.Length > maxLength)
                        {
                            maxLength = str.Length;
                        }
                    }
                    if (maxLength > curLength) colLength[row.Count - 1] = maxLength;
                }
                else
                {
                    int maxLength = 0;
                    foreach (string str in col)
                    {
                        if (str.Length > maxLength)
                        {
                            maxLength = str.Length;
                        }
                    }
                    colLength.Add(maxLength);
                }
            }
            rows.Add(row);
            
        }

        public string GetTableRows()
        {
            StringBuilder sb = new StringBuilder();
            rows.Sort();
            foreach (ITableRow row in heading)
            {
                row.SetStringFormat(sb);
            }
            foreach(ITableRow row in rows)
            {

                row.SetStringFormat(sb);
              

            }
            return sb.ToString();
        }

        private void AddHeaderRow(params object[] cols)
        {
            TableRow row = new TableRow(this);
            foreach (object o in cols)
            {
                String str = o.ToString().Trim();
                row.Add(str);
                if (colLength.Count >= row.Count)
                {
                    int curLength = colLength[row.Count - 1];
                    if (str.Length > curLength) colLength[row.Count - 1] = str.Length;
                }
                else
                {
                    colLength.Add(str.Length);
                }
            }
            heading.Add(row);
        }

    }
}
