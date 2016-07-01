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
using System.IO;
using Alachisoft.NosDB.Common.Queries.Parser;

namespace Alachisoft.NosDB.Common.Queries.Util
{
    public class QueryParsingHelper
    {
        private const string GrammarTable = "Alachisoft.NosDB.Common.Queries.NCacheDB_DQL1.0.cgt";
        private  readonly DQLParser _parser;
        private Reduction _currentReduction;
        private readonly object _lockObject = new object();


        public QueryParsingHelper()
        {
            _parser = new DQLParser(GrammarTable);
        }

        public ParseMessage Parse(string query)
        {
            TextReader textReader = new StringReader(query);
            ParseMessage message;

            lock (_lockObject)
            {
                message = _parser.Parse(textReader, true);
                _currentReduction = _parser.CurrentReduction;
            }

            return message;
        }

        public string LineNumber
        {
            get
            {
                lock (_lockObject)
                {
                    return _parser.CurrentLineNumber.ToString();
                }
            }
        }

        public string Keyword
        {
            get
            {
                lock (_lockObject)
                {
                    return _parser.CurrentToken.Data.ToString();
                }
            }
        }

        public Reduction CurrentReduction
        {
            get { return _currentReduction; }
        }
    }
}
