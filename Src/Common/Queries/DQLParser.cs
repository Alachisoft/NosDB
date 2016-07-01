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
using System.Reflection;
using Alachisoft.NosDB.Common.Queries.Parser;
using Alachisoft.NosDB.Core.Queries;

namespace Alachisoft.NosDB.Common.Queries
{
    public class DQLParser : Parser.Parser
    {
        readonly DqlParserRule _parserRule;

        public DQLParser(string resourceName) 
        {
            _parserRule = new DqlParserRule(); 
            
            Assembly assembly = GetType().Assembly;
            Stream s = assembly.GetManifestResourceStream(resourceName);
            
            LoadGrammar(s);            
        }

        public ParseMessage Parse(TextReader Source, bool GenerateContext)
        {
            bool done = false;
            ParseMessage response;
            
            OpenStream(Source);
            TrimReductions = true;

            do
            {
                response = Parse();
                switch (response)
                {
                    case ParseMessage.LexicalError:
                        //Cannot recognize token
                        done = true;
                        break;

                    case ParseMessage.SyntaxError:
                        //Expecting a different token
                        done = true; 
                        break;

                    case ParseMessage.Reduction:
                        //Create a customized object to store the reduction
                        if (GenerateContext)
                            CurrentReduction = CreateNewObject(CurrentReduction);
                        break;

                    case ParseMessage.Accept:
                        //Success!
                        done = true;
                        break;

                    case ParseMessage.TokenRead:
                        //You don't have to do anything here.
                        break;

                    case ParseMessage.InternalError:
                        //INTERNAL ERROR! Something is horribly wrong.
                        done = true;
                        break;

                    case ParseMessage.CommentError:
                        //COMMENT ERROR! Unexpected end of file
                        done = true;
                        break;
                }
            } while (!done);

            CloseFile();
            return response;
        }


        private Reduction CreateNewObject(Reduction reduction)
        {
            Reduction result = null;
            switch ((RuleConstants)System.Enum.ToObject(typeof(RuleConstants), reduction.ParentRule.TableIndex))
            {
                case RuleConstants.RULE_STATEMENT:
                    ////<Statement> ::= <SingleStatment>
                    result = _parserRule.CreateRULE_STATEMENT(reduction);
                    break;
                case RuleConstants.RULE_STATEMENT2:
                    ////<Statement> ::= <MultiStatement>
                    result = _parserRule.CreateRULE_STATEMENT2(reduction);
                    break;
                case RuleConstants.RULE_SINGLESTATMENT:
                    ////<SingleStatment> ::= <SPStatement>
                    result = _parserRule.CreateRULE_SINGLESTATMENT(reduction);
                    break;
                case RuleConstants.RULE_MULTISTATEMENT:
                    ////<MultiStatement> ::= <DMLStatement> <OptTerminator> <MultiStatement>
                    result = _parserRule.CreateRULE_MULTISTATEMENT(reduction);
                    break;
                case RuleConstants.RULE_MULTISTATEMENT2:
                    ////<MultiStatement> ::= <DDLStatement> <OptTerminator> <MultiStatement>
                    result = _parserRule.CreateRULE_MULTISTATEMENT2(reduction);
                    break;
                case RuleConstants.RULE_MULTISTATEMENT3:
                    ////<MultiStatement> ::= <DCLStatement> <OptTerminator> <MultiStatement>
                    result = _parserRule.CreateRULE_MULTISTATEMENT3(reduction);
                    break;
                case RuleConstants.RULE_MULTISTATEMENT4:
                    ////<MultiStatement> ::= <DMLStatement> <OptTerminator>
                    result = _parserRule.CreateRULE_MULTISTATEMENT4(reduction);
                    break;
                case RuleConstants.RULE_MULTISTATEMENT5:
                    ////<MultiStatement> ::= <DDLStatement> <OptTerminator>
                    result = _parserRule.CreateRULE_MULTISTATEMENT5(reduction);
                    break;
                case RuleConstants.RULE_MULTISTATEMENT6:
                    ////<MultiStatement> ::= <DCLStatement> <OptTerminator>
                    result = _parserRule.CreateRULE_MULTISTATEMENT6(reduction);
                    break;
                case RuleConstants.RULE_DMLSTATEMENT:
                    ////<DMLStatement> ::= <SelectQuery>
                    result = _parserRule.CreateRULE_DMLSTATEMENT(reduction);
                    break;
                case RuleConstants.RULE_DMLSTATEMENT2:
                    ////<DMLStatement> ::= <InsertQuery>
                    result = _parserRule.CreateRULE_DMLSTATEMENT2(reduction);
                    break;
                case RuleConstants.RULE_DMLSTATEMENT3:
                    ////<DMLStatement> ::= <UpdateQuery>
                    result = _parserRule.CreateRULE_DMLSTATEMENT3(reduction);
                    break;
                case RuleConstants.RULE_DMLSTATEMENT4:
                    ////<DMLStatement> ::= <DeleteQuery>
                    result = _parserRule.CreateRULE_DMLSTATEMENT4(reduction);
                    break;
                case RuleConstants.RULE_SPSTATEMENT:
                    ////<SPStatement> ::= <ExecuteStatement>
                    result = _parserRule.CreateRULE_SPSTATEMENT(reduction);
                    break;
                case RuleConstants.RULE_DDLSTATEMENT:
                    ////<DDLStatement> ::= <CreateStatement>
                    result = _parserRule.CreateRULE_DDLSTATEMENT(reduction);
                    break;
                case RuleConstants.RULE_DDLSTATEMENT2:
                    ////<DDLStatement> ::= <AlterStatement>
                    result = _parserRule.CreateRULE_DDLSTATEMENT2(reduction);
                    break;
                case RuleConstants.RULE_DDLSTATEMENT3:
                    ////<DDLStatement> ::= <DropStatement>
                    result = _parserRule.CreateRULE_DDLSTATEMENT3(reduction);
                    break;
                case RuleConstants.RULE_DDLSTATEMENT4:
                    ////<DDLStatement> ::= <TruncateStatement>
                    result = _parserRule.CreateRULE_DDLSTATEMENT4(reduction);
                    break;
                case RuleConstants.RULE_DDLSTATEMENT5:
                    ////<DDLStatement> ::= <BackupStatement>
                    result = _parserRule.CreateRULE_DDLSTATEMENT5(reduction);
                    break;
                case RuleConstants.RULE_DDLSTATEMENT6:
                    ////<DDLStatement> ::= <RestoreStatement>
                    result = _parserRule.CreateRULE_DDLSTATEMENT6(reduction);
                    break;
                case RuleConstants.RULE_DCLSTATEMENT:
                    ////<DCLStatement> ::= <GrantStatement>
                    result = _parserRule.CreateRULE_DCLSTATEMENT(reduction);
                    break;
                case RuleConstants.RULE_DCLSTATEMENT2:
                    ////<DCLStatement> ::= <RevokeStatement>
                    result = _parserRule.CreateRULE_DCLSTATEMENT2(reduction);
                    break;
                case RuleConstants.RULE_LIMITEDID_LIMITID:
                    ////<LimitedId> ::= LimitId
                    result = _parserRule.CreateRULE_LIMITEDID_LIMITID(reduction);
                    break;
                case RuleConstants.RULE_DELIMITEDIDDOLLARS_DELIMITIDDOLLARS:
                    ////<DelimitedIdDollars> ::= DelimitIdDollars
                    result = _parserRule.CreateRULE_DELIMITEDIDDOLLARS_DELIMITIDDOLLARS(reduction);
                    break;
                case RuleConstants.RULE_DELIMITIDQUOTES_DELIMITIDQUOTES:
                    ////<DelimitIdQuotes> ::= DelimitIdQuotes
                    result = _parserRule.CreateRULE_DELIMITIDQUOTES_DELIMITIDQUOTES(reduction);
                    break;
                case RuleConstants.RULE_DELIMITEDID:
                    ////<DelimitedId> ::= <DelimitedIdDollars>
                    result = _parserRule.CreateRULE_DELIMITEDID(reduction);
                    break;
                case RuleConstants.RULE_DELIMITEDID2:
                    ////<DelimitedId> ::= <DelimitIdQuotes>
                    result = _parserRule.CreateRULE_DELIMITEDID2(reduction);
                    break;
                case RuleConstants.RULE_PARAMETER_PARAMETERRULE:
                    ////<Parameter> ::= ParameterRule
                    result = _parserRule.CreateRULE_PARAMETER_PARAMETERRULE(reduction);
                    break;
                case RuleConstants.RULE_DISTINCTRESTRICTION_DISTINCT:
                    ////<DistinctRestriction> ::= DISTINCT
                    result = _parserRule.CreateRULE_DISTINCTRESTRICTION_DISTINCT(reduction);
                    break;
                case RuleConstants.RULE_DISTINCTRESTRICTION:
                    ////<DistinctRestriction> ::= 
                    result = _parserRule.CreateRULE_DISTINCTRESTRICTION(reduction);
                    break;
                case RuleConstants.RULE_IDENTIFIER:
                    ////<Identifier> ::= <DelimitedId>
                    result = _parserRule.CreateRULE_IDENTIFIER(reduction);
                    break;
                case RuleConstants.RULE_IDENTIFIER2:
                    ////<Identifier> ::= <LimitedId>
                    result = _parserRule.CreateRULE_IDENTIFIER2(reduction);
                    break;
                case RuleConstants.RULE_HINTPARAMETER:
                    ////<HintParameter> ::= <StrLiteral>
                    result = _parserRule.CreateRULE_HINTPARAMETER(reduction);
                    break;
                case RuleConstants.RULE_ATTRIBUTE:
                    ////<Attribute> ::= <Identifier> <Indexer>
                    result = _parserRule.CreateRULE_ATTRIBUTE(reduction);
                    break;
                case RuleConstants.RULE_INDEXER_LBRACKET_INTEGERLITERAL_RBRACKET:
                    ////<Indexer> ::= <Indexer> '[' IntegerLiteral ']'
                    result = _parserRule.CreateRULE_INDEXER_LBRACKET_INTEGERLITERAL_RBRACKET(reduction);
                    break;
                case RuleConstants.RULE_INDEXER:
                    ////<Indexer> ::= 
                    result = _parserRule.CreateRULE_INDEXER(reduction);
                    break;
                case RuleConstants.RULE_STRLITERAL_STRINGLITERAL:
                    ////<StrLiteral> ::= StringLiteral
                    result = _parserRule.CreateRULE_STRLITERAL_STRINGLITERAL(reduction);
                    break;
                case RuleConstants.RULE_NUMLITERAL_INTEGERLITERAL:
                    ////<NumLiteral> ::= IntegerLiteral
                    result = _parserRule.CreateRULE_NUMLITERAL_INTEGERLITERAL(reduction);
                    break;
                case RuleConstants.RULE_NUMLITERAL_REALLITERAL:
                    ////<NumLiteral> ::= RealLiteral
                    result = _parserRule.CreateRULE_NUMLITERAL_REALLITERAL(reduction);
                    break;
                case RuleConstants.RULE_REPLACELIST_EQ_COMMA:
                    ////<ReplaceList> ::= <BinaryExpression> '=' <BinaryExpression> ',' <ReplaceList>
                    result = _parserRule.CreateRULE_REPLACELIST_EQ_COMMA(reduction);
                    break;
                case RuleConstants.RULE_REPLACELIST_EQ:
                    ////<ReplaceList> ::= <BinaryExpression> '=' <BinaryExpression>
                    result = _parserRule.CreateRULE_REPLACELIST_EQ(reduction);
                    break;
                case RuleConstants.RULE_BINARYEXPRLIST_LPARAN_RPARAN:
                    ////<BinaryExprList> ::= '(' <BinaryExpressionList> ')'
                    result = _parserRule.CreateRULE_BINARYEXPRLIST_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_BINARYEXPRESSIONLIST_COMMA:
                    ////<BinaryExpressionList> ::= <BinaryExpression> ',' <BinaryExpressionList>
                    result = _parserRule.CreateRULE_BINARYEXPRESSIONLIST_COMMA(reduction);
                    break;
                case RuleConstants.RULE_BINARYEXPRESSIONLIST:
                    ////<BinaryExpressionList> ::= <BinaryExpression>
                    result = _parserRule.CreateRULE_BINARYEXPRESSIONLIST(reduction);
                    break;
                case RuleConstants.RULE_ATRLIST_LPARAN_RPARAN:
                    ////<AtrList> ::= '(' <AttributeList> ')'
                    result = _parserRule.CreateRULE_ATRLIST_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_ATTRIBUTELIST_COMMA:
                    ////<AttributeList> ::= <Attribute> ',' <AttributeList>
                    result = _parserRule.CreateRULE_ATTRIBUTELIST_COMMA(reduction);
                    break;
                case RuleConstants.RULE_ATTRIBUTELIST:
                    ////<AttributeList> ::= <Attribute>
                    result = _parserRule.CreateRULE_ATTRIBUTELIST(reduction);
                    break;
                case RuleConstants.RULE_JSONVALLIST_LPARAN_RPARAN:
                    ////<JSONValList> ::= '(' <JSONValueList> ')'
                    result = _parserRule.CreateRULE_JSONVALLIST_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_JSONVALUELIST_COMMA:
                    ////<JSONValueList> ::= <JSONValue> ',' <JSONValueList>
                    result = _parserRule.CreateRULE_JSONVALUELIST_COMMA(reduction);
                    break;
                case RuleConstants.RULE_JSONVALUELIST:
                    ////<JSONValueList> ::= <JSONValue>
                    result = _parserRule.CreateRULE_JSONVALUELIST(reduction);
                    break;
                case RuleConstants.RULE_DATE_DATETIME_LPARAN_STRINGLITERAL_RPARAN:
                    ////<Date> ::= DateTime '(' StringLiteral ')'
                    result = _parserRule.CreateRULE_DATE_DATETIME_LPARAN_STRINGLITERAL_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_COLLECTIONNAME:
                    ////<CollectionName> ::= <Identifier>
                    result = _parserRule.CreateRULE_COLLECTIONNAME(reduction);
                    break;
                case RuleConstants.RULE_COLLECTIONNAME_DOT:
                    ////<CollectionName> ::= <LimitedId> '.' <LimitedId>
                    result = _parserRule.CreateRULE_COLLECTIONNAME_DOT(reduction);
                    break;
                case RuleConstants.RULE_VALUE:
                    ////<Value> ::= <ValueSign> <NumLiteral>
                    result = _parserRule.CreateRULE_VALUE(reduction);
                    break;
                case RuleConstants.RULE_VALUE2:
                    ////<Value> ::= <StrLiteral>
                    result = _parserRule.CreateRULE_VALUE2(reduction);
                    break;
                case RuleConstants.RULE_VALUE_TRUE:
                    ////<Value> ::= TRUE
                    result = _parserRule.CreateRULE_VALUE_TRUE(reduction);
                    break;
                case RuleConstants.RULE_VALUE_FALSE:
                    ////<Value> ::= FALSE
                    result = _parserRule.CreateRULE_VALUE_FALSE(reduction);
                    break;
                case RuleConstants.RULE_VALUE3:
                    ////<Value> ::= <Date>
                    result = _parserRule.CreateRULE_VALUE3(reduction);
                    break;
                case RuleConstants.RULE_VALUE4:
                    ////<Value> ::= <Parameter>
                    result = _parserRule.CreateRULE_VALUE4(reduction);
                    break;
                case RuleConstants.RULE_VALUE_NULL:
                    ////<Value> ::= NULL
                    result = _parserRule.CreateRULE_VALUE_NULL(reduction);
                    break;
                case RuleConstants.RULE_FUNCTIONNAME:
                    ////<FunctionName> ::= <LimitedId>
                    result = _parserRule.CreateRULE_FUNCTIONNAME(reduction);
                    break;
                case RuleConstants.RULE_FUNCTIONATTRGROUP:
                    ////<FunctionAttrGroup> ::= <DistinctRestriction> <FuncExpressions>
                    result = _parserRule.CreateRULE_FUNCTIONATTRGROUP(reduction);
                    break;
                case RuleConstants.RULE_FUNCTION_LPARAN_RPARAN:
                    ////<Function> ::= <FunctionName> '(' <FunctionAttrGroup> ')'
                    result = _parserRule.CreateRULE_FUNCTION_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_ASEXPRESSIONS_COMMA:
                    ////<AsExpressions> ::= <AsExpressions> ',' <BinaryExpression> <Alias>
                    result = _parserRule.CreateRULE_ASEXPRESSIONS_COMMA(reduction);
                    break;
                case RuleConstants.RULE_ASEXPRESSIONS_COMMA_TIMES:
                    ////<AsExpressions> ::= <AsExpressions> ',' '*'
                    result = _parserRule.CreateRULE_ASEXPRESSIONS_COMMA_TIMES(reduction);
                    break;
                case RuleConstants.RULE_ASEXPRESSIONS:
                    ////<AsExpressions> ::= <BinaryExpression> <Alias>
                    result = _parserRule.CreateRULE_ASEXPRESSIONS(reduction);
                    break;
                case RuleConstants.RULE_ASEXPRESSIONS_TIMES:
                    ////<AsExpressions> ::= '*'
                    result = _parserRule.CreateRULE_ASEXPRESSIONS_TIMES(reduction);
                    break;
                case RuleConstants.RULE_FUNCEXPRESSIONS_COMMA:
                    ////<FuncExpressions> ::= <FuncExpressions> ',' <BinaryExpression>
                    result = _parserRule.CreateRULE_FUNCEXPRESSIONS_COMMA(reduction);
                    break;
                case RuleConstants.RULE_FUNCEXPRESSIONS_COMMA_TIMES:
                    ////<FuncExpressions> ::= <FuncExpressions> ',' '*'
                    result = _parserRule.CreateRULE_FUNCEXPRESSIONS_COMMA_TIMES(reduction);
                    break;
                case RuleConstants.RULE_FUNCEXPRESSIONS:
                    ////<FuncExpressions> ::= <BinaryExpression>
                    result = _parserRule.CreateRULE_FUNCEXPRESSIONS(reduction);
                    break;
                case RuleConstants.RULE_FUNCEXPRESSIONS_TIMES:
                    ////<FuncExpressions> ::= '*'
                    result = _parserRule.CreateRULE_FUNCEXPRESSIONS_TIMES(reduction);
                    break;
                case RuleConstants.RULE_FUNCEXPRESSIONS2:
                    ////<FuncExpressions> ::= 
                    result = _parserRule.CreateRULE_FUNCEXPRESSIONS2(reduction);
                    break;
                case RuleConstants.RULE_ALIAS_AS:
                    ////<Alias> ::= AS <Identifier>
                    result = _parserRule.CreateRULE_ALIAS_AS(reduction);
                    break;
                case RuleConstants.RULE_ALIAS:
                    ////<Alias> ::= 
                    result = _parserRule.CreateRULE_ALIAS(reduction);
                    break;
                case RuleConstants.RULE_ORDER_ASC:
                    ////<Order> ::= ASC
                    result = _parserRule.CreateRULE_ORDER_ASC(reduction);
                    break;
                case RuleConstants.RULE_ORDER_DESC:
                    ////<Order> ::= DESC
                    result = _parserRule.CreateRULE_ORDER_DESC(reduction);
                    break;
                case RuleConstants.RULE_ORDER:
                    ////<Order> ::= 
                    result = _parserRule.CreateRULE_ORDER(reduction);
                    break;
                case RuleConstants.RULE_WHERESECTION_WHERE:
                    ////<WhereSection> ::= WHERE <Expression>
                    result = _parserRule.CreateRULE_WHERESECTION_WHERE(reduction);
                    break;
                case RuleConstants.RULE_WHERESECTION:
                    ////<WhereSection> ::= 
                    result = _parserRule.CreateRULE_WHERESECTION(reduction);
                    break;
                case RuleConstants.RULE_EXPRESSION:
                    ////<Expression> ::= <OrExpr>
                    result = _parserRule.CreateRULE_EXPRESSION(reduction);
                    break;
                case RuleConstants.RULE_OREXPR:
                    ////<OrExpr> ::= <AndExpr>
                    result = _parserRule.CreateRULE_OREXPR(reduction);
                    break;
                case RuleConstants.RULE_OREXPR_OR:
                    ////<OrExpr> ::= <AndExpr> OR <OrExpr>
                    result = _parserRule.CreateRULE_OREXPR_OR(reduction);
                    break;
                case RuleConstants.RULE_ANDEXPR_AND:
                    ////<AndExpr> ::= <UnaryExpr> AND <AndExpr>
                    result = _parserRule.CreateRULE_ANDEXPR_AND(reduction);
                    break;
                case RuleConstants.RULE_ANDEXPR:
                    ////<AndExpr> ::= <UnaryExpr>
                    result = _parserRule.CreateRULE_ANDEXPR(reduction);
                    break;
                case RuleConstants.RULE_UNARYEXPR_NOT:
                    ////<UnaryExpr> ::= NOT <CompareExpr>
                    result = _parserRule.CreateRULE_UNARYEXPR_NOT(reduction);
                    break;
                case RuleConstants.RULE_UNARYEXPR:
                    ////<UnaryExpr> ::= <CompareExpr>
                    result = _parserRule.CreateRULE_UNARYEXPR(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR:
                    ////<CompareExpr> ::= <BinaryExpression> <LogicalOperator> <BinaryExpression>
                    result = _parserRule.CreateRULE_COMPAREEXPR(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_LIKE:
                    ////<CompareExpr> ::= <BinaryExpression> LIKE <BinaryExpression>
                    result = _parserRule.CreateRULE_COMPAREEXPR_LIKE(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_NOT_LIKE:
                    ////<CompareExpr> ::= <BinaryExpression> NOT LIKE <BinaryExpression>
                    result = _parserRule.CreateRULE_COMPAREEXPR_NOT_LIKE(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_CONTAINS_ANY:
                    ////<CompareExpr> ::= <BinaryExpression> CONTAINS ANY <ParamList>
                    result = _parserRule.CreateRULE_COMPAREEXPR_CONTAINS_ANY(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_NOT_CONTAINS_ANY:
                    ////<CompareExpr> ::= <BinaryExpression> NOT CONTAINS ANY <ParamList>
                    result = _parserRule.CreateRULE_COMPAREEXPR_NOT_CONTAINS_ANY(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_CONTAINS_ALL:
                    ////<CompareExpr> ::= <BinaryExpression> CONTAINS ALL <ParamList>
                    result = _parserRule.CreateRULE_COMPAREEXPR_CONTAINS_ALL(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_NOT_CONTAINS_ALL:
                    ////<CompareExpr> ::= <BinaryExpression> NOT CONTAINS ALL <ParamList>
                    result = _parserRule.CreateRULE_COMPAREEXPR_NOT_CONTAINS_ALL(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_ARRAY_SIZE:
                    ////<CompareExpr> ::= <BinaryExpression> ARRAY SIZE <ParamInteger>
                    result = _parserRule.CreateRULE_COMPAREEXPR_ARRAY_SIZE(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_NOT_ARRAY_SIZE:
                    ////<CompareExpr> ::= <BinaryExpression> NOT ARRAY SIZE <ParamInteger>
                    result = _parserRule.CreateRULE_COMPAREEXPR_NOT_ARRAY_SIZE(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_IN:
                    ////<CompareExpr> ::= <BinaryExpression> IN <BinaryExprList>
                    result = _parserRule.CreateRULE_COMPAREEXPR_IN(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_NOT_IN:
                    ////<CompareExpr> ::= <BinaryExpression> NOT IN <BinaryExprList>
                    result = _parserRule.CreateRULE_COMPAREEXPR_NOT_IN(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_BETWEEN_AND:
                    ////<CompareExpr> ::= <BinaryExpression> BETWEEN <BinaryExpression> AND <BinaryExpression>
                    result = _parserRule.CreateRULE_COMPAREEXPR_BETWEEN_AND(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_NOT_BETWEEN_AND:
                    ////<CompareExpr> ::= <BinaryExpression> NOT BETWEEN <BinaryExpression> AND <BinaryExpression>
                    result = _parserRule.CreateRULE_COMPAREEXPR_NOT_BETWEEN_AND(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_EXISTS:
                    ////<CompareExpr> ::= <BinaryExpression> EXISTS
                    result = _parserRule.CreateRULE_COMPAREEXPR_EXISTS(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_NOT_EXISTS:
                    ////<CompareExpr> ::= <BinaryExpression> NOT EXISTS
                    result = _parserRule.CreateRULE_COMPAREEXPR_NOT_EXISTS(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_IS_NULL:
                    ////<CompareExpr> ::= <BinaryExpression> IS NULL
                    result = _parserRule.CreateRULE_COMPAREEXPR_IS_NULL(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_IS_NOT_NULL:
                    ////<CompareExpr> ::= <BinaryExpression> IS NOT NULL
                    result = _parserRule.CreateRULE_COMPAREEXPR_IS_NOT_NULL(reduction);
                    break;
                case RuleConstants.RULE_COMPAREEXPR_LPARAN_RPARAN:
                    ////<CompareExpr> ::= '(' <Expression> ')'
                    result = _parserRule.CreateRULE_COMPAREEXPR_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_LOGICALOPERATOR_EQ:
                    ////<LogicalOperator> ::= '='
                    result = _parserRule.CreateRULE_LOGICALOPERATOR_EQ(reduction);
                    break;
                case RuleConstants.RULE_LOGICALOPERATOR_EXCLAMEQ:
                    ////<LogicalOperator> ::= '!='
                    result = _parserRule.CreateRULE_LOGICALOPERATOR_EXCLAMEQ(reduction);
                    break;
                case RuleConstants.RULE_LOGICALOPERATOR_EQEQ:
                    ////<LogicalOperator> ::= '=='
                    result = _parserRule.CreateRULE_LOGICALOPERATOR_EQEQ(reduction);
                    break;
                case RuleConstants.RULE_LOGICALOPERATOR_LTGT:
                    ////<LogicalOperator> ::= '<>'
                    result = _parserRule.CreateRULE_LOGICALOPERATOR_LTGT(reduction);
                    break;
                case RuleConstants.RULE_LOGICALOPERATOR_LT:
                    ////<LogicalOperator> ::= '<'
                    result = _parserRule.CreateRULE_LOGICALOPERATOR_LT(reduction);
                    break;
                case RuleConstants.RULE_LOGICALOPERATOR_GT:
                    ////<LogicalOperator> ::= '>'
                    result = _parserRule.CreateRULE_LOGICALOPERATOR_GT(reduction);
                    break;
                case RuleConstants.RULE_LOGICALOPERATOR_LTEQ:
                    ////<LogicalOperator> ::= '<='
                    result = _parserRule.CreateRULE_LOGICALOPERATOR_LTEQ(reduction);
                    break;
                case RuleConstants.RULE_LOGICALOPERATOR_GTEQ:
                    ////<LogicalOperator> ::= '>='
                    result = _parserRule.CreateRULE_LOGICALOPERATOR_GTEQ(reduction);
                    break;
                case RuleConstants.RULE_BINARYEXPRESSION:
                    ////<BinaryExpression> ::= <AddExpression>
                    result = _parserRule.CreateRULE_BINARYEXPRESSION(reduction);
                    break;
                case RuleConstants.RULE_ADDEXPRESSION_PLUS:
                    ////<AddExpression> ::= <MultExpression> '+' <AddExpression>
                    result = _parserRule.CreateRULE_ADDEXPRESSION_PLUS(reduction);
                    break;
                case RuleConstants.RULE_ADDEXPRESSION_MINUS:
                    ////<AddExpression> ::= <MultExpression> '-' <AddExpression>
                    result = _parserRule.CreateRULE_ADDEXPRESSION_MINUS(reduction);
                    break;
                case RuleConstants.RULE_ADDEXPRESSION:
                    ////<AddExpression> ::= <MultExpression>
                    result = _parserRule.CreateRULE_ADDEXPRESSION(reduction);
                    break;
                case RuleConstants.RULE_MULTEXPRESSION_TIMES:
                    ////<MultExpression> ::= <MultExpression> '*' <ValueExpression>
                    result = _parserRule.CreateRULE_MULTEXPRESSION_TIMES(reduction);
                    break;
                case RuleConstants.RULE_MULTEXPRESSION_DIV:
                    ////<MultExpression> ::= <MultExpression> '/' <ValueExpression>
                    result = _parserRule.CreateRULE_MULTEXPRESSION_DIV(reduction);
                    break;
                case RuleConstants.RULE_MULTEXPRESSION_PERCENT:
                    ////<MultExpression> ::= <MultExpression> '%' <ValueExpression>
                    result = _parserRule.CreateRULE_MULTEXPRESSION_PERCENT(reduction);
                    break;
                case RuleConstants.RULE_MULTEXPRESSION:
                    ////<MultExpression> ::= <ValueExpression>
                    result = _parserRule.CreateRULE_MULTEXPRESSION(reduction);
                    break;
                case RuleConstants.RULE_VALUEEXPRESSION:
                    ////<ValueExpression> ::= <ValueSign> <JSONValue>
                    result = _parserRule.CreateRULE_VALUEEXPRESSION(reduction);
                    break;
                case RuleConstants.RULE_VALUEEXPRESSION2:
                    ////<ValueExpression> ::= <ValueSign> <Attribute>
                    result = _parserRule.CreateRULE_VALUEEXPRESSION2(reduction);
                    break;
                case RuleConstants.RULE_VALUEEXPRESSION3:
                    ////<ValueExpression> ::= <ValueSign> <Function>
                    result = _parserRule.CreateRULE_VALUEEXPRESSION3(reduction);
                    break;
                case RuleConstants.RULE_VALUEEXPRESSION4:
                    ////<ValueExpression> ::= <ValueSign> <ParensExpression>
                    result = _parserRule.CreateRULE_VALUEEXPRESSION4(reduction);
                    break;
                case RuleConstants.RULE_VALUESIGN_PLUS:
                    ////<ValueSign> ::= '+'
                    result = _parserRule.CreateRULE_VALUESIGN_PLUS(reduction);
                    break;
                case RuleConstants.RULE_VALUESIGN_MINUS:
                    ////<ValueSign> ::= '-'
                    result = _parserRule.CreateRULE_VALUESIGN_MINUS(reduction);
                    break;
                case RuleConstants.RULE_VALUESIGN:
                    ////<ValueSign> ::= 
                    result = _parserRule.CreateRULE_VALUESIGN(reduction);
                    break;
                case RuleConstants.RULE_PARAMLIST:
                    ////<ParamList> ::= <BinaryExprList>
                    result = _parserRule.CreateRULE_PARAMLIST(reduction);
                    break;
                case RuleConstants.RULE_PARAMLIST2:
                    ////<ParamList> ::= <Parameter>
                    result = _parserRule.CreateRULE_PARAMLIST2(reduction);
                    break;
                case RuleConstants.RULE_PARAMLIST3:
                    ////<ParamList> ::= <Attribute>
                    result = _parserRule.CreateRULE_PARAMLIST3(reduction);
                    break;
                case RuleConstants.RULE_PARAMINTEGER_INTEGERLITERAL:
                    ////<ParamInteger> ::= IntegerLiteral
                    result = _parserRule.CreateRULE_PARAMINTEGER_INTEGERLITERAL(reduction);
                    break;
                case RuleConstants.RULE_PARAMINTEGER:
                    ////<ParamInteger> ::= <Parameter>
                    result = _parserRule.CreateRULE_PARAMINTEGER(reduction);
                    break;
                case RuleConstants.RULE_PARENSEXPRESSION_LPARAN_RPARAN:
                    ////<ParensExpression> ::= '(' <BinaryExpression> ')'
                    result = _parserRule.CreateRULE_PARENSEXPRESSION_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_FROMSECTION_FROM:
                    ////<FromSection> ::= FROM <CollectionName>
                    result = _parserRule.CreateRULE_FROMSECTION_FROM(reduction);
                    break;
                case RuleConstants.RULE_OPTTERMINATOR_SEMI:
                    ////<OptTerminator> ::= ';'
                    result = _parserRule.CreateRULE_OPTTERMINATOR_SEMI(reduction);
                    break;
                case RuleConstants.RULE_OPTTERMINATOR:
                    ////<OptTerminator> ::= 
                    result = _parserRule.CreateRULE_OPTTERMINATOR(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUESOPTBRK_LPARAN_RPARAN:
                    ////<QueryValuesOptBrk> ::= '(' <QueryValues> ')'
                    result = _parserRule.CreateRULE_QUERYVALUESOPTBRK_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUESOPTBRK:
                    ////<QueryValuesOptBrk> ::= <QueryValues>
                    result = _parserRule.CreateRULE_QUERYVALUESOPTBRK(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_EQ_COMMA:
                    ////<QueryValues> ::= <Attribute> '=' <BinaryExpression> ',' <QueryValues>
                    result = _parserRule.CreateRULE_QUERYVALUES_EQ_COMMA(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_COMMA:
                    ////<QueryValues> ::= <Attribute> <ArrayUpdateOp> <BinaryExprList> ',' <QueryValues>
                    result = _parserRule.CreateRULE_QUERYVALUES_COMMA(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_REPLACE_LPARAN_RPARAN_COMMA:
                    ////<QueryValues> ::= <Attribute> REPLACE '(' <ReplaceList> ')' ',' <QueryValues>
                    result = _parserRule.CreateRULE_QUERYVALUES_REPLACE_LPARAN_RPARAN_COMMA(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_RENAME_TO_COMMA:
                    ////<QueryValues> ::= RENAME <Attribute> TO <StrLiteral> ',' <QueryValues>
                    result = _parserRule.CreateRULE_QUERYVALUES_RENAME_TO_COMMA(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_DELETE_COMMA:
                    ////<QueryValues> ::= DELETE <Attribute> ',' <QueryValues>
                    result = _parserRule.CreateRULE_QUERYVALUES_DELETE_COMMA(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_EQ:
                    ////<QueryValues> ::= <Attribute> '=' <BinaryExpression>
                    result = _parserRule.CreateRULE_QUERYVALUES_EQ(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES:
                    ////<QueryValues> ::= <Attribute> <ArrayUpdateOp> <BinaryExprList>
                    result = _parserRule.CreateRULE_QUERYVALUES(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_REPLACE_LPARAN_RPARAN:
                    ////<QueryValues> ::= <Attribute> REPLACE '(' <ReplaceList> ')'
                    result = _parserRule.CreateRULE_QUERYVALUES_REPLACE_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_RENAME_TO:
                    ////<QueryValues> ::= RENAME <Attribute> TO <StrLiteral>
                    result = _parserRule.CreateRULE_QUERYVALUES_RENAME_TO(reduction);
                    break;
                case RuleConstants.RULE_QUERYVALUES_DELETE:
                    ////<QueryValues> ::= DELETE <Attribute>
                    result = _parserRule.CreateRULE_QUERYVALUES_DELETE(reduction);
                    break;
                case RuleConstants.RULE_ARRAYUPDATEOP_ADD:
                    ////<ArrayUpdateOp> ::= ADD
                    result = _parserRule.CreateRULE_ARRAYUPDATEOP_ADD(reduction);
                    break;
                case RuleConstants.RULE_ARRAYUPDATEOP_INSERT:
                    ////<ArrayUpdateOp> ::= INSERT
                    result = _parserRule.CreateRULE_ARRAYUPDATEOP_INSERT(reduction);
                    break;
                case RuleConstants.RULE_ARRAYUPDATEOP_REMOVE:
                    ////<ArrayUpdateOp> ::= REMOVE
                    result = _parserRule.CreateRULE_ARRAYUPDATEOP_REMOVE(reduction);
                    break;
                case RuleConstants.RULE_JSONOBJECT_LBRACE_RBRACE:
                    ////<JSONObject> ::= '{' '}'
                    result = _parserRule.CreateRULE_JSONOBJECT_LBRACE_RBRACE(reduction);
                    break;
                case RuleConstants.RULE_JSONOBJECT_LBRACE_RBRACE2:
                    ////<JSONObject> ::= '{' <JSONMembers> '}'
                    result = _parserRule.CreateRULE_JSONOBJECT_LBRACE_RBRACE2(reduction);
                    break;
                case RuleConstants.RULE_JSONMEMBERS:
                    ////<JSONMembers> ::= <AttributePair>
                    result = _parserRule.CreateRULE_JSONMEMBERS(reduction);
                    break;
                case RuleConstants.RULE_JSONMEMBERS_COMMA:
                    ////<JSONMembers> ::= <AttributePair> ',' <JSONMembers>
                    result = _parserRule.CreateRULE_JSONMEMBERS_COMMA(reduction);
                    break;
                case RuleConstants.RULE_ATTRIBUTEPAIR_COLON:
                    ////<AttributePair> ::= <DelimitIdQuotes> ':' <JSONdocValue>
                    result = _parserRule.CreateRULE_ATTRIBUTEPAIR_COLON(reduction);
                    break;
                case RuleConstants.RULE_JSONARRAY_LBRACKET_RBRACKET:
                    ////<JSONArray> ::= '[' ']'
                    result = _parserRule.CreateRULE_JSONARRAY_LBRACKET_RBRACKET(reduction);
                    break;
                case RuleConstants.RULE_JSONARRAY_LBRACKET_RBRACKET2:
                    ////<JSONArray> ::= '[' <Elements> ']'
                    result = _parserRule.CreateRULE_JSONARRAY_LBRACKET_RBRACKET2(reduction);
                    break;
                case RuleConstants.RULE_ELEMENTS:
                    ////<Elements> ::= <JSONdocValue>
                    result = _parserRule.CreateRULE_ELEMENTS(reduction);
                    break;
                case RuleConstants.RULE_ELEMENTS_COMMA:
                    ////<Elements> ::= <Elements> ',' <JSONdocValue>
                    result = _parserRule.CreateRULE_ELEMENTS_COMMA(reduction);
                    break;
                case RuleConstants.RULE_JSONVALUE:
                    ////<JSONValue> ::= <Value>
                    result = _parserRule.CreateRULE_JSONVALUE(reduction);
                    break;
                case RuleConstants.RULE_JSONVALUE2:
                    ////<JSONValue> ::= <JSONString>
                    result = _parserRule.CreateRULE_JSONVALUE2(reduction);
                    break;
                case RuleConstants.RULE_JSONVALUE3:
                    ////<JSONValue> ::= <ArrayProjection>
                    result = _parserRule.CreateRULE_JSONVALUE3(reduction);
                    break;
                case RuleConstants.RULE_ARRAYPROJECTION_LPARAN_RPARAN_SLICE_LPARAN_INTEGERLITERAL_COMMA_INTEGERLITERAL_RPARAN:
                    ////<ArrayProjection> ::= '(' <BinaryExpression> ')' SLICE '(' <ValueSign> IntegerLiteral ',' IntegerLiteral ')'
                    result = _parserRule.CreateRULE_ARRAYPROJECTION_LPARAN_RPARAN_SLICE_LPARAN_INTEGERLITERAL_COMMA_INTEGERLITERAL_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_ARRAYPROJECTION_LPARAN_RPARAN_SLICE_MATCH:
                    ////<ArrayProjection> ::= '(' <BinaryExpression> ')' SLICE MATCH <BinaryExprList>
                    result = _parserRule.CreateRULE_ARRAYPROJECTION_LPARAN_RPARAN_SLICE_MATCH(reduction);
                    break;
                case RuleConstants.RULE_JSONDOCVALUE:
                    ////<JSONdocValue> ::= <JSONValue>
                    result = _parserRule.CreateRULE_JSONDOCVALUE(reduction);
                    break;
                case RuleConstants.RULE_JSONDOCVALUE2:
                    ////<JSONdocValue> ::= <DelimitIdQuotes>
                    result = _parserRule.CreateRULE_JSONDOCVALUE2(reduction);
                    break;
                case RuleConstants.RULE_JSONSTRING:
                    ////<JSONString> ::= <JSONObject>
                    result = _parserRule.CreateRULE_JSONSTRING(reduction);
                    break;
                case RuleConstants.RULE_JSONSTRING2:
                    ////<JSONString> ::= <JSONArray>
                    result = _parserRule.CreateRULE_JSONSTRING2(reduction);
                    break;
                case RuleConstants.RULE_DDLIDENTIFIERCONFIG:
                    ////<DDLIdentifierConfig> ::= <Identifier> <JSONObject>
                    result = _parserRule.CreateRULE_DDLIDENTIFIERCONFIG(reduction);
                    break;
                case RuleConstants.RULE_DDLCONFIGURATION:
                    ////<DDLConfiguration> ::= <Identifier>
                    result = _parserRule.CreateRULE_DDLCONFIGURATION(reduction);
                    break;
                case RuleConstants.RULE_DDLCONFIGURATION2:
                    ////<DDLConfiguration> ::= <DDLIdentifierConfig>
                    result = _parserRule.CreateRULE_DDLCONFIGURATION2(reduction);
                    break;
                case RuleConstants.RULE_DDLCONFIGURATION3:
                    ////<DDLConfiguration> ::= <JSONObject>
                    result = _parserRule.CreateRULE_DDLCONFIGURATION3(reduction);
                    break;
                case RuleConstants.RULE_OBJECTTYPE_DATABASE:
                    ////<ObjectType> ::= DATABASE
                    result = _parserRule.CreateRULE_OBJECTTYPE_DATABASE(reduction);
                    break;
                case RuleConstants.RULE_OBJECTTYPE_COLLECTION:
                    ////<ObjectType> ::= COLLECTION
                    result = _parserRule.CreateRULE_OBJECTTYPE_COLLECTION(reduction);
                    break;
                case RuleConstants.RULE_OBJECTTYPE_INDEX:
                    ////<ObjectType> ::= INDEX
                    result = _parserRule.CreateRULE_OBJECTTYPE_INDEX(reduction);
                    break;
                case RuleConstants.RULE_OBJECTTYPE_LOGIN:
                    ////<ObjectType> ::= LOGIN
                    result = _parserRule.CreateRULE_OBJECTTYPE_LOGIN(reduction);
                    break;
                case RuleConstants.RULE_OBJECTTYPE_USER:
                    ////<ObjectType> ::= USER
                    result = _parserRule.CreateRULE_OBJECTTYPE_USER(reduction);
                    break;
                case RuleConstants.RULE_OBJECTTYPE_ROLE:
                    ////<ObjectType> ::= ROLE
                    result = _parserRule.CreateRULE_OBJECTTYPE_ROLE(reduction);
                    break;
                case RuleConstants.RULE_OBJECTTYPE_MASTER_KEY:
                    ////<ObjectType> ::= MASTER KEY
                    result = _parserRule.CreateRULE_OBJECTTYPE_MASTER_KEY(reduction);
                    break;
                case RuleConstants.RULE_EXECUTESTATEMENT_EXECUTE_LPARAN_RPARAN:
                    ////<ExecuteStatement> ::= EXECUTE '(' <FunctionName> ')' <OptTerminator>
                    result = _parserRule.CreateRULE_EXECUTESTATEMENT_EXECUTE_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_EXECUTESTATEMENT_EXEC_LPARAN_RPARAN:
                    ////<ExecuteStatement> ::= EXEC '(' <FunctionName> ')' <OptTerminator>
                    result = _parserRule.CreateRULE_EXECUTESTATEMENT_EXEC_LPARAN_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_SELECTQUERY_SELECT:
                    ////<SelectQuery> ::= SELECT <SelectAttributes> <SelectFromSection> <SelectWhereSection> <GroupSection> <HavingSection> <OrderSection> <OffsetSection> <HintSection>
                    result = _parserRule.CreateRULE_SELECTQUERY_SELECT(reduction);
                    break;
                case RuleConstants.RULE_SELECTATTRIBUTES:
                    ////<SelectAttributes> ::= <DistinctRestriction> <TopSection> <AsExpressions>
                    result = _parserRule.CreateRULE_SELECTATTRIBUTES(reduction);
                    break;
                case RuleConstants.RULE_TOPSECTION_TOP_LPARAN_INTEGERLITERAL_RPARAN:
                    ////<TopSection> ::= TOP '(' IntegerLiteral ')'
                    result = _parserRule.CreateRULE_TOPSECTION_TOP_LPARAN_INTEGERLITERAL_RPARAN(reduction);
                    break;
                case RuleConstants.RULE_TOPSECTION_TOP_INTEGERLITERAL:
                    ////<TopSection> ::= TOP IntegerLiteral
                    result = _parserRule.CreateRULE_TOPSECTION_TOP_INTEGERLITERAL(reduction);
                    break;
                case RuleConstants.RULE_TOPSECTION:
                    ////<TopSection> ::= 
                    result = _parserRule.CreateRULE_TOPSECTION(reduction);
                    break;
                case RuleConstants.RULE_SELECTFROMSECTION:
                    ////<SelectFromSection> ::= <FromSection>
                    result = _parserRule.CreateRULE_SELECTFROMSECTION(reduction);
                    break;
                case RuleConstants.RULE_SELECTWHERESECTION:
                    ////<SelectWhereSection> ::= <WhereSection>
                    result = _parserRule.CreateRULE_SELECTWHERESECTION(reduction);
                    break;
                case RuleConstants.RULE_GROUPSECTION_GROUP_BY:
                    ////<GroupSection> ::= GROUP BY <OrderedExpressionList>
                    result = _parserRule.CreateRULE_GROUPSECTION_GROUP_BY(reduction);
                    break;
                case RuleConstants.RULE_GROUPSECTION:
                    ////<GroupSection> ::= 
                    result = _parserRule.CreateRULE_GROUPSECTION(reduction);
                    break;
                case RuleConstants.RULE_HAVINGSECTION_HAVING:
                    ////<HavingSection> ::= HAVING <Function> <LogicalOperator> <Value>
                    result = _parserRule.CreateRULE_HAVINGSECTION_HAVING(reduction);
                    break;
                case RuleConstants.RULE_HAVINGSECTION:
                    ////<HavingSection> ::= 
                    result = _parserRule.CreateRULE_HAVINGSECTION(reduction);
                    break;
                case RuleConstants.RULE_ORDERSECTION_ORDER_BY:
                    ////<OrderSection> ::= ORDER BY <OrderedExpressionList>
                    result = _parserRule.CreateRULE_ORDERSECTION_ORDER_BY(reduction);
                    break;
                case RuleConstants.RULE_ORDERSECTION:
                    ////<OrderSection> ::= 
                    result = _parserRule.CreateRULE_ORDERSECTION(reduction);
                    break;
                case RuleConstants.RULE_OFFSETSECTION_OFFSET_INTEGERLITERAL_ROWS_FETCH_NEXT_INTEGERLITERAL_ROWS_ONLY:
                    ////<OffsetSection> ::= OFFSET IntegerLiteral ROWS FETCH NEXT IntegerLiteral ROWS ONLY
                    result = _parserRule.CreateRULE_OFFSETSECTION_OFFSET_INTEGERLITERAL_ROWS_FETCH_NEXT_INTEGERLITERAL_ROWS_ONLY(reduction);
                    break;
                case RuleConstants.RULE_OFFSETSECTION_OFFSET_INTEGERLITERAL_ROWS:
                    ////<OffsetSection> ::= OFFSET IntegerLiteral ROWS
                    result = _parserRule.CreateRULE_OFFSETSECTION_OFFSET_INTEGERLITERAL_ROWS(reduction);
                    break;
                case RuleConstants.RULE_OFFSETSECTION:
                    ////<OffsetSection> ::= 
                    result = _parserRule.CreateRULE_OFFSETSECTION(reduction);
                    break;
                case RuleConstants.RULE_HINTSECTION_HINT:
                    ////<HintSection> ::= HINT <HintParameter>
                    result = _parserRule.CreateRULE_HINTSECTION_HINT(reduction);
                    break;
                case RuleConstants.RULE_HINTSECTION:
                    ////<HintSection> ::= 
                    result = _parserRule.CreateRULE_HINTSECTION(reduction);
                    break;
                case RuleConstants.RULE_ORDEREDEXPRESSION:
                    ////<OrderedExpression> ::= <BinaryExpression> <Order>
                    result = _parserRule.CreateRULE_ORDEREDEXPRESSION(reduction);
                    break;
                case RuleConstants.RULE_ORDEREDEXPRESSIONLIST_COMMA:
                    ////<OrderedExpressionList> ::= <OrderedExpressionList> ',' <OrderedExpression>
                    result = _parserRule.CreateRULE_ORDEREDEXPRESSIONLIST_COMMA(reduction);
                    break;
                case RuleConstants.RULE_ORDEREDEXPRESSIONLIST:
                    ////<OrderedExpressionList> ::= <OrderedExpression>
                    result = _parserRule.CreateRULE_ORDEREDEXPRESSIONLIST(reduction);
                    break;
                case RuleConstants.RULE_INSERTQUERY_INSERT_INTO_VALUES:
                    ////<InsertQuery> ::= INSERT INTO <CollectionName> <AtrList> VALUES <BinaryExprList>
                    result = _parserRule.CreateRULE_INSERTQUERY_INSERT_INTO_VALUES(reduction);
                    break;
                case RuleConstants.RULE_UPDATEQUERY_UPDATE_SET:
                    ////<UpdateQuery> ::= UPDATE <CollectionName> SET <QueryValuesOptBrk> <UpdateWhereSection>
                    result = _parserRule.CreateRULE_UPDATEQUERY_UPDATE_SET(reduction);
                    break;
                case RuleConstants.RULE_UPDATEWHERESECTION:
                    ////<UpdateWhereSection> ::= <WhereSection>
                    result = _parserRule.CreateRULE_UPDATEWHERESECTION(reduction);
                    break;
                case RuleConstants.RULE_DELETEQUERY_DELETE:
                    ////<DeleteQuery> ::= DELETE <DeleteFromSection> <DeleteWhereSection>
                    result = _parserRule.CreateRULE_DELETEQUERY_DELETE(reduction);
                    break;
                case RuleConstants.RULE_DELETEFROMSECTION:
                    ////<DeleteFromSection> ::= <FromSection>
                    result = _parserRule.CreateRULE_DELETEFROMSECTION(reduction);
                    break;
                case RuleConstants.RULE_DELETEWHERESECTION:
                    ////<DeleteWhereSection> ::= <WhereSection>
                    result = _parserRule.CreateRULE_DELETEWHERESECTION(reduction);
                    break;
                case RuleConstants.RULE_CREATESTATEMENT_CREATE:
                    ////<CreateStatement> ::= CREATE <ObjectType> <DDLConfiguration>
                    result = _parserRule.CreateRULE_CREATESTATEMENT_CREATE(reduction);
                    break;
                case RuleConstants.RULE_ALTERSTATEMENT_ALTER:
                    ////<AlterStatement> ::= ALTER <ObjectType> <DDLConfiguration>
                    result = _parserRule.CreateRULE_ALTERSTATEMENT_ALTER(reduction);
                    break;
                case RuleConstants.RULE_DROPSTATEMENT_DROP:
                    ////<DropStatement> ::= DROP <ObjectType> <DDLConfiguration>
                    result = _parserRule.CreateRULE_DROPSTATEMENT_DROP(reduction);
                    break;
                case RuleConstants.RULE_TRUNCATESTATEMENT_TRUNCATE_COLLECTION:
                    ////<TruncateStatement> ::= TRUNCATE COLLECTION <CollectionName>
                    result = _parserRule.CreateRULE_TRUNCATESTATEMENT_TRUNCATE_COLLECTION(reduction);
                    break;
                case RuleConstants.RULE_BACKUPSTATEMENT_BACKUP_DATABASE:
                    ////<BackupStatement> ::= BACKUP DATABASE <DDLIdentifierConfig>
                    result = _parserRule.CreateRULE_BACKUPSTATEMENT_BACKUP_DATABASE(reduction);
                    break;
                case RuleConstants.RULE_RESTORESTATEMENT_RESTORE_DATABASE:
                    ////<RestoreStatement> ::= RESTORE DATABASE <DDLIdentifierConfig>
                    result = _parserRule.CreateRULE_RESTORESTATEMENT_RESTORE_DATABASE(reduction);
                    break;
                case RuleConstants.RULE_CONTROLSTATEMENT:
                    ////<ControlStatement> ::= <EnableStatement>
                    result = _parserRule.CreateRULE_CONTROLSTATEMENT(reduction);
                    break;
                case RuleConstants.RULE_CONTROLSTATEMENT2:
                    ////<ControlStatement> ::= <DisableStatement>
                    result = _parserRule.CreateRULE_CONTROLSTATEMENT2(reduction);
                    break;
                case RuleConstants.RULE_DISABLESTATEMENT:
                    ////<DisableStatement> ::= 
                    result = _parserRule.CreateRULE_DISABLESTATEMENT(reduction);
                    break;
                case RuleConstants.RULE_ENABLESTATEMENT:
                    ////<EnableStatement> ::= 
                    result = _parserRule.CreateRULE_ENABLESTATEMENT(reduction);
                    break;
                case RuleConstants.RULE_DCLOBJECT:
                    ////<DCLObject> ::= <Identifier>
                    result = _parserRule.CreateRULE_DCLOBJECT(reduction);
                    break;
                case RuleConstants.RULE_DCLOBJECT_DOT:
                    ////<DCLObject> ::= <Identifier> '.' <Identifier>
                    result = _parserRule.CreateRULE_DCLOBJECT_DOT(reduction);
                    break;
                case RuleConstants.RULE_GRANTSTATEMENT_GRANT_ON_TO:
                    ////<GrantStatement> ::= GRANT <Identifier> ON <DCLObject> TO <StrLiteral>
                    result = _parserRule.CreateRULE_GRANTSTATEMENT_GRANT_ON_TO(reduction);
                    break;
                case RuleConstants.RULE_REVOKESTATEMENT_REVOKE_ON_FROM:
                    ////<RevokeStatement> ::= REVOKE <Identifier> ON <DCLObject> FROM <StrLiteral>
                    result = _parserRule.CreateRULE_REVOKESTATEMENT_REVOKE_ON_FROM(reduction);
                    break;
            }
            return result ?? reduction;
        }

    }
}
