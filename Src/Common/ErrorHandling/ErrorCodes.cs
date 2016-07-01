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
namespace Alachisoft.NosDB.Common.ErrorHandling
{
	/// <summary>
	/// This class contains Error codes defined as constants. Every error code defined in this class should
	/// have an mapping in <ref>Alachisoft.NosDB.Common.ErrorHandling.ErrorMessages</ref>ErrorMessages class
	/// </summary>
    public class ErrorCodes
	{
		/* 
        Error code ranges: 
        1.    Collection            10000-14999
        2.    Indexing              15000-19999
        3.    Querying              20000-24999
        5.    Triggers/UDF          30000-34999
        6.    Cache                 35000-39999
        7.    Operation Log         40000-44999
        8.    Security              45000-49999
        9.    Storage               50000-54999
        10.   Config                55000-59999
        11.   Distributor           60000-64999
        12.   Cluster               65000-69999
       
         */

		//A sample error code is defined. Please follow the naming convention for constants 
        public const int MISSING_PARTITION_KEY = 50009;

        public class StateTransfer
        {
            public const int UNKNOWN_ISSUE = 75000;
            public const int PRIMARY_CHANGED = 75001;
            public const int BUCKET_DESTINATION_CHANGED = 75002;
            public const int SHARD_UNAVAILABLE = 75003;
        }

	    public class Cluster 
        {            
            public const int UNKNOWN_ISSUE = 65000;
            public const int DESTINATION_NULL = 65001;
            public const int SERVER_NOT_EXIST = 65002;
            public const int DESTINATION_SERVER_NOT_EXIST = 65003;
            public const int NOT_PRIMARY = 65004;
            public const int PRIMARY_ALREADY_EXISTS = 65005;
            public const int DATABASE_MANAGER_DISPOSED = 65006;

        }

		public class Indexes
		{
			public const int UNKNOWN_ISSUE = 15000;
			public const int TREE_INITIALIZATION_FAILURE = 15001;
			public const int NUMERIC_BOUNDS_CALCULATION_FAILURE = 15002;
			public const int TREE_COMMIT_FAILURE = 15003;
			public const int TREE_ROLLBACK_FAILURE = 15004;
            public const int INDEX_ALREADY_DEFINED = 15005;
            public const int ENUMERATION_NOT_POSSIBLE = 15006;
            public const int INDEX_ALREADY_DEFINED_FOR_ATTRIBUTES = 15007;
		    public const int INDEX_DOESNOT_EXIST = 15008;
		    public const int ATTRIBUTEVALUE_TYPE_MISMATCH = 15009;
		}

		public class Query
		{
			public const int UNKNOWN_ISSUE = 20000;
			public const int INVALID_SYNTAX = 20001;
			public const int NOT_SUPPORTED = 20002;
			public const int INVALID_SCALAR_FUNCTION_ARGUMENTS = 20003;
			public const int INVALID_NUMBER_OF_SCALAR_FUNCTION_ARGUMENTS = 20004;
			public const int INVALID_AGGREGATE_FUNCTION_ARGUMENTS = 20005;
			public const int INVALID_NUMBER_OF_AGGREGATE_FUNCTION_ARGUMENTS = 20006;
			public const int INVALID_FUNCTION_NAME_SPECIFIED = 20007;
			public const int UNASSIGNED_QUERY_PARAMETER = 20008;
			public const int INVALID_ARRAY_INDEX = 20009;
			public const int INVALID_ARITHMETIC_OPERATOR_WITH_CONSTANT = 20010;
			public const int INVALID_NUMBER_OF_INSERT_PARAMETERS = 20011;
			public const int INVALID_INSERT_QUERY_CONSTANT_VALUE = 20012;
			public const int INVALID_INSERT_QUERY_ATTRIBUTE = 20013;
			public const int INVALID_INSERT_QUERY_ATTRIBUTE_CONFLICT = 20014;
			public const int INVALID_NUMBER_OF_ARRAY_RANGE_ELEMENTS = 20015;
			public const int INVALID_IN_OPERATOR_ARGUMENTS = 20016;
            public const int INVALID_CONSTANT_BINARY_EXPRESSION_SPECIFIED = 20017;
            public const int INVALID_SINGLE_ATTRIBUTE_ARGUMENT = 20018;
		    public const int INVALID_CONSTANT_FUNCTION_SPECIFIED = 20019;

		    public const int INVALID_NON_QUERY_TYPE = 21000;
		    public const int INVALID_DDL_JSON_KEY_USAGE = 21001;
            public const int INVALID_DDL_CONFIGURATION_JSON = 21002;

			public const int QUERYCRITERIA_FIELD_ALREADY_EXISTS = 22500;
			public const int PREDICATOR_NOT_EXECUTED = 22501;
			public const int AGGREGATION_INVALID_FUNCTION = 22502;
			public const int INVALID_BETWEEN_OPERATOR_ARGUMENTS = 22503;
			public const int ARRAY_FOUND_IN_ORDERBY = 22504;
		    public const int ATTRIBUTE_NULL_OR_EMPTY = 22505;
		    public const int INVALID_ATTRIBUTE = 22506;
            public const int PARAMETER_NOT_SUPPORTED = 22507;
		    public const int PREFIX_COMPARISON_LIST_MISMATCH = 22508;
            public const int DISTICT_NOT_SUPPORTED = 22509;

            public const int INVAILD_ARAY_ITEM = 22513;
            public const int INVALID_DDL_CONFIGURATION = 22514;


		}

		public class Distributor
		{
		    public const int UNKNOWN_ISSUE = 60000;
			public const int SHARD_READ_ONLY = 60001;
			public const int SHARD_CONFIGURATION_NULL = 60002;
			public const int CONNECTION_DOESNOT_EXIST = 60003;
			public const int UNKNOWN_CHANNEL_SEND_ISSUE = 60004;
			public const int NOT_CONNECTED_TO_SHARD = 60005;
			public const int NO_CHANNEL_EXISTS = 60006;
			public const int NO_SECONDARY_NODE = 60007;
			public const int CHANNEL_CONNECT_FAILED = 60008;
			public const int SEND_DESTINATION_UNSPECIFIED = 60009;
            public const int MISSING_CONNECTION_STRING = 60010;
            public const int CLUSTER_INFO_UNAVAILABLE = 60011;
            public const int SHARD_INFO_UNAVAILABLE = 60012;
            public const int DATABASE_DOESNOT_EXIST = 60013;
            public const int INVALID_OPERATION = 60014;
            public const int CONFIG_SESSION_NOT_AVAILABLE = 60015;
            public const int INVALID_READER_ID = 60016;
            public const int UNEXPECTED_SHARD_DOESNOT_EXIST = 60017;
            public const int COLLECTION_DOESNOT_EXIST = 60018;
            public const int CHANNEL_NOT_RESPONDING = 60019;
            public const int QUERY_OPERATION_BY_NONQUERY = 60020;
            public const int NONQUERY_OPERATION_BY_QUERY = 60021;
            public const int TIMEOUT = 60022;
		    public const int UNKNOWN_GETQUERYCHUNK_EXCEPTION = 60023;
		    public const int NOT_SELECT_QUERY = 60024;
		    public const int DESTINATION_NULL = 60025;
		    public const int PRIMARY_DOESNOT_EXIST = 60026;
            public const int DATABASE_NOT_INITIALIZED = 60027;
            public const int FAILED_TO_COMMUNICATE_WITH_SHARD = 60028;
		    public const int CONFIGURATION_SERVER_NOTRESPONDING = 60029;
            public const int COLLECTION_DOES_NOT_EXIST = 60030;
            public const int INVALID_CONNECTION_STRING = 60031;
            public static int FAILED_TO_GET_SHARD = 60032;
            public static int DATA_TYPE_NOT_SUPPORTED = 60033;
            public static int INVALID_DOCUMENT_KEY = 60034;
            public static int NO_SHARD_EXIST = 60035;
		    public static int NO_RUNNING_NODE = 60036;


		}

	    public class Security
		{
			//45000 - 49999
            public const int UNKNOWN_ISSUE = 45000;
			public const int INVALID_TOKEN = 45001;
			public const int REMOTE_CLIENT_WITH_LOCAL_SERVER = 45002;
			public const int CERTIFICATE_ERROR = 45003;
			public const int NO_AUTHENTICATING_AUTHROITY = 45004;
			public const int TARGET_UNKNOWN = 45005;
            public const int UNAUTHORIZED_USER = 45006;
            public const int INVALID_ROLE = 45007;
            public const int USER_ALREADY_EXIST = 45008;
            public const int NO_USER_EXIST = 45009;
            public const int INVALID_RESOURCE = 45010;
            public const int NO_RESOURCE_EXIST = 45011;
            public const int USER_NOT_REGISTERED = 45012;
            public const int INVALID_WINDOWS_USER = 45013;
            public const int UNAUTHENTIC_DB_SERVER_CONNECTION = 45014;
            public const int MIXED_MODE_AUTH_DISABLED = 45015;
            public const int DATABASE_OR_CLUSTER_USER = 45016;
            public const int SSPI_ERROR = 45017;
            public const int LOGIN_NOT_EXIST = 45018;
            public const int ERROR_READING_REGISTRY = 45019;
            public const int NO_CREDENTIALS = 45020;
            public const int LAST_SYSTEM_USER = 45021;
            public const int CONFIG_NOT_FOUND = 45022;
            public const int PATH_NOT_FOUND = 45023;
            public const int VALUE_NOT_FOUND = 45024;
            public const int INVALID_PASSWORD = 45025;
            public const int INVALID_USER = 45026;
            public const int INVALID_DOMAIN_USERNAME = 45027;
		}

	    public class Collection
        {
            public const int UNKNOWN_ISSUE = 10000;
            public const int KEY_ALREADY_EXISTS = 10001;
            public const int KEY_DOES_NOT_EXISTS = 10002;
            public const int DELETE_NOT_ALLOWED_CAPPED = 10003;
            public const int DOCUMENT_DOES_NOT_EXIST = 10004;
            public const int COLLECTION_DISPODED = 10005;
            public const int COLLECTION_DOESNOT_EXIST = 10006;
            public const int COLLECTION_OPERATION_NOTALLOWED = 10007;
            public const int BUCKET_UNAVAILABLE = 10008;
            public const int DISTRIBUTION_NOT_SET = 10009;
            public const int COLLECTION_NOT_AVAILABLE = 10010;
            
            // Attachment specific
            public const int FILE_NOT_FOUND = 10100;
            public const int EXCEPTION_FROM_PROVIDER = 10101;
            public const int ATTACHMENT_NOT_FOUND = 10102;
            public const int ERROR_INSERTING_METADATA = 10103;

            
        }

        public class Cache
        {
            public const int UNKNOWN_ISSUE = 35000;
            public const int GENERAL_PROBLEM = 35001;
        }

	    public class Database
	    {
            public const int Mode = 14999;
            public const int PRIMARY_RESTRICTED_ACCESS = 14998;
            public const int DATABASE_DOESNOT_EXIST = 14997;
	    }
	}
}
