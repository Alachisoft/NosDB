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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public interface IEvaluable : IPrintable
    {
        string InString { get; }
        string CaseSensitiveInString { get; }

        EvaluationType EvaluationType { get; }
        List<Attribute> Attributes { get; }
        List<Function> Functions { get; }

        void AssignConstants(IList<IParameter> parameters);

        bool Evaluate(out IJsonValue value, IJSONDocument document);

        IComparable Add(IEvaluable evaluable, IJSONDocument document);
        IComparable Subtract(IEvaluable evaluable, IJSONDocument document);
        IComparable Multiply(IEvaluable evaluable, IJSONDocument document);
        IComparable Divide(IEvaluable evaluable, IJSONDocument document);
        IComparable Modulate(IEvaluable evaluable, IJSONDocument document);
    }
}