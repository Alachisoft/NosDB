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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Queries.Parser;
using Alachisoft.NosDB.Common.Queries.ParseTree;
using Alachisoft.NosDB.Common.Queries.ParseTree.DCL;
using Alachisoft.NosDB.Common.Queries.ParseTree.DDL;
using Alachisoft.NosDB.Common.Queries.ParseTree.DML;
using Alachisoft.NosDB.Common.Queries.Updation;


using Attribute = Alachisoft.NosDB.Common.JSON.Expressions.Attribute;

namespace Alachisoft.NosDB.Core.Queries
{
    public class DqlParserRule
    {
 
        public DqlParserRule()
		{

		}

        #region Helpers

        private Reduction GetReduction(Reduction reduction, int index)
        {
            return (Reduction) (reduction.GetToken(index)).Data;
        }

        private object GetReductionTag(Reduction reduction, int index)
        {
            return ((Reduction)(reduction.GetToken(index)).Data).Tag;
        }

        private T GetReductionTag<T>(Reduction reduction, int index)
        {
            return (T)((Reduction)(reduction.GetToken(index)).Data).Tag;
        }

        private string GetReductionDataString(Reduction reduction, int index)
        {
            return (reduction.GetToken(index)).Data as string;
        }

        private bool IsAttributeParent(Reduction reduction)
        {
            //Bug:: GoldParser's bug... won't trunicate space off terminal's name.
            if (reduction.ParentRule.Definition.Equals("Identifier "))
                return true;
            return false;
        }

        private IEvaluable GetEvaluable(Reduction reduction)
        {
            if (reduction.Tag is IEvaluable)
                return (IEvaluable)reduction.Tag;

            return IsAttributeParent(reduction) ? new Attribute(reduction.Tag.ToString())
                    : GetConstantEvaluable(reduction.Tag.ToString());
        }

        private Attribute ParseDelimitedIdentifier(string identifier)
        {
            //Marked... Needs parsing by the means of using regex for extracting 
            //array indexers and dot operators.

            return new Attribute(identifier);
        }

        private IEvaluable GetConstantEvaluable(string valueString)
        {
            valueString = valueString.ToLower();

            if (valueString.Contains("\""))
                return new StringConstantValue((valueString));

            if (valueString.Equals("*"))
                return new AllEvaluable();

            if (valueString.Equals("true"))
                return new BooleanConstantValue(true, valueString);

            if (valueString.Equals("false"))
                return new BooleanConstantValue(false, valueString);

            if (Regex.IsMatch(valueString, @"^[0-9]*$"))
                return new IntegerConstantValue(valueString);

            if (Regex.IsMatch(valueString, @"^(([0-9]*)\.([0-9]+)$)"))
                return new DoubleConstantValue(valueString);

            return new StringConstantValue((valueString));
        }

        private Reduction AssignBinaryExpression(Reduction reduction, ArithmeticOperation operation)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            Reduction rhsReduction = GetReduction(reduction, 2);

            if (lhsReduction.Tag is string || rhsReduction.Tag is string)
            {
                if (lhsReduction.Tag is string && rhsReduction.Tag is string)
                {
                    reduction.Tag = new BinaryExpression(GetEvaluable(lhsReduction),
                        operation, GetEvaluable(rhsReduction));
                }
                else if (lhsReduction.Tag is string)
                {
                    reduction.Tag = new BinaryExpression(GetEvaluable(lhsReduction), operation, (IEvaluable)rhsReduction.Tag);
                }
                else if (rhsReduction.Tag is string)
                {
                    reduction.Tag = new BinaryExpression((IEvaluable)lhsReduction.Tag, operation, GetEvaluable(rhsReduction));
                }
            }
            else
            {
                reduction.Tag = new BinaryExpression((IEvaluable)lhsReduction.Tag, operation, (IEvaluable)rhsReduction.Tag);
            }
            return null;
        }

        private Reduction AssignComparisonExpressionUniary(Reduction reduction, Condition condition)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);

            if (lhsReduction.Tag is string)
            {
                reduction.Tag = new ComparisonPredicate(
                    new Attribute(lhsReduction.Tag as string), condition);
            }
            else if (lhsReduction.Tag is Attribute)
            {
                reduction.Tag = new ComparisonPredicate((Attribute)lhsReduction.Tag, condition);
            }
            else
            {
                throw new QuerySystemException(ErrorCodes.Query.INVALID_SINGLE_ATTRIBUTE_ARGUMENT,
                    new []{condition.ToString()});
            }
            return null;
        }

        private Reduction AssignComparisonExpressionBinary(Reduction reduction, Condition condition, int secondIndex = 2)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            Reduction rhsReduction = GetReduction(reduction, secondIndex);

            if ((condition.CompareTo(Condition.Like) == 0 || condition.CompareTo(Condition.NotLike) == 0)
                && (rhsReduction.Tag is StringConstantValue))
            {
                ((StringConstantValue)rhsReduction.Tag).PossibleWildCard = true;
            }

            reduction.Tag = new ComparisonPredicate(GetEvaluable(lhsReduction),
                condition, GetEvaluable(rhsReduction));
            return null;
        }

        private Reduction AssignComparisonExpressionBinary(Reduction reduction, Condition condition, IEvaluable value2)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            reduction.Tag = new ComparisonPredicate(GetEvaluable(lhsReduction), condition, value2);
            return null;
        }
       
        private BinaryExpression GetArithematicOperation(Reduction lhsReduction)
        {
            if (lhsReduction.Tag is string)
            {
                return new BinaryExpression(GetEvaluable(lhsReduction));
            }

            if (lhsReduction.Tag is IEvaluable)
            {
                return new BinaryExpression((IEvaluable)lhsReduction.Tag);
            }

            return (BinaryExpression)lhsReduction.Tag;
        }

        #endregion

        #region reduction methods

        ///Implements ////<Statement> ::= <SPStatement>  
        public Reduction CreateRULE_STATEMENT(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<Statement> ::= <DMLStatement>
        public Reduction CreateRULE_STATEMENT2(Reduction reduction)
        {
            return null;
        }
        
        ///Implements ////<SingleStatment> ::= <DMLStatement>
        public Reduction CreateRULE_SINGLESTATMENT(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<SingleStatment> ::= <SPStatement>
        public Reduction CreateRULE_SINGLESTATMENT2(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<MultiStatement> ::= <DMLStatement> <OptTerminator> <MultiStatement>
        public Reduction CreateRULE_MULTISTATEMENT(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 2);

            if (!(rhs is IList<IDqlObject>))
            {
                var temp = rhs as IDqlObject;
                rhs = new ParsedObjects();
                ((ParsedObjects)rhs).Add(temp);
            }

            ((ParsedObjects)rhs).AddFirst(lhs as IDqlObject);

            reduction.Tag = rhs;
            return null;
        }

        ///Implements ////<MultiStatement> ::= <DDLStatement> <OptTerminator> <MultiStatement>
        public Reduction CreateRULE_MULTISTATEMENT2(Reduction reduction)
        {
            return CreateRULE_MULTISTATEMENT(reduction);
        }

        ///Implements ////<MultiStatement> ::= <DCLStatement> <OptTerminator> <MultiStatement>
        public Reduction CreateRULE_MULTISTATEMENT3(Reduction reduction)
        {
            reduction.Tag = CreateRULE_MULTISTATEMENT(reduction);
            return null;
        }

        ///Implements ////<MultiStatement> ::= <DCLStatement> <OptTerminator>
        public Reduction CreateRULE_MULTISTATEMENT4(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<MultiStatement> ::= <DDLStatement> <OptTerminator>
        public Reduction CreateRULE_MULTISTATEMENT5(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<MultiStatement> ::= <DCLStatement> <OptTerminator>
        public Reduction CreateRULE_MULTISTATEMENT6(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<DMLStatement> ::= <SelectQuery>
        public Reduction CreateRULE_DMLSTATEMENT(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DMLStatement> ::= <InsertQuery>
        public Reduction CreateRULE_DMLSTATEMENT2(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DMLStatement> ::= <UpdateQuery>
        public Reduction CreateRULE_DMLSTATEMENT3(Reduction reduction)
        {
            return null;
        }

        ///Implements ///<DMLStatement> ::= <DeleteQuery>
        public Reduction CreateRULE_DMLSTATEMENT4(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<SPStatement> ::= <ExecuteStatement>
        public Reduction CreateRULE_SPSTATEMENT(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DDLStatement> ::= <CreateStatement>
        public Reduction CreateRULE_DDLSTATEMENT(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DDLStatement> ::= <AlterStatement>
        public Reduction CreateRULE_DDLSTATEMENT2(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DDLStatement> ::= <DropStatement>
        public Reduction CreateRULE_DDLSTATEMENT3(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DDLStatement> ::= <TruncateStatement>
        public Reduction CreateRULE_DDLSTATEMENT4(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DDLStatement> ::= <BackupStatement>
        public Reduction CreateRULE_DDLSTATEMENT5(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DDLStatement> ::= <RestoreStatement>
        public Reduction CreateRULE_DDLSTATEMENT6(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DCLStatement> ::= <GrantStatement>
        public Reduction CreateRULE_DCLSTATEMENT(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DCLStatement> ::= <RevokeStatement>
        public Reduction CreateRULE_DCLSTATEMENT2(Reduction reduction)
        {
            return null;
        }

        ///Implements  ////<LimitedId> ::= LimitId
        public Reduction CreateRULE_LIMITEDID_LIMITID(Reduction reduction)
        {
            reduction.Tag = GetReductionDataString(reduction, 0);
            return null;
        }

        ///Implements ////<DelimitedIdDollars> ::= DelimitIdDollars
        public Reduction CreateRULE_DELIMITEDIDDOLLARS_DELIMITIDDOLLARS(Reduction reduction)
        {
            reduction.Tag = GetReductionDataString(reduction, 0).Trim(new []{'$'});
            return null;
        }

        ///Implements ////<DelimitIdQuotes> ::= DelimitIdQuotes
        public Reduction CreateRULE_DELIMITIDQUOTES_DELIMITIDQUOTES(Reduction reduction)
        {
            reduction.Tag = GetReductionDataString(reduction, 0).Trim('"');
            return null;
        }

        ///Implements ////<DelimitedId> ::= <DelimitedIdDollars>
        public Reduction CreateRULE_DELIMITEDID(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DelimitedId> ::= <DelimitIdQuotes>
        public Reduction CreateRULE_DELIMITEDID2(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<Parameter> ::= ParameterRule
        public Reduction CreateRULE_PARAMETER_PARAMETERRULE(Reduction reduction)
        {
            reduction.Tag = new Parameter(GetReductionDataString(reduction, 0).Trim('@'));
            return null;
        }
        
        ///Implements ////<DistinctRestriction> ::= DISTINCT
        public Reduction CreateRULE_DISTINCTRESTRICTION_DISTINCT(Reduction reduction)
        {
            reduction.Tag = true;
            return null;
        }

        ///Implements ////<DistinctRestriction> ::= 
        public Reduction CreateRULE_DISTINCTRESTRICTION(Reduction reduction)
        {
            reduction.Tag = false;
            return null;
        }

        ///Implements ////<Identifier> ::= <DelimitId>
        public Reduction CreateRULE_IDENTIFIER(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<Identifier> ::= <LimitedId>
        public Reduction CreateRULE_IDENTIFIER2(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<Attribute> ::= <Identifier> <Indexer>
        public Reduction CreateRULE_ATTRIBUTE(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object mhs = GetReductionTag(reduction, 1);

            lhs = new Attribute(lhs as string);

            if (mhs != null)
            {
                ((Attribute)lhs).Indecies = ((List<int>)mhs).ToArray();
            }

            reduction.Tag = lhs;
            return null;
        }
        
        ///Implements ////<Attribute> ::= <Identifier> <Indexer> '.' <Attribute>
        public Reduction CreateRULE_ATTRIBUTE_DOT(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object mhs = GetReductionTag(reduction, 1);
            object rhs = GetReductionTag(reduction, 3);

            lhs = new Attribute(lhs as string);

            if (mhs != null)
            {
                ((Attribute)lhs).Indecies = ((List<int>)mhs).ToArray();
            }

            if (!(rhs is Attribute))
            {
                rhs = new Attribute(rhs.ToString());
            }

            ((Attribute)lhs).ChildAttribute = rhs as Attribute;
            reduction.Tag = lhs;
            return null;
        }

        ///Implements ////<Indexer> ::= <Indexer> '[' IntegerLiteral ']'
        public Reduction CreateRULE_INDEXER_LBRACKET_INTEGERLITERAL_RBRACKET(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            string rhs = GetReductionDataString(reduction, 2);

            if (lhs == null)
                lhs = new List<int>();

            int index;
            if (!Int32.TryParse(rhs, out index) || index < 0)
            {
                throw new QuerySystemException(ErrorCodes.Query.INVALID_ARRAY_INDEX, new[] { rhs });
            }

            ((List<int>)lhs).Add(index);
            
            reduction.Tag = lhs;
            return null;
        }

        ///Implements ////<Indexer> ::= 
        public Reduction CreateRULE_INDEXER(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<StrLiteral> ::= StringLiteral
        public Reduction CreateRULE_STRLITERAL_STRINGLITERAL(Reduction reduction)
        {
            reduction.Tag = new StringConstantValue(GetReductionDataString(reduction, 0));
            return null;
        }

        ///Implements ////<NumLiteral> ::= IntegerLiteral
        public Reduction CreateRULE_NUMLITERAL_INTEGERLITERAL(Reduction reduction)
        {
            reduction.Tag = new IntegerConstantValue(GetReductionDataString(reduction, 0));
            return null;
        }

        ///Implements ////<NumLiteral> ::= RealLiteral
        public Reduction CreateRULE_NUMLITERAL_REALLITERAL(Reduction reduction)
        {
            reduction.Tag = new DoubleConstantValue(GetReductionDataString(reduction, 0));
            return null;
        }

        ///Implements ////<ReplaceList> ::= <BinaryExpression> '=' <BinaryExpression> ',' <ReplaceList>
        public Reduction CreateRULE_REPLACELIST_EQ_COMMA(Reduction reduction)
        {
            Reduction leftReduction = GetReduction(reduction, 0);
            Reduction middleReduction = GetReduction(reduction, 2);
            Reduction rightReduction = GetReduction(reduction, 4);

            if (leftReduction.Tag is string)
            {
                leftReduction.Tag = GetEvaluable(leftReduction);
            }
            if (middleReduction.Tag is string)
            {
                middleReduction.Tag = GetEvaluable(middleReduction);
            }

            List<KeyValuePair<IEvaluable, IEvaluable>> list;
            if (!(rightReduction.Tag is IList))
            {
                list = new List<KeyValuePair<IEvaluable, IEvaluable>>();
                list.Add(new KeyValuePair<IEvaluable, IEvaluable>((IEvaluable)leftReduction.Tag, (IEvaluable)middleReduction.Tag));
                list.Add(((KeyValuePair<IEvaluable, IEvaluable>)rightReduction.Tag));
                reduction.Tag = list;
            }
            else
            {
                ((List<KeyValuePair<IEvaluable, IEvaluable>>)rightReduction.Tag).Add
                    (new KeyValuePair<IEvaluable, IEvaluable>((IEvaluable)leftReduction.Tag,
                        (IEvaluable)middleReduction.Tag));
                reduction.Tag = rightReduction.Tag;
            }

           
            return null;
        }

        ///Implements////<ReplaceList> ::= <BinaryExpression> '=' <BinaryExpression>
        public Reduction CreateRULE_REPLACELIST_EQ(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            Reduction rhsReduction = GetReduction(reduction, 2);

            if (lhsReduction.Tag is string)
            {
                lhsReduction.Tag = GetEvaluable(lhsReduction);
            }

            if (rhsReduction.Tag is string)
            {
                rhsReduction.Tag = GetEvaluable(rhsReduction);
            }
            List<KeyValuePair<IEvaluable, IEvaluable>> list = new List<KeyValuePair<IEvaluable, IEvaluable>>();
            list.Add(new KeyValuePair<IEvaluable, IEvaluable>(
                (IEvaluable)lhsReduction.Tag, (IEvaluable)rhsReduction.Tag));
            reduction.Tag = list; 

            return null;
        }

        ///Implements ////<BinaryExprList> ::= '(' <BinaryExpressionList> ')'
        public Reduction CreateRULE_BINARYEXPRLIST_LPARAN_RPARAN(Reduction reduction)
        {
            Reduction valueReduction = GetReduction(reduction, 1);

            if (valueReduction.Tag is string)
            {
                valueReduction.Tag = GetEvaluable(valueReduction);
            }

            if (!(valueReduction.Tag is ValueList))
            {
                var temp = (IEvaluable)valueReduction.Tag;
                valueReduction.Tag = new ValueList();
                ((ValueList)valueReduction.Tag).Add(temp);
            }

            reduction.Tag = valueReduction.Tag;
            return null;
        }


        ///Implements ////<BinaryExpressionList> ::= <BinaryExpression> ',' <BinaryExpressionList>
        public Reduction CreateRULE_BINARYEXPRESSIONLIST_COMMA(Reduction reduction)
        {
            Reduction leftReduction = GetReduction(reduction, 0);
            Reduction rightReduction = GetReduction(reduction, 2);

            if (leftReduction.Tag is string)
            {
                leftReduction.Tag = GetEvaluable(leftReduction);
            }

            if (rightReduction.Tag is string)
            {
                rightReduction.Tag = GetEvaluable(rightReduction);
            }

            if (!(rightReduction.Tag is ValueList))
            {
                var temp = (IEvaluable)rightReduction.Tag;
                rightReduction.Tag = new ValueList() { (IEvaluable)leftReduction.Tag, temp}; 
            }
            else
            {
                var valueList = new ValueList() { (IEvaluable)leftReduction.Tag };
                valueList.AddRange(((ValueList)rightReduction.Tag).Values);

                rightReduction.Tag = valueList;
            }

            reduction.Tag = rightReduction.Tag;
            return null;
        }


        ///Implements ////<BinaryExpressionList> ::= <BinaryExpression>
        public Reduction CreateRULE_BINARYEXPRESSIONLIST(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<AtrList> ::= '(' <AttributeList> ')'
        public Reduction CreateRULE_ATRLIST_LPARAN_RPARAN(Reduction reduction)
        {
            Reduction valueReduction = GetReduction(reduction, 1);
            
            if (valueReduction.Tag is string)
            {
                valueReduction.Tag = new Attribute(valueReduction.Tag.ToString());
            }

            if (!(valueReduction.Tag is IList))
            {
                valueReduction.Tag = new List<Attribute>() { (Attribute)valueReduction.Tag };
            }

            reduction.Tag = valueReduction.Tag;
            return null;
        }
      
        ///Implements ////<Date> ::= DateTime '.' now
        public Reduction CreateRULE_DATE_DATETIME_DOT_NOW(Reduction reduction)
        {
            reduction.Tag = new DateTimeConstantValue();
            return null;
        }

        ///Implements ////<Date> ::= DateTime '(' StringLiteral ')'
        public Reduction CreateRULE_DATE_DATETIME_LPARAN_STRINGLITERAL_RPARAN(Reduction reduction)
        {
            reduction.Tag = new DateTimeConstantValue(GetReductionDataString(reduction, 2));
            return null;
        }

        ///Implements ////<CollectionName> ::= <LimitedId> '.' <LimitedId>
        public Reduction CreateRULE_COLLECTIONNAME_DOT(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0) + "."
                + GetReductionTag(reduction, 2);
            return null;
        }

        ///Implements ////<CollectionName> ::= <Identifier>
        public Reduction CreateRULE_COLLECTIONNAME(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0) as string;
            return null;
        }

        ///Implements ////<Value> ::= '-' <NumLiteral>
        public Reduction CreateRULE_VALUE_MINUS(Reduction reduction)
        {
            object value = GetReductionTag(reduction, 1);

            if (value is IntegerConstantValue)
            {
                reduction.Tag = new IntegerConstantValue("-" + value);
            }
            else
            {
                reduction.Tag = new DoubleConstantValue("-" + value);
            }
            return null;
        }


        ///Implements ////<Value> ::= '+' <NumLiteral>
        public Reduction CreateRULE_VALUE_PLUS(Reduction reduction)
        {
            object value = GetReductionTag(reduction, 0);

            if (value is IntegerConstantValue)
            {
                reduction.Tag = new IntegerConstantValue("+" + reduction.GetToken(1).Data);
            }
            else
            {
                reduction.Tag = new DoubleConstantValue("+" + reduction.GetToken(1).Data);
            }
            return null;
        }

        ///Implements ////<Value> ::= <ValueSign> <NumLiteral>
        public Reduction CreateRULE_VALUE(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 1);

            if (lhs != null)
            {
                if (rhs is IntegerConstantValue)
                {
                    if (lhs.Equals(ArithmeticOperation.Subtraction))
                    {
                        ((IntegerConstantValue)rhs).SetNegative();
                    }
                }
                if (rhs is DoubleConstantValue)
                {
                    if (lhs.Equals(ArithmeticOperation.Subtraction))
                    {
                        ((DoubleConstantValue)rhs).SetNegative();
                    }
                }
            }
            reduction.Tag = rhs;
            return null;
        }

        ///Implements ////<Value> ::= <StrLiteral>
        public Reduction CreateRULE_VALUE2(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<Value> ::= true
        public Reduction CreateRULE_VALUE_TRUE(Reduction reduction)
        {
            reduction.Tag = new BooleanConstantValue(true, GetReductionDataString(reduction, 0));
            return null;
        }

        ///Implements ////<Value> ::= false
        public Reduction CreateRULE_VALUE_FALSE(Reduction reduction)
        {
            reduction.Tag = new BooleanConstantValue(false, GetReductionDataString(reduction, 0));
            return null;
        }

        ///Implements ////<Value> ::= <Date>
        public Reduction CreateRULE_VALUE3(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<Value> ::= <Parameter>
        public Reduction CreateRULE_VALUE4(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<Value> ::= NULL
        public Reduction CreateRULE_VALUE_NULL(Reduction reduction)
        {
            reduction.Tag = NullValue.Null;
            //reduction.Tag = GetReductionTag(reduction, 0);
            //reduction.Tag = NullValue.Null;
            return null;
        }

        ///Implements ////<FunctionName> ::= <LimitedId>
        public Reduction CreateRULE_FUNCTIONNAME(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<FunctionAttrGroup> ::= <DistinctRestriction> <FuncExpressions>
        public Reduction CreateRULE_FUNCTIONATTRGROUP(Reduction reduction)
        {
            object restriction = GetReductionTag(reduction, 0);
            Reduction exprReduction = GetReduction(reduction, 1);

            if (exprReduction.Tag is string)
            {
                exprReduction.Tag = GetEvaluable(exprReduction);
            }
            if (exprReduction.Tag != null && !(exprReduction.Tag is IList))
            {
                exprReduction.Tag = new List<IEvaluable>() { (IEvaluable)exprReduction.Tag };
            }
            reduction.Tag = new ArrayList() { restriction.Equals(true), exprReduction.Tag };
            return null;
        }

        ///Implements ////<AttributeList> ::= <Attribute> ',' <AttributeList>
        public Reduction CreateRULE_ATTRIBUTELIST_COMMA(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            Reduction rhsReduction = GetReduction(reduction, 2);

            if (lhsReduction.Tag is string)
            {
                lhsReduction.Tag = new Attribute(lhsReduction.Tag.ToString());
            }
            if (rhsReduction.Tag is string)
            {
                rhsReduction.Tag = new Attribute(rhsReduction.Tag.ToString());
            }
            if (rhsReduction.Tag is IList)
            {
                //maintain the order of attributes 
                var tempAttributeList = new List<Attribute>() { (Attribute)lhsReduction.Tag };
                tempAttributeList.AddRange(((List<Attribute>)rhsReduction.Tag));
                rhsReduction.Tag = tempAttributeList;
            }
            else
            {
                Attribute temp = (Attribute)rhsReduction.Tag;
                rhsReduction.Tag = new List<Attribute>()
                {
                    (Attribute)lhsReduction.Tag,
                    temp
                };
            }
            reduction.Tag = rhsReduction.Tag;
            return null;
        }

        ///Implements ////<AttributeList> ::= <Attribute>
        public Reduction CreateRULE_ATTRIBUTELIST(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<JSONValList> ::= '(' <JSONValueList> ')'
        public Reduction CreateRULE_JSONVALLIST_LPARAN_RPARAN(Reduction reduction)
        {
            return CreateRULE_BINARYEXPRLIST_LPARAN_RPARAN(reduction);
        }

        ///Implements ////<JSONValueList> ::= <JSONValue> ',' <JSONValueList>
        public Reduction CreateRULE_JSONVALUELIST_COMMA(Reduction reduction)
        {
            return CreateRULE_BINARYEXPRESSIONLIST_COMMA(reduction);
        }

        ///Implements ////<JSONValueList> ::= <JSONValue>
        public Reduction CreateRULE_JSONVALUELIST(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<Function> ::= <FunctionName> '(' <FunctionAttrGroup> ')'
        public Reduction CreateRULE_FUNCTION_LPARAN_RPARAN(Reduction reduction)
        {
            string functionName = GetReductionTag(reduction, 0) as string;
            ArrayList exprlist = GetReductionTag(reduction, 2) as ArrayList;

            if (exprlist == null)
            {
                reduction.Tag = new Function(functionName);
                return null;
            }

            if((bool)exprlist[0])
                throw new QuerySystemException(ErrorCodes.Query.DISTICT_NOT_SUPPORTED); 

            reduction.Tag = new Function(functionName, false,
                exprlist[1] != null ? (List<IEvaluable>)exprlist[1] : null);

            return null;
        }

        ///Implements ////<AsExpressions> ::= <AsExpressions> ',' <BinaryExpression> <Alias>
        public Reduction CreateRULE_ASEXPRESSIONS_COMMA(Reduction reduction)
        {
            object rhs = GetReductionTag(reduction, 0);
            Reduction midReduction = GetReduction(reduction, 2);
            string alias = GetReductionTag(reduction, 3) as string;

            if (rhs is string && rhs.Equals("*"))
            {
                rhs = new AllEvaluable();
            }

            if (midReduction.Tag is string)
            {
                midReduction.Tag = GetEvaluable(midReduction);
            }

            if (alias != null)
            {
                if (!(midReduction.Tag is BinaryExpression))
                {
                    midReduction.Tag = new BinaryExpression((IEvaluable)midReduction.Tag);
                }

                ((BinaryExpression)midReduction.Tag).Alias = alias;
            }

            if (rhs is IList)
            {
                ((List<IEvaluable>)rhs).Add((IEvaluable)midReduction.Tag);
            }
            else
            {
                rhs = new List<IEvaluable>() { (IEvaluable)rhs, (IEvaluable)midReduction.Tag };
            }

            reduction.Tag = rhs;
            return null;
        }

        ///Implements ////<AsExpressions> ::= <BinaryExpression> <Alias>
        public Reduction CreateRULE_ASEXPRESSIONS(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            string rhs = GetReductionTag(reduction, 1) as string;

            if (lhsReduction.Tag is string)
            {
                lhsReduction.Tag = GetEvaluable(lhsReduction);
            }

            if (rhs != null)
            {
                if (!(lhsReduction.Tag is BinaryExpression))
                    lhsReduction.Tag = new BinaryExpression((IEvaluable)lhsReduction.Tag);

                ((BinaryExpression)lhsReduction.Tag).Alias = rhs;
            }

            reduction.Tag = lhsReduction.Tag;
            return null;
        }
        
        ///Implements ////<AsExpressions> ::= <AsExpressions> ',' '*'
        public Reduction CreateRULE_ASEXPRESSIONS_COMMA_TIMES(Reduction reduction)
        {
            Reduction rhsReduction = GetReduction(reduction, 0);

            if (rhsReduction.Tag is IList)
            {
                ((List<IEvaluable>)rhsReduction.Tag).Add(new AllEvaluable());
            }
            else
            {
                rhsReduction.Tag = new List<IEvaluable>() { new AllEvaluable(), GetEvaluable(rhsReduction) };
            }

            reduction.Tag = rhsReduction.Tag;
            return null;
        }

        ///Implements ////<AsExpressions> ::= '*'
        public Reduction CreateRULE_ASEXPRESSIONS_TIMES(Reduction reduction)
        {
            reduction.Tag = new List<IEvaluable> { new AllEvaluable() };
            return null;
        }

        ///Implements ////<FuncExpressions> ::= <FuncExpressions> ',' <BinaryExpression>
        public Reduction CreateRULE_FUNCEXPRESSIONS_COMMA(Reduction reduction)
        {
            Reduction rhsReduction = GetReduction(reduction, 0);
            Reduction lhsReduction = GetReduction(reduction, 2);

            if (lhsReduction.Tag is string)
            {
                lhsReduction.Tag = GetEvaluable(lhsReduction);
            }

            if (rhsReduction.Tag is string)
            {
                rhsReduction.Tag = GetEvaluable(rhsReduction);
            }

            if (rhsReduction.Tag is IList)
            {
                ((List<IEvaluable>)rhsReduction.Tag).Add((IEvaluable)lhsReduction.Tag);
            }
            else
            {
                //fix Funtion's argument order as parsed : bugfix 9211 
                rhsReduction.Tag = new List<IEvaluable>() { (IEvaluable)rhsReduction.Tag, (IEvaluable)lhsReduction.Tag };
            }
            reduction.Tag = rhsReduction.Tag;
            return null;
        }

        ///Implements ////<FuncExpressions> ::= <FuncExpressions> ',' '*'
        public Reduction CreateRULE_FUNCEXPRESSIONS_COMMA_TIMES(Reduction reduction)
        {
            Reduction rhsReduction = GetReduction(reduction, 0);

            if (rhsReduction.Tag is string)
            {
                rhsReduction.Tag = GetEvaluable(rhsReduction);
            }

            if (rhsReduction.Tag is IList)
            {
                ((List<IEvaluable>)rhsReduction.Tag).Add(new AllEvaluable());
            }
            else
            {
                rhsReduction.Tag = new List<IEvaluable>() { new AllEvaluable(), (IEvaluable)rhsReduction.Tag };
            }
            reduction.Tag = rhsReduction.Tag;
            return null;
        }

        ///Implements ////<FuncExpressions> ::= <BinaryExpression>
        public Reduction CreateRULE_FUNCEXPRESSIONS(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<FuncExpressions> ::= '*'
        public Reduction CreateRULE_FUNCEXPRESSIONS_TIMES(Reduction reduction)
        {
            reduction.Tag = new AllEvaluable();
            return null;
        }

        ///Implements ////<FuncExpressions> ::= 
        public Reduction CreateRULE_FUNCEXPRESSIONS2(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<Alias> ::= AS <LimitedId>
        public Reduction CreateRULE_ALIAS_AS(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 1);
            return null;
        }

        ///Implements ////<Alias> ::= 
        public Reduction CreateRULE_ALIAS(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<AddExpression> ::= <MultExpression> '+' <AddExpression>
        public Reduction CreateRULE_ADDEXPRESSION_PLUS(Reduction reduction)
        {
            return AssignBinaryExpression(reduction, ArithmeticOperation.Addition);
        }

        ///Implements  ////<AddExpression> ::= <MultExpression> '-' <AddExpression>
        public Reduction CreateRULE_ADDEXPRESSION_MINUS(Reduction reduction)
        {
            return AssignBinaryExpression(reduction, ArithmeticOperation.Subtraction);
        }

        ///Implements  ////<AddExpression> ::= <MultExpression>
        public Reduction CreateRULE_ADDEXPRESSION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<MultExpression> ::= <MultExpression> '*' <ValueExpression>
        public Reduction CreateRULE_MULTEXPRESSION_TIMES(Reduction reduction)
        {
            return AssignBinaryExpression(reduction, ArithmeticOperation.Multiplication);
        }

        ///Implements ////<MultExpression> ::= <MultExpression> '/' <ValueExpression>
        public Reduction CreateRULE_MULTEXPRESSION_DIV(Reduction reduction)
        {
            return AssignBinaryExpression(reduction, ArithmeticOperation.Division);
        }

        ///Implements ////<MultExpression> ::= <MultExpression> '%' <ValueExpression>
        public Reduction CreateRULE_MULTEXPRESSION_PERCENT(Reduction reduction)
        {
            return AssignBinaryExpression(reduction, ArithmeticOperation.Modulus);
        }

        ///Implements ////<MultExpression> ::= <ValueExpression>
        public Reduction CreateRULE_MULTEXPRESSION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<ValueExpression> ::= <ValueSign> <JSONValue>
        public Reduction CreateRULE_VALUEEXPRESSION(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 1);

            if (rhs is string)
            {
                rhs = GetConstantEvaluable(rhs as string);
            }

            if (lhs != null)
            {
                if (!(rhs is IntegerConstantValue || rhs is DoubleConstantValue
                    || rhs is Parameter))
                {
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_ARITHMETIC_OPERATOR_WITH_CONSTANT, new []{ rhs.ToString() });
                }
                if (rhs is IntegerConstantValue)
                {
                    if (lhs.Equals(ArithmeticOperation.Subtraction))
                    {
                        ((IntegerConstantValue)rhs).SetNegative();
                    }
                }
                if (rhs is DoubleConstantValue)
                {
                    if (lhs.Equals(ArithmeticOperation.Subtraction))
                    {
                        ((DoubleConstantValue)rhs).SetNegative();
                    }
                }
                if (rhs is Parameter)
                {
                    ((Parameter)rhs).ArithmeticOperation = (ArithmeticOperation)lhs;
                }
            }
            reduction.Tag = rhs;
            return null;
        }

        ///Implements ////<ValueExpression> ::= <ValueSign> <Attribute>
        public Reduction CreateRULE_VALUEEXPRESSION2(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 1);

            if (rhs is string)
            {
                rhs = new Attribute(rhs as string);
            }

            if (lhs != null)
            {
                ((Attribute)rhs).ArithmeticOperation = (ArithmeticOperation)rhs;
            }

            reduction.Tag = rhs;
            return null;
        }

        ///Implements ////<ValueExpression> ::= <ValueSign> <Function>
        public Reduction CreateRULE_VALUEEXPRESSION3(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 1);
            if (lhs != null)
            {
                ((Attribute)rhs).ArithmeticOperation = (ArithmeticOperation)rhs;
            }
            reduction.Tag = rhs;
            return null;
        }

        ///Implements ////<ValueExpression> ::= <ValueSign> <ParensExpression>
        public Reduction CreateRULE_VALUEEXPRESSION4(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 1);
            if (lhs != null)
            {
                if (rhs is IntegerConstantValue || rhs is DoubleConstantValue || rhs is Parameter)
                {
                    return CreateRULE_VALUEEXPRESSION(reduction);
                }
                if (rhs is BinaryExpression)
                {
                    ((BinaryExpression)rhs).ReturnOperation = (ArithmeticOperation)lhs;
                    reduction.Tag = rhs;
                    return null;
                }
                throw new QuerySystemException(ErrorCodes.Query.INVALID_ARITHMETIC_OPERATOR_WITH_CONSTANT, new[] { rhs.ToString() });
            }
            reduction.Tag = rhs;
            return null;
        }

        ///Implements ////<ValueSign> ::= '+'
        public Reduction CreateRULE_VALUESIGN_PLUS(Reduction reduction)
        {
            reduction.Tag = ArithmeticOperation.Addition;
            return null;
        }

        ///Implements ////<ValueSign> ::= '-'
        public Reduction CreateRULE_VALUESIGN_MINUS(Reduction reduction)
        {
            reduction.Tag = ArithmeticOperation.Subtraction;
            return null;
        }

        ///Implements ////<ValueSign> ::= 
        public Reduction CreateRULE_VALUESIGN(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<ParamList> ::= <BinaryExprList>
        public Reduction CreateRULE_PARAMLIST(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<ParamList> ::= <Parameter>
        public Reduction CreateRULE_PARAMLIST2(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<ParamList> ::= <Attribute>
        public Reduction CreateRULE_PARAMLIST3(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<ParamInteger> ::= IntegerLiteral
        public Reduction CreateRULE_PARAMINTEGER_INTEGERLITERAL(Reduction reduction)
        {
            reduction.Tag = new IntegerConstantValue(GetReductionDataString(reduction, 0));
            return null;
        }

        ///Implements ////<ParamInteger> ::= <Parameter>
        public Reduction CreateRULE_PARAMINTEGER(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<ParensExpression> ::= '(' <ArithExpression> ')'
        public Reduction CreateRULE_PARENSEXPRESSION_LPARAN_RPARAN(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 1);
            return null;
        }

        ///Implements ////<Order> ::= ASC
        public Reduction CreateRULE_ORDER_ASC(Reduction reduction)
        {
            reduction.Tag = SortOrder.ASC;
            return null;
        }

        ///Implements ////<Order> ::= DESC
        public Reduction CreateRULE_ORDER_DESC(Reduction reduction)
        {
            reduction.Tag = SortOrder.DESC;
            return null;
        }

        ///Implements ////<Order> ::= 
        public Reduction CreateRULE_ORDER(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<WhereSection> ::= WHERE <Expression>
        public Reduction CreateRULE_WHERESECTION_WHERE(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 1);
            return null;
        }

        ///Implements ////<WhereSection> ::= 
        public Reduction CreateRULE_WHERESECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements  ////<Expression> ::= <OrExpr>
        public Reduction CreateRULE_EXPRESSION(Reduction reduction)
        {
            //Won't be called.
            return null;
        }

        ///Implements ////<OrExpr> ::= <OrExpr> OR <AndExpr>
        public Reduction CreateRULE_OREXPR_OR(Reduction reduction)
        {
            ITreePredicate lhs = GetReductionTag(reduction, 0) as ITreePredicate;
            ITreePredicate rhs = GetReductionTag(reduction, 2) as ITreePredicate;

            if (lhs == null || rhs == null)
            {
                return null;
            }

            if (rhs is OrTreePredicate && !rhs.Completed)
            {
                ((OrTreePredicate)rhs).Add(lhs);
                reduction.Tag = rhs;
            }
            else
            {
                OrTreePredicate andExpupper = new OrTreePredicate();
                andExpupper.Add(lhs);
                andExpupper.Add(rhs);
                reduction.Tag = andExpupper;
            }
            return null;
        }

        ///Implements ////<OrExpr> ::= <AndExpr>
        public Reduction CreateRULE_OREXPR(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<AndExpr> ::= <UnaryExpr> AND <AndExpr> 
        public Reduction CreateRULE_ANDEXPR_AND(Reduction reduction)
        {
            ITreePredicate lhs = GetReductionTag(reduction, 0) as ITreePredicate;
            ITreePredicate rhs = GetReductionTag(reduction, 2) as ITreePredicate;

            if (lhs == null || rhs == null)
            {
                return null;
            }

            if (rhs is AndTreePredicate && !rhs.Completed)
            {
                ((AndTreePredicate)rhs).Add(lhs);
                reduction.Tag = rhs;
            }
            else
            {
                AndTreePredicate andExpupper = new AndTreePredicate();
                andExpupper.Add(lhs);
                andExpupper.Add(rhs);
                reduction.Tag = andExpupper;
            }
            return null;
        }

        ///Implements ////<AndExpr> ::= <UnaryExpr>
        public Reduction CreateRULE_ANDEXPR(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<UnaryExpr> ::= NOT <CompareExpr>
        public Reduction CreateRULE_UNARYEXPR_NOT(Reduction reduction)
        {
            object value = GetReductionTag(reduction, 1);
            if (value is ComparisonPredicate)
            {
                ((ComparisonPredicate)value).IsNot = true;
            }
            reduction.Tag = value;
            return null;
        }

        ///Implements ////<UnaryExpr> ::= <CompareExpr>
        public Reduction CreateRULE_UNARYEXPR(Reduction reduction)
        {
            reduction.Tag = ((Reduction)(reduction.GetToken(0)).Data).Tag;
            return null;
        }

        ///Implements ////<CompareExpr> ::= <ArithExpression> <LogicalOperator> <ArithExpression>
        public Reduction CreateRULE_COMPAREEXPR(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, (Condition)GetReductionTag(reduction, 1));
        }

        ///Implements ////<CompareExpr> ::= <ArithExpression> LIKE <ArithExpression>
        public Reduction CreateRULE_COMPAREEXPR_LIKE(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.Like);
        }

        ///Implements ////<CompareExpr> ::= <ArithExpression> NOT LIKE <ArithExpression>
        public Reduction CreateRULE_COMPAREEXPR_NOT_LIKE(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.NotLike, 3);
        }

        ///Implements////<CompareExpr> ::= <BinaryExpression> CONTAINS ANY <ParamJSONArray>
        public Reduction CreateRULE_COMPAREEXPR_CONTAINS_ANY(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.ContainsAny, 3);
        }

        ///Implements ////<CompareExpr> ::= <BinaryExpression> NOT CONTAINS ANY <ParamJSONArray>
        public Reduction CreateRULE_COMPAREEXPR_NOT_CONTAINS_ANY(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.NotContainsAny, 4);
        }

        ///Implements ////<CompareExpr> ::= <BinaryExpression> CONTAINS ALL <ParamJSONArray>
        public Reduction CreateRULE_COMPAREEXPR_CONTAINS_ALL(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.ContainsAll, 3);
        }

        ///Implements ////<CompareExpr> ::= <BinaryExpression> NOT CONTAINS ALL <ParamJSONArray>
        public Reduction CreateRULE_COMPAREEXPR_NOT_CONTAINS_ALL(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.NotContainsAll, 4);
        }

        ///Implements ////<CompareExpr> ::= <BinaryExpression> ARRAY SIZE <ParamInteger>
        public Reduction CreateRULE_COMPAREEXPR_ARRAY_SIZE(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.ArraySize, 3);
        }

        ///Implements ////<CompareExpr> ::= <BinaryExpression> NOT ARRAY SIZE <ParamInteger>
        public Reduction CreateRULE_COMPAREEXPR_NOT_ARRAY_SIZE(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.NotArraySize, 4);
        }

        ///Implements ////<CompareExpr> ::= <ArithExpression> IN <BinaryExprList>
        public Reduction CreateRULE_COMPAREEXPR_IN(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.In);
        }

        ///Implements ////<CompareExpr> ::= <ArithExpression> NOT IN <BinaryExprList>
        public Reduction CreateRULE_COMPAREEXPR_NOT_IN(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.NotIn, 3);
        }

        ///Implements ////<CompareExpr> ::= <BinaryExpression> BETWEEN <BinaryExpression> AND <BinaryExpression>
        public Reduction CreateRULE_COMPAREEXPR_BETWEEN_AND(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            Reduction midReduction = GetReduction(reduction, 2);
            Reduction rhsReduction = GetReduction(reduction, 4);

            reduction.Tag = new ComparisonPredicate(GetEvaluable(lhsReduction),
                Condition.Between, GetEvaluable(midReduction), GetEvaluable(rhsReduction));
            return null;
        }

        ///Implements <BinaryExpression> NOT BETWEEN <BinaryExpression> AND <BinaryExpression>
        public Reduction CreateRULE_COMPAREEXPR_NOT_BETWEEN_AND(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            Reduction midReduction = GetReduction(reduction, 3);
            Reduction rhsReduction = GetReduction(reduction, 5);

            reduction.Tag = new ComparisonPredicate(GetEvaluable(lhsReduction),
                Condition.NotBetween, GetEvaluable(midReduction), GetEvaluable(rhsReduction));
            return null;
        }

        ///Implements ////<CompareExpr> ::= <BinaryExpression> EXISTS
        public Reduction CreateRULE_COMPAREEXPR_EXISTS(Reduction reduction)
        {
            return AssignComparisonExpressionUniary(reduction, Condition.Exists);
        }

        ///Implements ////<CompareExpr> ::= <BinaryExpression> NOT EXISTS
        public Reduction CreateRULE_COMPAREEXPR_NOT_EXISTS(Reduction reduction)
        {
            return AssignComparisonExpressionUniary(reduction, Condition.NotExists);
        }

        ///Implements ////<CompareExpr> ::= <ArithExpression> IS NULL
        public Reduction CreateRULE_COMPAREEXPR_IS_NULL(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.IsNull, NullValue.Null);
        }

        ///Implements ////<CompareExpr> ::= <ArithExpression> IS NOT NULL
        public Reduction CreateRULE_COMPAREEXPR_IS_NOT_NULL(Reduction reduction)
        {
            return AssignComparisonExpressionBinary(reduction, Condition.IsNotNull, NullValue.Null);
        }

        ///Implements ////<CompareExpr> ::= '(' <Expression> ')'
        public Reduction CreateRULE_COMPAREEXPR_LPARAN_RPARAN(Reduction reduction)
        {
            ITreePredicate expression = GetReductionTag(reduction, 1) as ITreePredicate;
            if (expression != null)
            {
                expression.Completed = true;
            }
            reduction.Tag = expression;
            return null;
        }

        ///Implements ////<LogicalOperator> ::= '='
        public Reduction CreateRULE_LOGICALOPERATOR_EQ(Reduction reduction)
        {
            reduction.Tag = Condition.Equals;
            return null;
        }

        ///Implements ////<LogicalOperator> ::= '!='
        public Reduction CreateRULE_LOGICALOPERATOR_EXCLAMEQ(Reduction reduction)
        {
            reduction.Tag = Condition.NotEquals;
            return null;
        }

        ///Implements ////<LogicalOperator> ::= '=='
        public Reduction CreateRULE_LOGICALOPERATOR_EQEQ(Reduction reduction)
        {
            reduction.Tag = Condition.Equals;
            return null;
        }

        ///Implements ////<LogicalOperator> ::= '<>'
        public Reduction CreateRULE_LOGICALOPERATOR_LTGT(Reduction reduction)
        {
            reduction.Tag = Condition.NotEquals;
            return null;
        }

        ///Implements ////<LogicalOperator> ::= '<'
        public Reduction CreateRULE_LOGICALOPERATOR_LT(Reduction reduction)
        {
            reduction.Tag = Condition.LesserThan;
            return null;
        }

        ///Implements ////<LogicalOperator> ::= '>'
        public Reduction CreateRULE_LOGICALOPERATOR_GT(Reduction reduction)
        {
            reduction.Tag = Condition.GreaterThan;
            return null;
        }

        ///Implements ////<LogicalOperator> ::= '<='
        public Reduction CreateRULE_LOGICALOPERATOR_LTEQ(Reduction reduction)
        {
            reduction.Tag = Condition.LesserThanEqualTo;
            return null;
        }

        ///Implements ////<LogicalOperator> ::= '>='
        public Reduction CreateRULE_LOGICALOPERATOR_GTEQ(Reduction reduction)
        {
            reduction.Tag = Condition.GreaterThanEqualTo;
            return null;
        }

        ///Implements  ////<TopSection> ::= TOP '(' IntegerLiteral ')'
        public Reduction CreateRULE_TOPSECTION_TOP_LPARAN_INTEGERLITERAL_RPARAN(Reduction reduction)
        {
            reduction.Tag = new IntegerConstantValue(GetReductionDataString(reduction, 2));
            return null;
        }

        ///Implements  ////<TopSection> ::= TOP IntegerLiteral
        public Reduction CreateRULE_TOPSECTION_TOP_INTEGERLITERAL(Reduction reduction)
        {
            reduction.Tag = new IntegerConstantValue(GetReductionDataString(reduction, 1));
            return null;
        }

        ///Implements  ////<TopSection> ::= 
        public Reduction CreateRULE_TOPSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<BinaryExpression> ::= <AddExpression>
        public Reduction CreateRULE_BINARYEXPRESSION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<FromSection> ::= FROM <CollectionName>
        public Reduction CreateRULE_FROMSECTION_FROM(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 1);
            return null;
        }

        ///Implements  ////<SelectFromSection> ::= <FromSection>
        public Reduction CreateRULE_SELECTFROMSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<OptTerminator> ::= ';'
        public Reduction CreateRULE_OPTTERMINATOR_SEMI(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<OptTerminator> ::= 
        public Reduction CreateRULE_OPTTERMINATOR(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<QueryValues> ::= <Attribute> '=' <BinaryExpression> ',' <QueryValues>
        public Reduction CreateRULE_QUERYVALUES_EQ_COMMA(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 0);
            Reduction valueReduction = GetReduction(reduction, 2);
            Updator updator = GetReductionTag(reduction, 4) as Updator;

            if (updator != null)
            {
                if (attribute is string)
                {
                    attribute = new Attribute(attribute as string);
                }

                if (valueReduction.Tag is string)
                {
                    valueReduction.Tag = GetEvaluable(valueReduction);
                }
                updator.CreateUpdation((IEvaluable)attribute, (IEvaluable)valueReduction.Tag);
            }

            reduction.Tag = updator;
            return null;
        }
        
        ///Implements ////<QueryValues> ::= <Attribute> <ArrayUpdateOp> <BinaryExprList> ',' <QueryValues>
        public Reduction CreateRULE_QUERYVALUES_COMMA(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 0);

            ArrayUpdateOption updateOpt =
                (ArrayUpdateOption)GetReductionTag(reduction, 1);

            ValueList values = GetReductionTag(reduction, 2) as ValueList;

            Updator updator = GetReductionTag(reduction, 4) as Updator;

            if (updator != null)
            {
                if (attribute is string)
                {
                    attribute = new Attribute(attribute as string);
                }

                if (values != null)
                {
                    switch (updateOpt)
                    {
                        case ArrayUpdateOption.Add:
                            updator.CreateArrayAddition((IEvaluable)attribute, values.ToArray());
                            break;

                        case ArrayUpdateOption.Insert:
                            updator.CreateArrayInsertion((IEvaluable)attribute, values.ToArray());
                            break;

                        case ArrayUpdateOption.Remove:
                            updator.CreateArrayRemoval((IEvaluable)attribute, values.ToArray());
                            break;
                    }
                }
            }

            reduction.Tag = updator;
            return null;
        }
        
        ///Implements ////<QueryValues> ::= <Attribute> REPLACE '(' <ReplaceList> ')' ',' <QueryValues>
        public Reduction CreateRULE_QUERYVALUES_REPLACE_LPARAN_RPARAN_COMMA(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 0);

            var values = (GetReductionTag(reduction, 3)
                as List<KeyValuePair<IEvaluable, IEvaluable>>);
            
            Updator updator = GetReductionTag(reduction, 6) as Updator;

            if (updator != null)
            {
                if (attribute is string)
                {
                    attribute = new Attribute(attribute as string);
                }

                if (values != null)
                {
                    updator.CreateArrayReplacement((IEvaluable)attribute, values.ToArray());
                }
            }

            reduction.Tag = updator;
            return null;
        }

        ///Implements ////<QueryValues> ::= RENAME <Attribute> TO <StrLiteral> ',' <QueryValues>
        public Reduction CreateRULE_QUERYVALUES_RENAME_TO_COMMA(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 1);
            object name = GetReductionTag(reduction, 3);
            Updator updator = GetReductionTag(reduction, 5) as Updator;

            if (updator != null)
            {
                if (attribute is string)
                {
                    attribute = new Attribute(attribute as string);
                }

                if (name != null)
                {
                    updator.CreateAttributeRenaming((IEvaluable)attribute, (IEvaluable)name);
                }
            }

            reduction.Tag = updator;
            return null;
        }

        ///Implements ////<QueryValues> ::= DELETE <Attribute> ',' <QueryValues>
        public Reduction CreateRULE_QUERYVALUES_DELETE_COMMA(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 1);
            Updator updator = GetReductionTag(reduction, 3) as Updator;

            if (updator != null)
            {
                if (attribute is string)
                {
                    attribute = new Attribute(attribute as string);
                }

                updator.CreateAttributeDeletion((IEvaluable) attribute);
            }

            reduction.Tag = updator;
            return null;
        }

        ///Implements ////<QueryValues> ::= <Attribute> '=' <BinaryExpression>
        public Reduction CreateRULE_QUERYVALUES_EQ(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 0);
            Reduction valueReduction = GetReduction(reduction, 2);

            if (attribute is string)
            {
                attribute = new Attribute(attribute as string);
            }

            if (valueReduction.Tag is string)
            {
                valueReduction.Tag = GetEvaluable(valueReduction);
            }

            Updator updator = new Updator();
            updator.CreateUpdation((IEvaluable)attribute, (IEvaluable)valueReduction.Tag);
            reduction.Tag = updator;
            return null;
        }

        ///Implements ////<QueryValues> ::= <Attribute> <ArrayUpdateOp> <BinaryExprList>
        public Reduction CreateRULE_QUERYVALUES(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 0);

            ArrayUpdateOption updateOpt = 
                (ArrayUpdateOption)GetReductionTag(reduction, 1);
            
            ValueList values = GetReductionTag(reduction, 2) as ValueList;
            
            if (attribute is string)
            {
                attribute = new Attribute(attribute as string);
            }

            Updator updator = new Updator();

            if (values != null)
            {
                switch (updateOpt)
                {
                    //Reverse Order of Values to maintain order
                    case ArrayUpdateOption.Add:
                        updator.CreateArrayAddition((IEvaluable)attribute, values.Reverse().ToArray());
                        break;

                    case ArrayUpdateOption.Insert:
                        updator.CreateArrayInsertion((IEvaluable)attribute, values.Reverse().ToArray());
                        break;

                    case ArrayUpdateOption.Remove:
                        updator.CreateArrayRemoval((IEvaluable)attribute, values.Reverse().ToArray());
                        break;
                }
            }

            reduction.Tag = updator;
            return null;
        }
        

        ///Implements ////<QueryValues> ::= <Attribute> REPLACE '(' <ReplaceList> ')'
        public Reduction CreateRULE_QUERYVALUES_REPLACE_LPARAN_RPARAN(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 0);

            List<KeyValuePair<IEvaluable, IEvaluable>> values =
                (GetReductionTag(reduction, 3) as List<KeyValuePair<IEvaluable, IEvaluable>>);

            if (attribute is string)
            {
                attribute = new Attribute(attribute as string);
            }
            Updator updator = new Updator();

            if (values != null)
            {
                updator.CreateArrayReplacement((IEvaluable)attribute, values.ToArray());
            }

            reduction.Tag = updator;
            return null;
        }

        ///Implements ////<QueryValues> ::= RENAME <Attribute> TO <StrLiteral>
        public Reduction CreateRULE_QUERYVALUES_RENAME_TO(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 1);
            object name = GetReductionTag(reduction, 3);

            if (attribute is string)
            {
                attribute = new Attribute(attribute as string);
            }

            Updator updator = new Updator();

            if (name != null)
            {
                updator.CreateAttributeRenaming((IEvaluable)attribute, (IEvaluable) name);
            }

            reduction.Tag = updator;
            return null;
        }

        ///Implements ////<QueryValues> ::= DELETE <Attribute>
        public Reduction CreateRULE_QUERYVALUES_DELETE(Reduction reduction)
        {
            object attribute = GetReductionTag(reduction, 1);

            if (attribute is string)
            {
                attribute = new Attribute(attribute as string);
            }

            Updator updator = new Updator();
            updator.CreateAttributeDeletion((IEvaluable)attribute);

            reduction.Tag = updator;
            return null;
        }


        ///Implements ////<ExecuteStatement> ::= EXECUTE '(' <FunctionName> ')' <OptTerminator>
        public Reduction CreateRULE_EXECUTESTATEMENT_EXECUTE_LPARAN_RPARAN(Reduction reduction)
        {
            reduction.Tag = new Function(GetReductionTag(reduction, 2) as string);
            return null;
        }

        ///Implements ////<ExecuteStatement> ::= EXEC '(' <FunctionName> ')' <OptTerminator>
        public Reduction CreateRULE_EXECUTESTATEMENT_EXEC_LPARAN_RPARAN(Reduction reduction)
        {
            return CreateRULE_EXECUTESTATEMENT_EXECUTE_LPARAN_RPARAN(reduction);
        }

        ///Implements ////<SelectQuery> ::= SELECT <SelectAttributes> <SelectFromSection> <SelectWhereSection> <GroupSection> <HavingSection> <OrderSection> <SkipSection> <LimitSection> <HintSection>  <OptTerminator>
        public Reduction CreateRULE_SELECTQUERY_SELECT(Reduction reduction)
        {
            Projection projection = (Projection)GetReductionTag(reduction, 1);

            string collection = GetReductionTag(reduction, 2) as string;
            ITreePredicate filterPredicate = GetReductionTag(reduction, 3) as ITreePredicate;
            List<IEvaluable> groupBy = GetReductionTag(reduction, 4) as List<IEvaluable>;
            List<IEvaluable> orderBy = GetReductionTag(reduction, 6) as List<IEvaluable>;

            Offset offset = (Offset)GetReductionTag(reduction, 7);

            if (projection.Limit != null && offset != null)
            {
                throw new QuerySystemException(ErrorCodes.Query.INVALID_SYNTAX, new[] { "TOP", "1" });
            }

            string hint = GetReductionTag(reduction, 8) as string;

            if(groupBy != null && groupBy.Count > 0)
            {
                if (projection.Evaluables.Count == 1 && projection.Evaluables[0] is AllEvaluable)
                {
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_SYNTAX, new[] { "*", "1" });
                }
            }

            IntegerConstantValue limit = null;

            if (projection.Limit != null)
            {
                limit = projection.Limit;
            }
            else if (offset != null)
            {
                limit = offset.Limit;
            }

            reduction.Tag = new SelectObject(projection.Distinct, projection.Evaluables,
                collection, filterPredicate, groupBy, orderBy, offset != null ? offset.Skip : null, limit, hint);
            
            return null;
        }

        ///Implements ////<SelectAttributes> ::= <DistinctRestriction> <TopSection> <AsExpressions>
        public Reduction CreateRULE_SELECTATTRIBUTES(Reduction reduction)
        {
            Reduction expReduction = GetReduction(reduction, 2);

            if (expReduction.Tag is string)
            {
                expReduction.Tag = GetEvaluable(expReduction);
            }

            if (!(expReduction.Tag is IList))
            {
                expReduction.Tag = new List<IEvaluable> { (IEvaluable)expReduction.Tag };
            }

            reduction.Tag = new Projection
            {
                Distinct = GetReductionTag<bool>(reduction, 0),
                Limit = GetReductionTag<IntegerConstantValue>(reduction, 1),
                Evaluables = (List<IEvaluable>)expReduction.Tag,
            };
            return null;
        }

        ///Implements ////<SelectWhereSection> ::= <WhereSection>
        public Reduction CreateRULE_SELECTWHERESECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<GroupSection> ::= GROUP BY <OrderedExpressionList>
        public Reduction CreateRULE_GROUPSECTION_GROUP_BY(Reduction reduction)
        {
            return CreateRULE_ORDERSECTION_ORDER_BY(reduction);
        }

        ///Implements ////<GroupSection> ::= 
        public Reduction CreateRULE_GROUPSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<HavingSection> ::= HAVING <Function> <LogicalOperator> <Value>
        public Reduction CreateRULE_HAVINGSECTION_HAVING(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<HavingSection> ::= 
        public Reduction CreateRULE_HAVINGSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<OrderSection> ::= ORDER BY <OrderedExpressionList>
        public Reduction CreateRULE_ORDERSECTION_ORDER_BY(Reduction reduction)
        {
            object value = GetReductionTag(reduction, 2);
            if (!(value is IList))
            {
                value = new List<IEvaluable>() { (IEvaluable)value };
            }
            reduction.Tag = value;
            return null;
        }

        ///Implements ////<OrderSection> ::= 
        public Reduction CreateRULE_ORDERSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<LimitSection> ::= LIMIT IntegerLiteral
        public Reduction CreateRULE_LIMITSECTION_LIMIT_INTEGERLITERAL(Reduction reduction)
        {
            reduction.Tag = new IntegerConstantValue(GetReductionDataString(reduction, 1));
            return null;
        }

        ///Implements ////<LimitSection> ::= 
        public Reduction CreateRULE_LIMITSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////////<SkipSection> ::= SKIP IntegerLiteral
        internal Reduction CreateRULE_SKIPSECTION_SKIP_INTEGERLITERAL(Reduction reduction)
        {
            reduction.Tag = new IntegerConstantValue(GetReductionDataString(reduction, 1));
            return null;
        }

        ///Implements ////////<SkipSection> ::= 
        internal Reduction CreateRULE_SKIPSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<OffsetSection> ::= OFFSET IntegerLiteral ROWS FETCH NEXT IntegerLiteral ROWS ONLY
        public Reduction CreateRULE_OFFSETSECTION_OFFSET_INTEGERLITERAL_ROWS_FETCH_NEXT_INTEGERLITERAL_ROWS_ONLY(Reduction reduction)
        {
            reduction.Tag = new Offset
            {
                Skip = new IntegerConstantValue(GetReductionDataString(reduction, 1)),
                Limit = new IntegerConstantValue(GetReductionDataString(reduction, 5))
            };
            return null;
        }

        ///Implements ////<OffsetSection> ::= OFFSET IntegerLiteral ROWS
        public Reduction CreateRULE_OFFSETSECTION_OFFSET_INTEGERLITERAL_ROWS(Reduction reduction)
        {
            reduction.Tag = new Offset
            {
                Skip = new IntegerConstantValue(GetReductionDataString(reduction, 1))
            };
            return null;
        }

        ///Implements ////<OffsetSection> ::= 
        public Reduction CreateRULE_OFFSETSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<QueryValuesOptBrk> ::= '(' <QueryValues> ')'
        public Reduction CreateRULE_QUERYVALUESOPTBRK_LPARAN_RPARAN(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 1) as Updator;
            return null;
        }

        ///Implements ////<QueryValuesOptBrk> ::= <QueryValues>
        public Reduction CreateRULE_QUERYVALUESOPTBRK(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0) as Updator;
            return null;
        }


        ///Implements ////<UpdateQuery> ::= UPDATE <CollectionName> SET <QueryValuesOptBrk> <UpdateWhereSection>
        public Reduction CreateRULE_UPDATEQUERY_UPDATE_SET(Reduction reduction)
        {
            string collection = GetReductionTag(reduction, 1) as string;
            Updator updator = GetReductionTag(reduction, 3) as Updator;
            ITreePredicate whereExpression = GetReductionTag(reduction, 4) as ITreePredicate;

            reduction.Tag = new UpdateObject(collection, updator, whereExpression);
            return null;
        }

        ///Implements ////<InsertQuery> ::= INSERT INTO <CollectionName> <AtrList> VALUES <JSONValList> <OptTerminator>
        public Reduction CreateRULE_INSERTQUERY_INSERT_INTO_VALUES(Reduction reduction)
        {
            string intoCollection = GetReductionTag(reduction, 2) as string;
            object attributes = GetReductionTag(reduction, 3);
            Reduction valuesReduction = GetReduction(reduction, 5);

            if (attributes is string)
            {
                attributes = new List<Attribute>() { new Attribute(attributes.ToString()) };
            }

            if (valuesReduction.Tag is string)
            {
                valuesReduction.Tag = new ValueList() { GetEvaluable(valuesReduction) }; 
            }

            List<Attribute> attributesList = (List<Attribute>)attributes;
            ValueList valuesList = (ValueList)valuesReduction.Tag;

            if (!attributesList.Count.Equals(valuesList.Count))
            {
                throw new QuerySystemException(ErrorCodes.Query.INVALID_NUMBER_OF_INSERT_PARAMETERS);
            }

            HashSet<string> uniqueSet = new HashSet<string>();
            foreach (var attribute in attributesList)
            {
                if (attribute.IsMultiLevel)
                {
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_INSERT_QUERY_ATTRIBUTE, new[] { attribute.ToString() });
                }
                if (uniqueSet.Contains(attribute.ToString()))
                {
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_INSERT_QUERY_ATTRIBUTE_CONFLICT, new[] { attribute.ToString() });
                }
                uniqueSet.Add(attribute.ToString());
            }

            foreach (var value in valuesList)
            {
                if (!value.EvaluationType.Equals(EvaluationType.Constant))
                {
                    throw new QuerySystemException(ErrorCodes.Query.INVALID_INSERT_QUERY_CONSTANT_VALUE, new[] { value.ToString() });
                }
            }

            var attributeValuePairs = new List<KeyValuePair<Attribute, IEvaluable>>();
            for (int i = 0; i < attributesList.Count; i++)
            {
                attributeValuePairs.Add(new KeyValuePair<Attribute, IEvaluable>(attributesList[i], valuesList[i]));
            }

            reduction.Tag = new InsertObject(intoCollection, attributeValuePairs);
            return null;
        }

        ///Implements ////<UpdateWhereSection> ::= <WhereSection>
        public Reduction CreateRULE_UPDATEWHERESECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DeleteQuery> ::= DELETE <DeleteFromSection> <DeleteWhereSection> <OptTerminator>
        public Reduction CreateRULE_DELETEQUERY_DELETE(Reduction reduction)
        {
            string fromCollection = GetReductionTag(reduction, 1) as string;
            ITreePredicate whereExpression = GetReductionTag(reduction, 2) as ITreePredicate;

            reduction.Tag = new DeleteObject(fromCollection, whereExpression);
            return null;
        }

        ///Implements ////<DeleteFromSection> ::= <FromSection>
        public Reduction CreateRULE_DELETEFROMSECTION(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<DeleteWhereSection> ::= <WhereSection>
        public Reduction CreateRULE_DELETEWHERESECTION(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 0);
            return null;
        }

        ///Implements ////<HintParameter> ::= <StrLiteral>
        internal Reduction CreateRULE_HINTPARAMETER(Reduction reduction)
        {
            return null;
        }
        
        ///Implements ////<HintSection> ::= HINT <HintParameter>
        internal Reduction CreateRULE_HINTSECTION_HINT(Reduction reduction)
        {
            reduction.Tag = GetReductionTag(reduction, 1) as string;
            return null;
        }

        ///Implements ////<HintSection> ::=
        internal Reduction CreateRULE_HINTSECTION(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<OrderedExpression> ::= <BinaryExpression> <Order>
        internal Reduction CreateRULE_ORDEREDEXPRESSION(Reduction reduction)
        {
            Reduction expressionReduction = GetReduction(reduction, 0);
            object orderObject = GetReductionTag(reduction, 1);

            if (expressionReduction.Tag is string)
            {
                expressionReduction.Tag = GetEvaluable(expressionReduction);
            }

            if (!(expressionReduction.Tag is BinaryExpression))
            {
                expressionReduction.Tag = new BinaryExpression((IEvaluable)expressionReduction.Tag);
            }

            if (orderObject != null)
            {
                ((BinaryExpression)expressionReduction.Tag).SortOrder = (SortOrder)orderObject;
            }

            reduction.Tag = expressionReduction.Tag;
            return null;
        }

        ///Implements ////<OrderedExpressionList> ::= <OrderedExpressionList> ',' <OrderedExpression>
        internal Reduction CreateRULE_ORDEREDEXPRESSIONLIST_COMMA(Reduction reduction)
        {
            object expressions = GetReductionTag(reduction, 0);
            object expression = GetReductionTag(reduction, 2);

            if (expressions is IList)
            {
                ((List<IEvaluable>)expressions).Add((IEvaluable)expression);
            }
            else
            {
                expressions = new List<IEvaluable>() { (IEvaluable)expressions, (IEvaluable)expression };
            }

            reduction.Tag = expressions;
            return null;
        }

        ///Implements ////<OrderedExpressionList> ::= <OrderedExpression>
        internal Reduction CreateRULE_ORDEREDEXPRESSIONLIST(Reduction reduction)
        {
            return null;
        }

        #region JSON parsing section

        ///Implements ////<JSONString> ::= <JSONObject>
        public Reduction CreateRULE_JSONSTRING(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<JSONString> ::= <JSONArray>
        public Reduction CreateRULE_JSONSTRING2(Reduction reduction)
        {
            return null;
        }

        #region DDL parsing section.

        ///Implements ////<DDLIdentifierConfig> ::= <Identifier> <JSONObject>
        public Reduction CreateRULE_DDLIDENTIFIERCONFIG(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 1);
            reduction.Tag = new DdlConfiguration(lhs as string,
                rhs as DocumentEvaluable);
            return null;
        }
        
        ///Implements ////<DDLConfiguration> ::= <Identifier>
        public Reduction CreateRULE_DDLCONFIGURATION(Reduction reduction)
        {
            return null;
        }


        ///Implements ////<DDLConfiguration> ::= <DDLIdentifierConfig>
        public Reduction CreateRULE_DDLCONFIGURATION2(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<DDLConfiguration> ::= <JSONObject>
        public Reduction CreateRULE_DDLCONFIGURATION3(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<ObjectType> ::= DATABASE
        public Reduction CreateRULE_OBJECTTYPE_DATABASE(Reduction reduction)
        {
            reduction.Tag = DbObjectType.Database;
            return null;
        }
        
        ///Implements ////<ObjectType> ::= COLLECTION
        public Reduction CreateRULE_OBJECTTYPE_COLLECTION(Reduction reduction)
        {
            reduction.Tag = DbObjectType.Collection;
            return null;
        }

        ///Implements ////<ObjectType> ::= INDEX
        public Reduction CreateRULE_OBJECTTYPE_INDEX(Reduction reduction)
        {
            reduction.Tag = DbObjectType.Index;
            return null;
        }

        ///Implements ////<ObjectType> ::= TRIGGER
        public Reduction CreateRULE_OBJECTTYPE_TRIGGER(Reduction reduction)
        {
            reduction.Tag = DbObjectType.Trigger;
            return null;
        }

        ///Implements ////<ObjectType> ::= FUNCTION
        public Reduction CreateRULE_OBJECTTYPE_FUNCTION(Reduction reduction)
        {
            reduction.Tag = DbObjectType.Function;
            return null;
        }

        ///Implements ////<ObjectType> ::= LOGIN
        public Reduction CreateRULE_OBJECTTYPE_LOGIN(Reduction reduction)
        {
            reduction.Tag = DbObjectType.Login;
            return null;
        }

        ///Implements ////<ObjectType> ::= USER
        public Reduction CreateRULE_OBJECTTYPE_USER(Reduction reduction)
        {
            reduction.Tag = DbObjectType.User;
            return null;
        }

        ///Implements ////<ObjectType> ::= ROLE
        public Reduction CreateRULE_OBJECTTYPE_ROLE(Reduction reduction)
        {
            reduction.Tag = DbObjectType.Role;
            return null;
        }

        ///Implements ////<ObjectType> ::= MASTER KEY
        public Reduction CreateRULE_OBJECTTYPE_MASTER_KEY(Reduction reduction)
        {
            reduction.Tag = DbObjectType.MasterKey;
            return null;
        }

        ///Implements ////<CreateStatement> ::= CREATE <ObjectType> <DDLConfiguration>
        public Reduction CreateRULE_CREATESTATEMENT_CREATE(Reduction reduction)
        {
            object conf = GetReductionTag(reduction, 2);
            if (conf is string)
            {
                conf = new DdlConfiguration(conf as string);
            }
            else if (conf is DocumentEvaluable)
            {
                conf = new DdlConfiguration(conf as DocumentEvaluable);
            }
            reduction.Tag = new DataDefinitionObject(DataDefinitionType.Create,
                (DbObjectType)GetReductionTag(reduction, 1), conf as DdlConfiguration);
            return null;
        }

        ///Implements ////<AlterStatement> ::= ALTER <ObjectType> <DDLConfiguration>
        public Reduction CreateRULE_ALTERSTATEMENT_ALTER(Reduction reduction)
        {
            object conf = GetReductionTag(reduction, 2);
            if (conf is string)
            {
                conf = new DdlConfiguration(conf as string);
            }
            else if (conf is DocumentEvaluable)
            {
                conf = new DdlConfiguration(conf as DocumentEvaluable);
            }
            reduction.Tag = new DataDefinitionObject(DataDefinitionType.Alter,
                (DbObjectType)GetReductionTag(reduction, 1), conf as DdlConfiguration);
            return null;
        }

        ///Implements ////<DropStatement> ::= DROP <ObjectType> <DDLConfiguration>
        public Reduction CreateRULE_DROPSTATEMENT_DROP(Reduction reduction)
        {
            object conf = GetReductionTag(reduction, 2);
            if (conf is string)
            {
                conf = new DdlConfiguration(conf as string);
            }
            else if (conf is DocumentEvaluable)
            {
                conf = new DdlConfiguration(conf as DocumentEvaluable);
            }
            reduction.Tag = new DataDefinitionObject(DataDefinitionType.Drop,
                (DbObjectType)GetReductionTag(reduction, 1),conf as DdlConfiguration);
            return null;
        }

        ///Implements ////<TruncateStatement> ::= TRUNCATE COLLECTION <CollectionName>
        public Reduction CreateRULE_TRUNCATESTATEMENT_TRUNCATE_COLLECTION(Reduction reduction)
        {
            reduction.Tag = new DataDefinitionObject(DataDefinitionType.Truncate, DbObjectType.Collection,
                GetReductionTag(reduction, 2) as DdlConfiguration);
            return null;

        }

        ///Implements ////<BackupStatement> ::= BACKUP DATABASE <DDLIdentifierConfig>
        public Reduction CreateRULE_BACKUPSTATEMENT_BACKUP_DATABASE(Reduction reduction)
        {
            reduction.Tag = new DataDefinitionObject(DataDefinitionType.Backup, DbObjectType.Database,
                GetReductionTag(reduction, 2) as DdlConfiguration);
            return null;

        }

        ///Implements ////<RestoreStatement> ::= RESTORE DATABASE <DDLIdentifierConfig>
        public Reduction CreateRULE_RESTORESTATEMENT_RESTORE_DATABASE(Reduction reduction)
        {
            reduction.Tag = new DataDefinitionObject(DataDefinitionType.Restore, DbObjectType.Database,
                GetReductionTag(reduction, 2) as DdlConfiguration);
            return null;

        }

        ///Implements ////<ControlStatement> ::= <EnableStatement>
        public Reduction CreateRULE_CONTROLSTATEMENT(Reduction reduction)
        {

            return null;
        }

        ///Implements ////<ControlStatement> ::= <DisableStatement>
        public Reduction CreateRULE_CONTROLSTATEMENT2(Reduction reduction)
        {

            return null;
        }

        ///Implements ////<DisableStatement> ::= 
        public Reduction CreateRULE_DISABLESTATEMENT(Reduction reduction)
        {
            reduction.Tag = new DataDefinitionObject(DataDefinitionType.Disable, DbObjectType.Trigger,
                 GetReductionTag(reduction, 2) as DdlConfiguration);
            return null;
        }

        ///Implements ////<EnableStatement> ::= 
        public Reduction CreateRULE_ENABLESTATEMENT(Reduction reduction)
        {
            reduction.Tag = new DataDefinitionObject(DataDefinitionType.Enable, DbObjectType.Trigger,
                 GetReductionTag(reduction, 2) as DdlConfiguration);
            return null;
        }

        ///Implements ////<DCLObject> ::= <Identifier>
        public Reduction CreateRULE_DCLOBJECT(Reduction reduction){
           
            return null;
        }

        ///Implements ////<DCLObject> ::= <Identifier> '.' <Identifier>
        public Reduction CreateRULE_DCLOBJECT_DOT(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 2);
            reduction.Tag = new Tuple<string, string>
                (lhs as string, rhs as string);
            return null;
        }

        ///Implements ////<GrantStatement> ::= GRANT <Identifier> ON <DCLObject> TO <StrLiteral>
        public Reduction CreateRULE_GRANTSTATEMENT_GRANT_ON_TO(Reduction reduction)
        {
            string roleName = GetReductionTag(reduction, 1) as string;
            StringConstantValue userName = GetReductionTag(reduction, 5) as StringConstantValue;
            object controlObject = GetReductionTag(reduction, 3);
            if (controlObject is Tuple<string, string>)
            {
                reduction.Tag = new DataControlObject(ControlType.Grant,
                    ((Tuple<string, string>)controlObject).Item1,
                    ((Tuple<string, string>)controlObject).Item2, userName.Value as string, roleName);
            }
            else if (controlObject is string)
            {
                reduction.Tag = new DataControlObject(ControlType.Grant,
                    controlObject as string, null, userName.Value as string, roleName);
            }
            return null;
        }

        ///Implements ////<RevokeStatement> ::= REVOKE <Identifier> ON <DCLObject> FROM <StrLiteral>
        public Reduction CreateRULE_REVOKESTATEMENT_REVOKE_ON_FROM(Reduction reduction)
        {
            string roleName = GetReductionTag(reduction, 1) as string;
            StringConstantValue userName = GetReductionTag(reduction, 5) as StringConstantValue;
            object controlObject = GetReductionTag(reduction, 3);
            if (controlObject is Tuple<string, string>)
            {
                reduction.Tag = new DataControlObject(ControlType.Revoke,
                    ((Tuple<string, string>)controlObject).Item1,
                    ((Tuple<string, string>)controlObject).Item2, userName.Value as string, roleName);
            }
            else if (controlObject is string)
            {
                reduction.Tag = new DataControlObject(ControlType.Revoke,
                    controlObject as string, null, userName.Value as string, roleName);
            }
            return null;
        }

        #endregion

        ///Implements ////<ArrayUpdateOp> ::= ADD
        public Reduction CreateRULE_ARRAYUPDATEOP_ADD(Reduction reduction)
        {
            reduction.Tag = ArrayUpdateOption.Add;
            return null;
        }

        ///Implements ////<ArrayUpdateOp> ::= INSERT
        public Reduction CreateRULE_ARRAYUPDATEOP_INSERT(Reduction reduction)
        {
            reduction.Tag = ArrayUpdateOption.Insert;
            return null;
        }

        ///Implements ////<ArrayUpdateOp> ::= REMOVE
        public Reduction CreateRULE_ARRAYUPDATEOP_REMOVE(Reduction reduction)
        {
            reduction.Tag = ArrayUpdateOption.Remove;
            return null;
        }

        ///Implements ////<JSONObject> ::= '{' '}'
        public Reduction CreateRULE_JSONOBJECT_LBRACE_RBRACE(Reduction reduction)
        {
            reduction.Tag = new DocumentEvaluable();
            return null;
        }

        ///Implements ////<JSONObject> ::= '{' <JSONMembers> '}'
        public Reduction CreateRULE_JSONOBJECT_LBRACE_RBRACE2(Reduction reduction)
        {
            object value = GetReductionTag(reduction, 1);
            DocumentEvaluable document;
            if (!(value is DocumentEvaluable))
            {
                document = new DocumentEvaluable();
                document.AddAttributeValue((KeyValuePair<string, IEvaluable>)value);
            }
            else
            {
                document = (DocumentEvaluable)value;
            }
            reduction.Tag = document;
            return null;
        }

        ///Implements ////<JSONMembers> ::= <AttributePair>
        public Reduction CreateRULE_JSONMEMBERS(Reduction reduction)
        {
            return null;
        }

        ///Implements  ////<JSONMembers> ::= <AttributePair> ',' <JSONMembers>
        public Reduction CreateRULE_JSONMEMBERS_COMMA(Reduction reduction)
        {
            object lhs = GetReductionTag(reduction, 0);
            object rhs = GetReductionTag(reduction, 2);
            DocumentEvaluable document;
            if (!(rhs is DocumentEvaluable))
            {
                document = new DocumentEvaluable();
                document.AddAttributeValue((KeyValuePair<string, IEvaluable>)lhs);
                document.AddAttributeValue((KeyValuePair<string, IEvaluable>)rhs);
            }
            else
            {
                document = (DocumentEvaluable)rhs;
                document.AddAttributeValue((KeyValuePair<string, IEvaluable>)lhs);
            }
            reduction.Tag = document;
            return null;
        }

        ///Implements ////<AttributePair> ::= <StrLiteral> ':' <JSONValue>
        public Reduction CreateRULE_ATTRIBUTEPAIR_COLON(Reduction reduction)
        {
            string lhs = GetReductionTag(reduction, 0) as string;
            Reduction rhsReduction = GetReduction(reduction, 2);
            if (rhsReduction.Tag is string)
            {
                //If the parent rule is "DelimitIdQuotes", the user wanted to specify a string in here.
                rhsReduction.Tag = (rhsReduction.ParentRule.Definition.Equals("DelimitIdQuotes ")) ? 
                    new StringConstantValue(rhsReduction.Tag as string) : GetEvaluable(rhsReduction);
            }
            reduction.Tag = new KeyValuePair<string, IEvaluable>(lhs, (IEvaluable)rhsReduction.Tag);
            return null;
        }

        ///Implements ////<JSONArray> ::= '[' ']'
        public Reduction CreateRULE_JSONARRAY_LBRACKET_RBRACKET(Reduction reduction)
        {
            reduction.Tag = new ArrayEvaluable();
            return null;
        }

        ///Implements ////<JSONArray> ::= '[' <Elements> ']'
        public Reduction CreateRULE_JSONARRAY_LBRACKET_RBRACKET2(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 1);
            if (lhsReduction.Tag is string)
            {
                lhsReduction.Tag = GetEvaluable(lhsReduction);
            }
            if (!(lhsReduction.Tag is ArrayEvaluable))
            {
                IEvaluable temp = (IEvaluable)lhsReduction.Tag;
                lhsReduction.Tag = new ArrayEvaluable();
                ((ArrayEvaluable)lhsReduction.Tag).AddElement(temp);
            }
            reduction.Tag = lhsReduction.Tag;
            return null;
        }

        ///Implements ////<Elements> ::= <JSONValue>
        public Reduction CreateRULE_ELEMENTS(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<Elements> ::= <Elements> ',' <JSONdocValue>
        public Reduction CreateRULE_ELEMENTS_COMMA(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 0);
            Reduction rhsReduction = GetReduction(reduction, 2);

            if (lhsReduction.Tag is string)
            {
                lhsReduction.Tag = GetEvaluable(lhsReduction);
            }
            if (rhsReduction.Tag is string)
            {
                rhsReduction.Tag = GetEvaluable(rhsReduction);
            }

            if (lhsReduction.Tag is ArrayEvaluable)
            {
                ((ArrayEvaluable)lhsReduction.Tag).AddElement((IEvaluable)rhsReduction.Tag);
            }
            else
            {
                var temp = lhsReduction.Tag;
                lhsReduction.Tag = new ArrayEvaluable();
                ((ArrayEvaluable)lhsReduction.Tag).AddElement((IEvaluable)temp);
                ((ArrayEvaluable)lhsReduction.Tag).AddElement((IEvaluable)rhsReduction.Tag);
            }
            reduction.Tag = lhsReduction.Tag;
            return null;
        }

        ///Implements ////<JSONValue> ::= 
        public Reduction CreateRULE_JSONVALUE(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<JSONValue> ::= <Value>
        public Reduction CreateRULE_JSONVALUE2(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<JSONValue> ::= <ArrayProjection>
        public Reduction CreateRULE_JSONVALUE3(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<ArrayProjection> ::= '(' <BinaryExpression> ')' SLICE '(' <ValueSign> IntegerLiteral ',' IntegerLiteral ')'
        public Reduction CreateRULE_ARRAYPROJECTION_LPARAN_RPARAN_SLICE_LPARAN_INTEGERLITERAL_COMMA_INTEGERLITERAL_RPARAN(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 1);
            object sign = GetReductionTag(reduction, 5);
            int start;
            int items;

            string startString = GetReductionDataString(reduction, 6);
            string itemString = GetReductionDataString(reduction, 8);
            if(!Int32.TryParse(startString, out start))
            {
                throw new QuerySystemException(ErrorCodes.Query.INVAILD_ARAY_ITEM, new string[] { startString });
            }
            if (!Int32.TryParse(itemString, out items))
            {
                throw new QuerySystemException(ErrorCodes.Query.INVAILD_ARAY_ITEM, new string[] { itemString });
            }
            
            if(start == 0)
                throw new QuerySystemException(ErrorCodes.Query.INVAILD_ARAY_ITEM, new string[] { startString });
            if (items == 0)
                throw new QuerySystemException(ErrorCodes.Query.INVAILD_ARAY_ITEM, new string[] { itemString });
            
            if (lhsReduction.Tag is string)
            {
                lhsReduction.Tag = GetEvaluable(lhsReduction);
            }

            if (sign != null)
            {
                if (sign.Equals(ArithmeticOperation.Subtraction))
                {
                    start = start*-1;
                }
            }

            reduction.Tag = new IndexedSlicer((IEvaluable) lhsReduction.Tag, start, items);
            return null;
        }

        ///Implements ////<ArrayProjection> ::= '(' <BinaryExpression> ')' SLICE MATCH <BinaryExprList>
        public Reduction CreateRULE_ARRAYPROJECTION_LPARAN_RPARAN_SLICE_MATCH(Reduction reduction)
        {
            Reduction lhsReduction = GetReduction(reduction, 1);
            ValueList values = GetReductionTag(reduction, 5) as ValueList;
           
            if (lhsReduction.Tag is string)
            {
                lhsReduction.Tag = GetEvaluable(lhsReduction);
            }

            reduction.Tag = new ValuedSlicer((IEvaluable)lhsReduction.Tag, values);
            return null;
        }

        ///Implements ////<JSONdocValue> ::= <JSONValue>
        public Reduction CreateRULE_JSONDOCVALUE(Reduction reduction)
        {
            return null;
        }

        ///Implements ////<JSONdocValue> ::= <DelimitIdQuotes>
        public Reduction CreateRULE_JSONDOCVALUE2(Reduction reduction)
        {
            return null;
        }
        
        #endregion

        #endregion
    }
}
