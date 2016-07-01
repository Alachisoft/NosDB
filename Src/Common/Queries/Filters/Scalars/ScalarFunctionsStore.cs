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
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;

namespace Alachisoft.NosDB.Common.Queries.Filters.Scalars
{
    public class ScalarFunctionsStore
    {
        public static IFunction GetScalarFunction(Function function)
        {
            IFunction assignedfunction = null;
            switch (function.FunctionName)
            {
                case "lcase":
                    assignedfunction = new LCASE();
                    break;

                case "len":
                    assignedfunction = new LEN();
                    break;

                case "mid":
                    try
                    {
                        if (function.Arguments.Count > 1)
                        {
                            if (function.Arguments.Count.Equals(3))
                            {
                                IJsonValue val1, val2;
                                if (function.Arguments[1].Evaluate(out val1, null) &&
                                    function.Arguments[2].Evaluate(out val2, null))
                                    assignedfunction = new MID(Convert.ToInt32(val1.Value), Convert.ToInt32(val1.Value));
                            }
                            else if (function.Arguments.Count.Equals(2))
                            {
                                IJsonValue val1;
                                if (function.Arguments[1].Evaluate(out val1, null))
                                    assignedfunction = new MID(Convert.ToInt32(val1.Value));
                            }
                        }
                        else
                        {
                            ThrowInvalidNumberOfArgumentsException(function.FunctionName);
                        }
                    }
                    catch (Exception ex)
                    {
                        ThrowInvalidArgumentsException(ex, function.FunctionName);
                        throw;
                    }
                    break;

                case "now":
                    assignedfunction = new NOW();
                    break;

                case "round":
                    try
                    {
                        if (function.Arguments.Count.Equals(2))
                        {
                            IJsonValue val1;
                            if (function.Arguments[1].Evaluate(out val1, null))
                            assignedfunction = new ROUND(Convert.ToInt32(val1.Value));
                        }
                        else
                        {
                            ThrowInvalidNumberOfArgumentsException(function.FunctionName);
                        }
                    }
                    catch (Exception ex)
                    {
                        ThrowInvalidArgumentsException(ex, function.FunctionName);
                        throw;
                    }
                    break;

                case "ucase":
                    assignedfunction = new UCASE();
                    break;

                case "format":
                    try
                    {
                        if (function.Arguments.Count.Equals(2))
                        {
                            IJsonValue val1;
                            if (function.Arguments[1].Evaluate(out val1, null))
                                assignedfunction = new FORMAT(Convert.ToString(val1.Value));
                        }
                        else
                        {
                            ThrowInvalidNumberOfArgumentsException(function.FunctionName);
                        }
                    }
                    catch (Exception ex)
                    {
                        ThrowInvalidArgumentsException(ex, function.FunctionName);
                        throw;
                    }
                    break;
            }
            return assignedfunction;
        }

        private static void ThrowInvalidArgumentsException(Exception ex, string functionName)
        {
            if (ex is InvalidCastException || ex is OverflowException || ex is FormatException || ex is NullReferenceException)
                throw new QuerySystemException(ErrorCodes.Query.INVALID_SCALAR_FUNCTION_ARGUMENTS, new[] { functionName });
        }

        private static void ThrowInvalidNumberOfArgumentsException(string functionName)
        {
            throw new QuerySystemException(ErrorCodes.Query.INVALID_NUMBER_OF_SCALAR_FUNCTION_ARGUMENTS, new[] { functionName });
        }
    }
}
