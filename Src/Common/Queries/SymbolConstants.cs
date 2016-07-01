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
    enum SymbolConstants : int
    {
        SYMBOL_EOF = 0, // (EOF)
        SYMBOL_ERROR = 1, // (Error)
        SYMBOL_WHITESPACE = 2, // (Whitespace)
        SYMBOL_MINUS = 3, // '-'
        SYMBOL_EXCLAMEQ = 4, // '!='
        SYMBOL_PERCENT = 5, // '%'
        SYMBOL_LPARAN = 6, // '('
        SYMBOL_RPARAN = 7, // ')'
        SYMBOL_TIMES = 8, // '*'
        SYMBOL_COMMA = 9, // ','
        SYMBOL_DOT = 10, // '.'
        SYMBOL_DIV = 11, // '/'
        SYMBOL_COLON = 12, // ':'
        SYMBOL_SEMI = 13, // ';'
        SYMBOL_LBRACKET = 14, // '['
        SYMBOL_RBRACKET = 15, // ']'
        SYMBOL_LBRACE = 16, // '{'
        SYMBOL_RBRACE = 17, // '}'
        SYMBOL_PLUS = 18, // '+'
        SYMBOL_LT = 19, // '<'
        SYMBOL_LTEQ = 20, // '<='
        SYMBOL_LTGT = 21, // '<>'
        SYMBOL_EQ = 22, // '='
        SYMBOL_EQEQ = 23, // '=='
        SYMBOL_GT = 24, // '>'
        SYMBOL_GTEQ = 25, // '>='
        SYMBOL_ADD = 26, // ADD
        SYMBOL_ALL = 27, // ALL
        SYMBOL_ALTER = 28, // ALTER
        SYMBOL_AND = 29, // AND
        SYMBOL_ANY = 30, // ANY
        SYMBOL_ARRAY = 31, // ARRAY
        SYMBOL_AS = 32, // AS
        SYMBOL_ASC = 33, // ASC
        SYMBOL_BACKUP = 34, // BACKUP
        SYMBOL_BETWEEN = 35, // BETWEEN
        SYMBOL_BY = 36, // BY
        SYMBOL_COLLECTION = 37, // COLLECTION
        SYMBOL_CONTAINS = 38, // CONTAINS
        SYMBOL_CREATE = 39, // CREATE
        SYMBOL_DATABASE = 40, // DATABASE
        SYMBOL_DATETIME = 41, // DateTime
        SYMBOL_DELETE = 42, // DELETE
        SYMBOL_DELIMITIDDOLLARS = 43, // DelimitIdDollars
        SYMBOL_DELIMITIDQUOTES = 44, // DelimitIdQuotes
        SYMBOL_DESC = 45, // DESC
        SYMBOL_DISTINCT = 46, // DISTINCT
        SYMBOL_DROP = 47, // DROP
        SYMBOL_EXEC = 48, // EXEC
        SYMBOL_EXECUTE = 49, // EXECUTE
        SYMBOL_EXISTS = 50, // EXISTS
        SYMBOL_FALSE = 51, // FALSE
        SYMBOL_FETCH = 52, // FETCH
        SYMBOL_FROM = 53, // FROM
        SYMBOL_GRANT = 54, // GRANT
        SYMBOL_GROUP = 55, // GROUP
        SYMBOL_HAVING = 56, // HAVING
        SYMBOL_HINT = 57, // HINT
        SYMBOL_IN = 58, // IN
        SYMBOL_INDEX = 59, // INDEX
        SYMBOL_INSERT = 60, // INSERT
        SYMBOL_INTEGERLITERAL = 61, // IntegerLiteral
        SYMBOL_INTO = 62, // INTO
        SYMBOL_IS = 63, // IS
        SYMBOL_KEY = 64, // KEY
        SYMBOL_LIKE = 65, // LIKE
        SYMBOL_LIMIT = 66, // LIMIT
        SYMBOL_LIMITID = 67, // LimitId
        SYMBOL_LOGIN = 68, // LOGIN
        SYMBOL_MASTER = 69, // MASTER
        SYMBOL_MATCH = 70, // MATCH
        SYMBOL_NEXT = 71, // NEXT
        SYMBOL_NOT = 72, // NOT
        SYMBOL_NULL = 73, // NULL
        SYMBOL_OFFSET = 74, // OFFSET
        SYMBOL_ON = 75, // ON
        SYMBOL_ONLY = 76, // ONLY
        SYMBOL_OR = 77, // OR
        SYMBOL_ORDER = 78, // ORDER
        SYMBOL_PARAMETERRULE = 79, // ParameterRule
        SYMBOL_REALLITERAL = 80, // RealLiteral
        SYMBOL_REMOVE = 81, // REMOVE
        SYMBOL_RENAME = 82, // RENAME
        SYMBOL_REPLACE = 83, // REPLACE
        SYMBOL_RESTORE = 84, // RESTORE
        SYMBOL_REVOKE = 85, // REVOKE
        SYMBOL_ROLE = 86, // ROLE
        SYMBOL_ROWS = 87, // ROWS
        SYMBOL_SELECT = 88, // SELECT
        SYMBOL_SET = 89, // SET
        SYMBOL_SIZE = 90, // SIZE
        SYMBOL_SKIP = 91, // SKIP
        SYMBOL_SLICE = 92, // SLICE
        SYMBOL_STRINGLITERAL = 93, // StringLiteral
        SYMBOL_TO = 94, // TO
        SYMBOL_TOP = 95, // TOP
        SYMBOL_TRUE = 96, // TRUE
        SYMBOL_TRUNCATE = 97, // TRUNCATE
        SYMBOL_UPDATE = 98, // UPDATE
        SYMBOL_USER = 99, // USER
        SYMBOL_VALUES = 100, // VALUES
        SYMBOL_WHERE = 101, // WHERE
        SYMBOL_ADDEXPRESSION = 102, // <AddExpression>
        SYMBOL_ALIAS = 103, // <Alias>
        SYMBOL_ALTERSTATEMENT = 104, // <AlterStatement>
        SYMBOL_ANDEXPR = 105, // <AndExpr>
        SYMBOL_ARRAYPROJECTION = 106, // <ArrayProjection>
        SYMBOL_ARRAYUPDATEOP = 107, // <ArrayUpdateOp>
        SYMBOL_ASEXPRESSIONS = 108, // <AsExpressions>
        SYMBOL_ATRLIST = 109, // <AtrList>
        SYMBOL_ATTRIBUTE = 110, // <Attribute>
        SYMBOL_ATTRIBUTELIST = 111, // <AttributeList>
        SYMBOL_ATTRIBUTEPAIR = 112, // <AttributePair>
        SYMBOL_BACKUPSTATEMENT = 113, // <BackupStatement>
        SYMBOL_BINARYEXPRESSION = 114, // <BinaryExpression>
        SYMBOL_BINARYEXPRESSIONLIST = 115, // <BinaryExpressionList>
        SYMBOL_BINARYEXPRLIST = 116, // <BinaryExprList>
        SYMBOL_COLLECTIONNAME = 117, // <CollectionName>
        SYMBOL_COMPAREEXPR = 118, // <CompareExpr>
        SYMBOL_CONTROLSTATEMENT = 119, // <ControlStatement>
        SYMBOL_CREATESTATEMENT = 120, // <CreateStatement>
        SYMBOL_DATE = 121, // <Date>
        SYMBOL_DCLOBJECT = 122, // <DCLObject>
        SYMBOL_DCLSTATEMENT = 123, // <DCLStatement>
        SYMBOL_DDLCONFIGURATION = 124, // <DDLConfiguration>
        SYMBOL_DDLIDENTIFIERCONFIG = 125, // <DDLIdentifierConfig>
        SYMBOL_DDLSTATEMENT = 126, // <DDLStatement>
        SYMBOL_DELETEFROMSECTION = 127, // <DeleteFromSection>
        SYMBOL_DELETEQUERY = 128, // <DeleteQuery>
        SYMBOL_DELETEWHERESECTION = 129, // <DeleteWhereSection>
        SYMBOL_DELIMITEDID = 130, // <DelimitedId>
        SYMBOL_DELIMITEDIDDOLLARS = 131, // <DelimitedIdDollars>
        SYMBOL_DELIMITIDQUOTES2 = 132, // <DelimitIdQuotes>
        SYMBOL_DISABLESTATEMENT = 133, // <DisableStatement>
        SYMBOL_DISTINCTRESTRICTION = 134, // <DistinctRestriction>
        SYMBOL_DMLSTATEMENT = 135, // <DMLStatement>
        SYMBOL_DROPSTATEMENT = 136, // <DropStatement>
        SYMBOL_ELEMENTS = 137, // <Elements>
        SYMBOL_ENABLESTATEMENT = 138, // <EnableStatement>
        SYMBOL_EXECUTESTATEMENT = 139, // <ExecuteStatement>
        SYMBOL_EXPRESSION = 140, // <Expression>
        SYMBOL_FROMSECTION = 141, // <FromSection>
        SYMBOL_FUNCEXPRESSIONS = 142, // <FuncExpressions>
        SYMBOL_FUNCTION = 143, // <Function>
        SYMBOL_FUNCTIONATTRGROUP = 144, // <FunctionAttrGroup>
        SYMBOL_FUNCTIONNAME = 145, // <FunctionName>
        SYMBOL_GRANTSTATEMENT = 146, // <GrantStatement>
        SYMBOL_GROUPSECTION = 147, // <GroupSection>
        SYMBOL_HAVINGSECTION = 148, // <HavingSection>
        SYMBOL_HINTPARAMETER = 149, // <HintParameter>
        SYMBOL_HINTSECTION = 150, // <HintSection>
        SYMBOL_IDENTIFIER = 151, // <Identifier>
        SYMBOL_INDEXER = 152, // <Indexer>
        SYMBOL_INSERTQUERY = 153, // <InsertQuery>
        SYMBOL_JSONARRAY = 154, // <JSONArray>
        SYMBOL_JSONDOCVALUE = 155, // <JSONdocValue>
        SYMBOL_JSONMEMBERS = 156, // <JSONMembers>
        SYMBOL_JSONOBJECT = 157, // <JSONObject>
        SYMBOL_JSONSTRING = 158, // <JSONString>
        SYMBOL_JSONVALLIST = 159, // <JSONValList>
        SYMBOL_JSONVALUE = 160, // <JSONValue>
        SYMBOL_JSONVALUELIST = 161, // <JSONValueList>
        SYMBOL_LIMITEDID = 162, // <LimitedId>
        SYMBOL_LOGICALOPERATOR = 163, // <LogicalOperator>
        SYMBOL_MULTEXPRESSION = 164, // <MultExpression>
        SYMBOL_MULTISTATEMENT = 165, // <MultiStatement>
        SYMBOL_NUMLITERAL = 166, // <NumLiteral>
        SYMBOL_OBJECTTYPE = 167, // <ObjectType>
        SYMBOL_OFFSETSECTION = 168, // <OffsetSection>
        SYMBOL_OPTTERMINATOR = 169, // <OptTerminator>
        SYMBOL_ORDER2 = 170, // <Order>
        SYMBOL_ORDEREDEXPRESSION = 171, // <OrderedExpression>
        SYMBOL_ORDEREDEXPRESSIONLIST = 172, // <OrderedExpressionList>
        SYMBOL_ORDERSECTION = 173, // <OrderSection>
        SYMBOL_OREXPR = 174, // <OrExpr>
        SYMBOL_PARAMETER = 175, // <Parameter>
        SYMBOL_PARAMINTEGER = 176, // <ParamInteger>
        SYMBOL_PARAMLIST = 177, // <ParamList>
        SYMBOL_PARENSEXPRESSION = 178, // <ParensExpression>
        SYMBOL_QUERYVALUES = 179, // <QueryValues>
        SYMBOL_QUERYVALUESOPTBRK = 180, // <QueryValuesOptBrk>
        SYMBOL_REPLACELIST = 181, // <ReplaceList>
        SYMBOL_RESTORESTATEMENT = 182, // <RestoreStatement>
        SYMBOL_REVOKESTATEMENT = 183, // <RevokeStatement>
        SYMBOL_SELECTATTRIBUTES = 184, // <SelectAttributes>
        SYMBOL_SELECTFROMSECTION = 185, // <SelectFromSection>
        SYMBOL_SELECTQUERY = 186, // <SelectQuery>
        SYMBOL_SELECTWHERESECTION = 187, // <SelectWhereSection>
        SYMBOL_SINGLESTATMENT = 188, // <SingleStatment>
        SYMBOL_SPSTATEMENT = 189, // <SPStatement>
        SYMBOL_STATEMENT = 190, // <Statement>
        SYMBOL_STRLITERAL = 191, // <StrLiteral>
        SYMBOL_TOPSECTION = 192, // <TopSection>
        SYMBOL_TRUNCATESTATEMENT = 193, // <TruncateStatement>
        SYMBOL_UNARYEXPR = 194, // <UnaryExpr>
        SYMBOL_UPDATEQUERY = 195, // <UpdateQuery>
        SYMBOL_UPDATEWHERESECTION = 196, // <UpdateWhereSection>
        SYMBOL_VALUE = 197, // <Value>
        SYMBOL_VALUEEXPRESSION = 198, // <ValueExpression>
        SYMBOL_VALUESIGN = 199, // <ValueSign>
        SYMBOL_WHERESECTION = 200  // <WhereSection>
    };

}