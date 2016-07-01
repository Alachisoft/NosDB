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
namespace Alachisoft.NosDB.Core.Queries
{
    enum RuleConstants : int
    {
        RULE_STATEMENT = 0, // <Statement> ::= <SingleStatment>
        RULE_STATEMENT2 = 1, // <Statement> ::= <MultiStatement>
        RULE_SINGLESTATMENT = 2, // <SingleStatment> ::= <SPStatement>
        RULE_MULTISTATEMENT = 3, // <MultiStatement> ::= <DMLStatement> <OptTerminator> <MultiStatement>
        RULE_MULTISTATEMENT2 = 4, // <MultiStatement> ::= <DDLStatement> <OptTerminator> <MultiStatement>
        RULE_MULTISTATEMENT3 = 5, // <MultiStatement> ::= <DCLStatement> <OptTerminator> <MultiStatement>
        RULE_MULTISTATEMENT4 = 6, // <MultiStatement> ::= <DMLStatement> <OptTerminator>
        RULE_MULTISTATEMENT5 = 7, // <MultiStatement> ::= <DDLStatement> <OptTerminator>
        RULE_MULTISTATEMENT6 = 8, // <MultiStatement> ::= <DCLStatement> <OptTerminator>
        RULE_DMLSTATEMENT = 9, // <DMLStatement> ::= <SelectQuery>
        RULE_DMLSTATEMENT2 = 10, // <DMLStatement> ::= <InsertQuery>
        RULE_DMLSTATEMENT3 = 11, // <DMLStatement> ::= <UpdateQuery>
        RULE_DMLSTATEMENT4 = 12, // <DMLStatement> ::= <DeleteQuery>
        RULE_SPSTATEMENT = 13, // <SPStatement> ::= <ExecuteStatement>
        RULE_DDLSTATEMENT = 14, // <DDLStatement> ::= <CreateStatement>
        RULE_DDLSTATEMENT2 = 15, // <DDLStatement> ::= <AlterStatement>
        RULE_DDLSTATEMENT3 = 16, // <DDLStatement> ::= <DropStatement>
        RULE_DDLSTATEMENT4 = 17, // <DDLStatement> ::= <TruncateStatement>
        RULE_DDLSTATEMENT5 = 18, // <DDLStatement> ::= <BackupStatement>
        RULE_DDLSTATEMENT6 = 19, // <DDLStatement> ::= <RestoreStatement>
        RULE_DCLSTATEMENT = 20, // <DCLStatement> ::= <GrantStatement>
        RULE_DCLSTATEMENT2 = 21, // <DCLStatement> ::= <RevokeStatement>
        RULE_LIMITEDID_LIMITID = 22, // <LimitedId> ::= LimitId
        RULE_DELIMITEDIDDOLLARS_DELIMITIDDOLLARS = 23, // <DelimitedIdDollars> ::= DelimitIdDollars
        RULE_DELIMITIDQUOTES_DELIMITIDQUOTES = 24, // <DelimitIdQuotes> ::= DelimitIdQuotes
        RULE_DELIMITEDID = 25, // <DelimitedId> ::= <DelimitedIdDollars>
        RULE_DELIMITEDID2 = 26, // <DelimitedId> ::= <DelimitIdQuotes>
        RULE_PARAMETER_PARAMETERRULE = 27, // <Parameter> ::= ParameterRule
        RULE_DISTINCTRESTRICTION_DISTINCT = 28, // <DistinctRestriction> ::= DISTINCT
        RULE_DISTINCTRESTRICTION = 29, // <DistinctRestriction> ::= 
        RULE_IDENTIFIER = 30, // <Identifier> ::= <DelimitedId>
        RULE_IDENTIFIER2 = 31, // <Identifier> ::= <LimitedId>
        RULE_HINTPARAMETER = 32, // <HintParameter> ::= <StrLiteral>
        RULE_ATTRIBUTE = 33, // <Attribute> ::= <Identifier> <Indexer>
        RULE_INDEXER_LBRACKET_INTEGERLITERAL_RBRACKET = 34, // <Indexer> ::= <Indexer> '[' IntegerLiteral ']'
        RULE_INDEXER = 35, // <Indexer> ::= 
        RULE_STRLITERAL_STRINGLITERAL = 36, // <StrLiteral> ::= StringLiteral
        RULE_NUMLITERAL_INTEGERLITERAL = 37, // <NumLiteral> ::= IntegerLiteral
        RULE_NUMLITERAL_REALLITERAL = 38, // <NumLiteral> ::= RealLiteral
        RULE_REPLACELIST_EQ_COMMA = 39, // <ReplaceList> ::= <BinaryExpression> '=' <BinaryExpression> ',' <ReplaceList>
        RULE_REPLACELIST_EQ = 40, // <ReplaceList> ::= <BinaryExpression> '=' <BinaryExpression>
        RULE_BINARYEXPRLIST_LPARAN_RPARAN = 41, // <BinaryExprList> ::= '(' <BinaryExpressionList> ')'
        RULE_BINARYEXPRESSIONLIST_COMMA = 42, // <BinaryExpressionList> ::= <BinaryExpression> ',' <BinaryExpressionList>
        RULE_BINARYEXPRESSIONLIST = 43, // <BinaryExpressionList> ::= <BinaryExpression>
        RULE_ATRLIST_LPARAN_RPARAN = 44, // <AtrList> ::= '(' <AttributeList> ')'
        RULE_ATTRIBUTELIST_COMMA = 45, // <AttributeList> ::= <Attribute> ',' <AttributeList>
        RULE_ATTRIBUTELIST = 46, // <AttributeList> ::= <Attribute>
        RULE_JSONVALLIST_LPARAN_RPARAN = 47, // <JSONValList> ::= '(' <JSONValueList> ')'
        RULE_JSONVALUELIST_COMMA = 48, // <JSONValueList> ::= <JSONValue> ',' <JSONValueList>
        RULE_JSONVALUELIST = 49, // <JSONValueList> ::= <JSONValue>
        RULE_DATE_DATETIME_LPARAN_STRINGLITERAL_RPARAN = 50, // <Date> ::= DateTime '(' StringLiteral ')'
        RULE_COLLECTIONNAME = 51, // <CollectionName> ::= <Identifier>
        RULE_COLLECTIONNAME_DOT = 52, // <CollectionName> ::= <LimitedId> '.' <LimitedId>
        RULE_VALUE = 53, // <Value> ::= <ValueSign> <NumLiteral>
        RULE_VALUE2 = 54, // <Value> ::= <StrLiteral>
        RULE_VALUE_TRUE = 55, // <Value> ::= TRUE
        RULE_VALUE_FALSE = 56, // <Value> ::= FALSE
        RULE_VALUE3 = 57, // <Value> ::= <Date>
        RULE_VALUE4 = 58, // <Value> ::= <Parameter>
        RULE_VALUE_NULL = 59, // <Value> ::= NULL
        RULE_FUNCTIONNAME = 60, // <FunctionName> ::= <LimitedId>
        RULE_FUNCTIONATTRGROUP = 61, // <FunctionAttrGroup> ::= <DistinctRestriction> <FuncExpressions>
        RULE_FUNCTION_LPARAN_RPARAN = 62, // <Function> ::= <FunctionName> '(' <FunctionAttrGroup> ')'
        RULE_ASEXPRESSIONS_COMMA = 63, // <AsExpressions> ::= <AsExpressions> ',' <BinaryExpression> <Alias>
        RULE_ASEXPRESSIONS_COMMA_TIMES = 64, // <AsExpressions> ::= <AsExpressions> ',' '*'
        RULE_ASEXPRESSIONS = 65, // <AsExpressions> ::= <BinaryExpression> <Alias>
        RULE_ASEXPRESSIONS_TIMES = 66, // <AsExpressions> ::= '*'
        RULE_FUNCEXPRESSIONS_COMMA = 67, // <FuncExpressions> ::= <FuncExpressions> ',' <BinaryExpression>
        RULE_FUNCEXPRESSIONS_COMMA_TIMES = 68, // <FuncExpressions> ::= <FuncExpressions> ',' '*'
        RULE_FUNCEXPRESSIONS = 69, // <FuncExpressions> ::= <BinaryExpression>
        RULE_FUNCEXPRESSIONS_TIMES = 70, // <FuncExpressions> ::= '*'
        RULE_FUNCEXPRESSIONS2 = 71, // <FuncExpressions> ::= 
        RULE_ALIAS_AS = 72, // <Alias> ::= AS <Identifier>
        RULE_ALIAS = 73, // <Alias> ::= 
        RULE_ORDER_ASC = 74, // <Order> ::= ASC
        RULE_ORDER_DESC = 75, // <Order> ::= DESC
        RULE_ORDER = 76, // <Order> ::= 
        RULE_WHERESECTION_WHERE = 77, // <WhereSection> ::= WHERE <Expression>
        RULE_WHERESECTION = 78, // <WhereSection> ::= 
        RULE_EXPRESSION = 79, // <Expression> ::= <OrExpr>
        RULE_OREXPR = 80, // <OrExpr> ::= <AndExpr>
        RULE_OREXPR_OR = 81, // <OrExpr> ::= <AndExpr> OR <OrExpr>
        RULE_ANDEXPR_AND = 82, // <AndExpr> ::= <UnaryExpr> AND <AndExpr>
        RULE_ANDEXPR = 83, // <AndExpr> ::= <UnaryExpr>
        RULE_UNARYEXPR_NOT = 84, // <UnaryExpr> ::= NOT <CompareExpr>
        RULE_UNARYEXPR = 85, // <UnaryExpr> ::= <CompareExpr>
        RULE_COMPAREEXPR = 86, // <CompareExpr> ::= <BinaryExpression> <LogicalOperator> <BinaryExpression>
        RULE_COMPAREEXPR_LIKE = 87, // <CompareExpr> ::= <BinaryExpression> LIKE <BinaryExpression>
        RULE_COMPAREEXPR_NOT_LIKE = 88, // <CompareExpr> ::= <BinaryExpression> NOT LIKE <BinaryExpression>
        RULE_COMPAREEXPR_CONTAINS_ANY = 89, // <CompareExpr> ::= <BinaryExpression> CONTAINS ANY <ParamList>
        RULE_COMPAREEXPR_NOT_CONTAINS_ANY = 90, // <CompareExpr> ::= <BinaryExpression> NOT CONTAINS ANY <ParamList>
        RULE_COMPAREEXPR_CONTAINS_ALL = 91, // <CompareExpr> ::= <BinaryExpression> CONTAINS ALL <ParamList>
        RULE_COMPAREEXPR_NOT_CONTAINS_ALL = 92, // <CompareExpr> ::= <BinaryExpression> NOT CONTAINS ALL <ParamList>
        RULE_COMPAREEXPR_ARRAY_SIZE = 93, // <CompareExpr> ::= <BinaryExpression> ARRAY SIZE <ParamInteger>
        RULE_COMPAREEXPR_NOT_ARRAY_SIZE = 94, // <CompareExpr> ::= <BinaryExpression> NOT ARRAY SIZE <ParamInteger>
        RULE_COMPAREEXPR_IN = 95, // <CompareExpr> ::= <BinaryExpression> IN <BinaryExprList>
        RULE_COMPAREEXPR_NOT_IN = 96, // <CompareExpr> ::= <BinaryExpression> NOT IN <BinaryExprList>
        RULE_COMPAREEXPR_BETWEEN_AND = 97, // <CompareExpr> ::= <BinaryExpression> BETWEEN <BinaryExpression> AND <BinaryExpression>
        RULE_COMPAREEXPR_NOT_BETWEEN_AND = 98, // <CompareExpr> ::= <BinaryExpression> NOT BETWEEN <BinaryExpression> AND <BinaryExpression>
        RULE_COMPAREEXPR_EXISTS = 99, // <CompareExpr> ::= <BinaryExpression> EXISTS
        RULE_COMPAREEXPR_NOT_EXISTS = 100, // <CompareExpr> ::= <BinaryExpression> NOT EXISTS
        RULE_COMPAREEXPR_IS_NULL = 101, // <CompareExpr> ::= <BinaryExpression> IS NULL
        RULE_COMPAREEXPR_IS_NOT_NULL = 102, // <CompareExpr> ::= <BinaryExpression> IS NOT NULL
        RULE_COMPAREEXPR_LPARAN_RPARAN = 103, // <CompareExpr> ::= '(' <Expression> ')'
        RULE_LOGICALOPERATOR_EQ = 104, // <LogicalOperator> ::= '='
        RULE_LOGICALOPERATOR_EXCLAMEQ = 105, // <LogicalOperator> ::= '!='
        RULE_LOGICALOPERATOR_EQEQ = 106, // <LogicalOperator> ::= '=='
        RULE_LOGICALOPERATOR_LTGT = 107, // <LogicalOperator> ::= '<>'
        RULE_LOGICALOPERATOR_LT = 108, // <LogicalOperator> ::= '<'
        RULE_LOGICALOPERATOR_GT = 109, // <LogicalOperator> ::= '>'
        RULE_LOGICALOPERATOR_LTEQ = 110, // <LogicalOperator> ::= '<='
        RULE_LOGICALOPERATOR_GTEQ = 111, // <LogicalOperator> ::= '>='
        RULE_BINARYEXPRESSION = 112, // <BinaryExpression> ::= <AddExpression>
        RULE_ADDEXPRESSION_PLUS = 113, // <AddExpression> ::= <MultExpression> '+' <AddExpression>
        RULE_ADDEXPRESSION_MINUS = 114, // <AddExpression> ::= <MultExpression> '-' <AddExpression>
        RULE_ADDEXPRESSION = 115, // <AddExpression> ::= <MultExpression>
        RULE_MULTEXPRESSION_TIMES = 116, // <MultExpression> ::= <MultExpression> '*' <ValueExpression>
        RULE_MULTEXPRESSION_DIV = 117, // <MultExpression> ::= <MultExpression> '/' <ValueExpression>
        RULE_MULTEXPRESSION_PERCENT = 118, // <MultExpression> ::= <MultExpression> '%' <ValueExpression>
        RULE_MULTEXPRESSION = 119, // <MultExpression> ::= <ValueExpression>
        RULE_VALUEEXPRESSION = 120, // <ValueExpression> ::= <ValueSign> <JSONValue>
        RULE_VALUEEXPRESSION2 = 121, // <ValueExpression> ::= <ValueSign> <Attribute>
        RULE_VALUEEXPRESSION3 = 122, // <ValueExpression> ::= <ValueSign> <Function>
        RULE_VALUEEXPRESSION4 = 123, // <ValueExpression> ::= <ValueSign> <ParensExpression>
        RULE_VALUESIGN_PLUS = 124, // <ValueSign> ::= '+'
        RULE_VALUESIGN_MINUS = 125, // <ValueSign> ::= '-'
        RULE_VALUESIGN = 126, // <ValueSign> ::= 
        RULE_PARAMLIST = 127, // <ParamList> ::= <BinaryExprList>
        RULE_PARAMLIST2 = 128, // <ParamList> ::= <Parameter>
        RULE_PARAMLIST3 = 129, // <ParamList> ::= <Attribute>
        RULE_PARAMINTEGER_INTEGERLITERAL = 130, // <ParamInteger> ::= IntegerLiteral
        RULE_PARAMINTEGER = 131, // <ParamInteger> ::= <Parameter>
        RULE_PARENSEXPRESSION_LPARAN_RPARAN = 132, // <ParensExpression> ::= '(' <BinaryExpression> ')'
        RULE_FROMSECTION_FROM = 133, // <FromSection> ::= FROM <CollectionName>
        RULE_OPTTERMINATOR_SEMI = 134, // <OptTerminator> ::= ';'
        RULE_OPTTERMINATOR = 135, // <OptTerminator> ::= 
        RULE_QUERYVALUESOPTBRK_LPARAN_RPARAN = 136, // <QueryValuesOptBrk> ::= '(' <QueryValues> ')'
        RULE_QUERYVALUESOPTBRK = 137, // <QueryValuesOptBrk> ::= <QueryValues>
        RULE_QUERYVALUES_EQ_COMMA = 138, // <QueryValues> ::= <Attribute> '=' <BinaryExpression> ',' <QueryValues>
        RULE_QUERYVALUES_COMMA = 139, // <QueryValues> ::= <Attribute> <ArrayUpdateOp> <BinaryExprList> ',' <QueryValues>
        RULE_QUERYVALUES_REPLACE_LPARAN_RPARAN_COMMA = 140, // <QueryValues> ::= <Attribute> REPLACE '(' <ReplaceList> ')' ',' <QueryValues>
        RULE_QUERYVALUES_RENAME_TO_COMMA = 141, // <QueryValues> ::= RENAME <Attribute> TO <StrLiteral> ',' <QueryValues>
        RULE_QUERYVALUES_DELETE_COMMA = 142, // <QueryValues> ::= DELETE <Attribute> ',' <QueryValues>
        RULE_QUERYVALUES_EQ = 143, // <QueryValues> ::= <Attribute> '=' <BinaryExpression>
        RULE_QUERYVALUES = 144, // <QueryValues> ::= <Attribute> <ArrayUpdateOp> <BinaryExprList>
        RULE_QUERYVALUES_REPLACE_LPARAN_RPARAN = 145, // <QueryValues> ::= <Attribute> REPLACE '(' <ReplaceList> ')'
        RULE_QUERYVALUES_RENAME_TO = 146, // <QueryValues> ::= RENAME <Attribute> TO <StrLiteral>
        RULE_QUERYVALUES_DELETE = 147, // <QueryValues> ::= DELETE <Attribute>
        RULE_ARRAYUPDATEOP_ADD = 148, // <ArrayUpdateOp> ::= ADD
        RULE_ARRAYUPDATEOP_INSERT = 149, // <ArrayUpdateOp> ::= INSERT
        RULE_ARRAYUPDATEOP_REMOVE = 150, // <ArrayUpdateOp> ::= REMOVE
        RULE_JSONOBJECT_LBRACE_RBRACE = 151, // <JSONObject> ::= '{' '}'
        RULE_JSONOBJECT_LBRACE_RBRACE2 = 152, // <JSONObject> ::= '{' <JSONMembers> '}'
        RULE_JSONMEMBERS = 153, // <JSONMembers> ::= <AttributePair>
        RULE_JSONMEMBERS_COMMA = 154, // <JSONMembers> ::= <AttributePair> ',' <JSONMembers>
        RULE_ATTRIBUTEPAIR_COLON = 155, // <AttributePair> ::= <DelimitIdQuotes> ':' <JSONdocValue>
        RULE_JSONARRAY_LBRACKET_RBRACKET = 156, // <JSONArray> ::= '[' ']'
        RULE_JSONARRAY_LBRACKET_RBRACKET2 = 157, // <JSONArray> ::= '[' <Elements> ']'
        RULE_ELEMENTS = 158, // <Elements> ::= <JSONdocValue>
        RULE_ELEMENTS_COMMA = 159, // <Elements> ::= <Elements> ',' <JSONdocValue>
        RULE_JSONVALUE = 160, // <JSONValue> ::= <Value>
        RULE_JSONVALUE2 = 161, // <JSONValue> ::= <JSONString>
        RULE_JSONVALUE3 = 162, // <JSONValue> ::= <ArrayProjection>
        RULE_ARRAYPROJECTION_LPARAN_RPARAN_SLICE_LPARAN_INTEGERLITERAL_COMMA_INTEGERLITERAL_RPARAN = 163, // <ArrayProjection> ::= '(' <BinaryExpression> ')' SLICE '(' <ValueSign> IntegerLiteral ',' IntegerLiteral ')'
        RULE_ARRAYPROJECTION_LPARAN_RPARAN_SLICE_MATCH = 164, // <ArrayProjection> ::= '(' <BinaryExpression> ')' SLICE MATCH <BinaryExprList>
        RULE_JSONDOCVALUE = 165, // <JSONdocValue> ::= <JSONValue>
        RULE_JSONDOCVALUE2 = 166, // <JSONdocValue> ::= <DelimitIdQuotes>
        RULE_JSONSTRING = 167, // <JSONString> ::= <JSONObject>
        RULE_JSONSTRING2 = 168, // <JSONString> ::= <JSONArray>
        RULE_DDLIDENTIFIERCONFIG = 169, // <DDLIdentifierConfig> ::= <Identifier> <JSONObject>
        RULE_DDLCONFIGURATION = 170, // <DDLConfiguration> ::= <Identifier>
        RULE_DDLCONFIGURATION2 = 171, // <DDLConfiguration> ::= <DDLIdentifierConfig>
        RULE_DDLCONFIGURATION3 = 172, // <DDLConfiguration> ::= <JSONObject>
        RULE_OBJECTTYPE_DATABASE = 173, // <ObjectType> ::= DATABASE
        RULE_OBJECTTYPE_COLLECTION = 174, // <ObjectType> ::= COLLECTION
        RULE_OBJECTTYPE_INDEX = 175, // <ObjectType> ::= INDEX
        RULE_OBJECTTYPE_LOGIN = 176, // <ObjectType> ::= LOGIN
        RULE_OBJECTTYPE_USER = 177, // <ObjectType> ::= USER
        RULE_OBJECTTYPE_ROLE = 178, // <ObjectType> ::= ROLE
        RULE_OBJECTTYPE_MASTER_KEY = 179, // <ObjectType> ::= MASTER KEY
        RULE_EXECUTESTATEMENT_EXECUTE_LPARAN_RPARAN = 180, // <ExecuteStatement> ::= EXECUTE '(' <FunctionName> ')' <OptTerminator>
        RULE_EXECUTESTATEMENT_EXEC_LPARAN_RPARAN = 181, // <ExecuteStatement> ::= EXEC '(' <FunctionName> ')' <OptTerminator>
        RULE_SELECTQUERY_SELECT = 182, // <SelectQuery> ::= SELECT <SelectAttributes> <SelectFromSection> <SelectWhereSection> <GroupSection> <HavingSection> <OrderSection> <OffsetSection> <HintSection>
        RULE_SELECTATTRIBUTES = 183, // <SelectAttributes> ::= <DistinctRestriction> <TopSection> <AsExpressions>
        RULE_TOPSECTION_TOP_LPARAN_INTEGERLITERAL_RPARAN = 184, // <TopSection> ::= TOP '(' IntegerLiteral ')'
        RULE_TOPSECTION_TOP_INTEGERLITERAL = 185, // <TopSection> ::= TOP IntegerLiteral
        RULE_TOPSECTION = 186, // <TopSection> ::= 
        RULE_SELECTFROMSECTION = 187, // <SelectFromSection> ::= <FromSection>
        RULE_SELECTWHERESECTION = 188, // <SelectWhereSection> ::= <WhereSection>
        RULE_GROUPSECTION_GROUP_BY = 189, // <GroupSection> ::= GROUP BY <OrderedExpressionList>
        RULE_GROUPSECTION = 190, // <GroupSection> ::= 
        RULE_HAVINGSECTION_HAVING = 191, // <HavingSection> ::= HAVING <Function> <LogicalOperator> <Value>
        RULE_HAVINGSECTION = 192, // <HavingSection> ::= 
        RULE_ORDERSECTION_ORDER_BY = 193, // <OrderSection> ::= ORDER BY <OrderedExpressionList>
        RULE_ORDERSECTION = 194, // <OrderSection> ::= 
        RULE_OFFSETSECTION_OFFSET_INTEGERLITERAL_ROWS_FETCH_NEXT_INTEGERLITERAL_ROWS_ONLY = 195, // <OffsetSection> ::= OFFSET IntegerLiteral ROWS FETCH NEXT IntegerLiteral ROWS ONLY
        RULE_OFFSETSECTION_OFFSET_INTEGERLITERAL_ROWS = 196, // <OffsetSection> ::= OFFSET IntegerLiteral ROWS
        RULE_OFFSETSECTION = 197, // <OffsetSection> ::= 
        RULE_HINTSECTION_HINT = 198, // <HintSection> ::= HINT <HintParameter>
        RULE_HINTSECTION = 199, // <HintSection> ::= 
        RULE_ORDEREDEXPRESSION = 200, // <OrderedExpression> ::= <BinaryExpression> <Order>
        RULE_ORDEREDEXPRESSIONLIST_COMMA = 201, // <OrderedExpressionList> ::= <OrderedExpressionList> ',' <OrderedExpression>
        RULE_ORDEREDEXPRESSIONLIST = 202, // <OrderedExpressionList> ::= <OrderedExpression>
        RULE_INSERTQUERY_INSERT_INTO_VALUES = 203, // <InsertQuery> ::= INSERT INTO <CollectionName> <AtrList> VALUES <BinaryExprList>
        RULE_UPDATEQUERY_UPDATE_SET = 204, // <UpdateQuery> ::= UPDATE <CollectionName> SET <QueryValuesOptBrk> <UpdateWhereSection>
        RULE_UPDATEWHERESECTION = 205, // <UpdateWhereSection> ::= <WhereSection>
        RULE_DELETEQUERY_DELETE = 206, // <DeleteQuery> ::= DELETE <DeleteFromSection> <DeleteWhereSection>
        RULE_DELETEFROMSECTION = 207, // <DeleteFromSection> ::= <FromSection>
        RULE_DELETEWHERESECTION = 208, // <DeleteWhereSection> ::= <WhereSection>
        RULE_CREATESTATEMENT_CREATE = 209, // <CreateStatement> ::= CREATE <ObjectType> <DDLConfiguration>
        RULE_ALTERSTATEMENT_ALTER = 210, // <AlterStatement> ::= ALTER <ObjectType> <DDLConfiguration>
        RULE_DROPSTATEMENT_DROP = 211, // <DropStatement> ::= DROP <ObjectType> <DDLConfiguration>
        RULE_TRUNCATESTATEMENT_TRUNCATE_COLLECTION = 212, // <TruncateStatement> ::= TRUNCATE COLLECTION <CollectionName>
        RULE_BACKUPSTATEMENT_BACKUP_DATABASE = 213, // <BackupStatement> ::= BACKUP DATABASE <DDLIdentifierConfig>
        RULE_RESTORESTATEMENT_RESTORE_DATABASE = 214, // <RestoreStatement> ::= RESTORE DATABASE <DDLIdentifierConfig>
        RULE_CONTROLSTATEMENT = 215, // <ControlStatement> ::= <EnableStatement>
        RULE_CONTROLSTATEMENT2 = 216, // <ControlStatement> ::= <DisableStatement>
        RULE_DISABLESTATEMENT = 217, // <DisableStatement> ::= 
        RULE_ENABLESTATEMENT = 218, // <EnableStatement> ::= 
        RULE_DCLOBJECT = 219, // <DCLObject> ::= <Identifier>
        RULE_DCLOBJECT_DOT = 220, // <DCLObject> ::= <Identifier> '.' <Identifier>
        RULE_GRANTSTATEMENT_GRANT_ON_TO = 221, // <GrantStatement> ::= GRANT <Identifier> ON <DCLObject> TO <StrLiteral>
        RULE_REVOKESTATEMENT_REVOKE_ON_FROM = 222  // <RevokeStatement> ::= REVOKE <Identifier> ON <DCLObject> FROM <StrLiteral>
    };

}