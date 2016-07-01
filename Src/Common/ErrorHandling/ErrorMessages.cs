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
using Alachisoft.NosDB.Common.DataStructures.Clustered;

namespace Alachisoft.NosDB.Common.ErrorHandling
{
	/// <summary>
	/// Helper class for building error messages. This class contains mapping for error code to messages
	/// </summary>
    public class ErrorMessages
	{
		private static IDictionary<int, string> s_errorMessageMap = new HashVector<int, string>();

		static ErrorMessages()
		{
			/* Error messages against a given error code shoulde be
             * added to the following table. Every error code should have corresponding error message
             * in this table.
             */
            s_errorMessageMap.Add(ErrorCodes.MISSING_PARTITION_KEY, "Partition key is missing in json document");

			#region Query System Exception Messages

            s_errorMessageMap.Add(ErrorCodes.Query.UNKNOWN_ISSUE, "Failed due to unknown reason(s). ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_SYNTAX, "Invalid query syntax specified. Syntax Error near '{0}' on line number: {1}");
			s_errorMessageMap.Add(ErrorCodes.Query.NOT_SUPPORTED, "Query type is not supported.");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_AGGREGATE_FUNCTION_ARGUMENTS, "Aggregate function '{0}' has some invalid arguments. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_NUMBER_OF_AGGREGATE_FUNCTION_ARGUMENTS, "Aggregate function '{0}' has invalid number of arguments. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_FUNCTION_NAME_SPECIFIED, "No definition could be found for function {0}.  ");
			s_errorMessageMap.Add(ErrorCodes.Query.UNASSIGNED_QUERY_PARAMETER, "Query parameter '{0}' is unassigned. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_ARRAY_INDEX, "Invalid array index specified {0}. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_ARITHMETIC_OPERATOR_WITH_CONSTANT, "Invalid binary expression constant specified {0}. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_NUMBER_OF_INSERT_PARAMETERS, "Invalid number of insert query values specified. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_INSERT_QUERY_CONSTANT_VALUE, "Invalid insert query value specified. '{0}' ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_INSERT_QUERY_ATTRIBUTE, "An attribute/key is expected at the left side of the assignment. '{0}' ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_INSERT_QUERY_ATTRIBUTE_CONFLICT, "Attribute '{0}' is specified multiple times. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_NUMBER_OF_ARRAY_RANGE_ELEMENTS, "Invalid array range value is specified.");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_IN_OPERATOR_ARGUMENTS, "Failed due to unknown reasons. ");
			s_errorMessageMap.Add(ErrorCodes.Query.QUERYCRITERIA_FIELD_ALREADY_EXISTS, "The given projection already exists in the selected criteria. ");
			s_errorMessageMap.Add(ErrorCodes.Query.PREDICATOR_NOT_EXECUTED, "The query finalizer could not be executed. ");
			s_errorMessageMap.Add(ErrorCodes.Query.AGGREGATION_INVALID_FUNCTION, "Invalid aggregation requested. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_BETWEEN_OPERATOR_ARGUMENTS, "The specified BETWEEN range is invalid, the result will be empty. ");
			s_errorMessageMap.Add(ErrorCodes.Query.ARRAY_FOUND_IN_ORDERBY, "An Order By clause cannot be run on an attribute value of type array. ");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_SCALAR_FUNCTION_ARGUMENTS, "Scalar function: '{0}' has invalid arguments.");
			s_errorMessageMap.Add(ErrorCodes.Query.INVALID_NUMBER_OF_SCALAR_FUNCTION_ARGUMENTS, "Scalar function: '{0}' has invalid number of arguments.");
            s_errorMessageMap.Add(ErrorCodes.Query.INVALID_DDL_CONFIGURATION_JSON, "Invalid Data definition configuration provided in JSON document.");
            s_errorMessageMap.Add(ErrorCodes.Query.INVALID_CONSTANT_BINARY_EXPRESSION_SPECIFIED, "Invalid constant binary expression provided for execution.");
            s_errorMessageMap.Add(ErrorCodes.Query.INVALID_NON_QUERY_TYPE, "Invalid query type specified in ExecuteNonQuery().");
            s_errorMessageMap.Add(ErrorCodes.Query.INVALID_DDL_JSON_KEY_USAGE, "Reserved Configuration JSON key specified.");
            s_errorMessageMap.Add(ErrorCodes.Query.INVALID_SINGLE_ATTRIBUTE_ARGUMENT, "Binary Expression is specified in uniary predicate for attribute.");
            s_errorMessageMap.Add(ErrorCodes.Query.INVALID_CONSTANT_FUNCTION_SPECIFIED, "Variable scalar function specified in the place of constant function");
            s_errorMessageMap.Add(ErrorCodes.Query.ATTRIBUTE_NULL_OR_EMPTY,"The specified attribute is either null or empty. ");
            s_errorMessageMap.Add(ErrorCodes.Query.INVALID_ATTRIBUTE,"The specified attribute format is invalid. ");
            s_errorMessageMap.Add(ErrorCodes.Query.PARAMETER_NOT_SUPPORTED, "Specified parameter '{0}' is invalid. Error {1}");
            s_errorMessageMap.Add(ErrorCodes.Query.PREFIX_COMPARISON_LIST_MISMATCH,"The prefix cannot be checked against anything eles than a list");
            s_errorMessageMap.Add(ErrorCodes.Query.DISTICT_NOT_SUPPORTED, "Invalid syntax sepcified. Distinct is not supported for function argument");
            s_errorMessageMap.Add(ErrorCodes.Query.INVAILD_ARAY_ITEM, "Invalid array item specified '{0}'.");
            s_errorMessageMap.Add(ErrorCodes.Query.INVALID_DDL_CONFIGURATION, "Invalid configuration specified. Error {0}");

			#endregion

            #region Cache Exception Messages
            s_errorMessageMap.Add(ErrorCodes.Cache.UNKNOWN_ISSUE, "Failed due to unknown reason(s).");
            s_errorMessageMap.Add(ErrorCodes.Cache.GENERAL_PROBLEM, "General failure in Caching system.");
            #endregion

            #region Indexes Exception Messages

            s_errorMessageMap.Add(ErrorCodes.Indexes.UNKNOWN_ISSUE, "Index failed with unknown issue");
            s_errorMessageMap.Add(ErrorCodes.Indexes.TREE_INITIALIZATION_FAILURE, "At index, failed to initialize tree");
            s_errorMessageMap.Add(ErrorCodes.Indexes.NUMERIC_BOUNDS_CALCULATION_FAILURE, "At index, Numeric bound calculation failure");
            s_errorMessageMap.Add(ErrorCodes.Indexes.TREE_COMMIT_FAILURE, "At index, failed to commit the tree");
            s_errorMessageMap.Add(ErrorCodes.Indexes.TREE_ROLLBACK_FAILURE, "At index, failed to rollback the tree");
            s_errorMessageMap.Add(ErrorCodes.Indexes.INDEX_ALREADY_DEFINED, "An index already exists with name {0}");
            s_errorMessageMap.Add(ErrorCodes.Indexes.ENUMERATION_NOT_POSSIBLE,"The enumeration on MultiAttributeIndex is not possible because the provided criteria is not a MultiAttributeValue");
            s_errorMessageMap.Add(ErrorCodes.Indexes.INDEX_ALREADY_DEFINED_FOR_ATTRIBUTES,"An index already exists for attributes {0}");
            s_errorMessageMap.Add(ErrorCodes.Indexes.INDEX_DOESNOT_EXIST, "The specified index {0} doesnot exist");
            s_errorMessageMap.Add(ErrorCodes.Indexes.ATTRIBUTEVALUE_TYPE_MISMATCH, "The comparison attributes {0} and {1} cannot be compared.");
			#endregion

            #region Distributor Exception Messages
            s_errorMessageMap.Add(ErrorCodes.Distributor.UNKNOWN_ISSUE, "Unknown Exception occurred inside Distributor. {0}");
            s_errorMessageMap.Add(ErrorCodes.Distributor.SHARD_READ_ONLY, "The Shard '{0}' is in readonly mode. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.SHARD_CONFIGURATION_NULL, "Shard: '{0}' configuration is null. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.CONNECTION_DOESNOT_EXIST, "Connection does not exist for '{0}'. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.UNKNOWN_CHANNEL_SEND_ISSUE, "Unknown Exception occurred while sending operation to Shard '{0}'. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.NOT_CONNECTED_TO_SHARD, "Not Connected to Shard '{0}'. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.NO_CHANNEL_EXISTS, "No Connection Exists for '{0}'. Shard Name = '{1}')");
            s_errorMessageMap.Add(ErrorCodes.Distributor.NO_SECONDARY_NODE, "No Secondary exists for Shard '{0}'. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.CHANNEL_CONNECT_FAILED, "Shard '{0}' failed to establish connection with {1}:{2}");
            s_errorMessageMap.Add(ErrorCodes.Distributor.SEND_DESTINATION_UNSPECIFIED, "The destination address was unspecified");
            s_errorMessageMap.Add(ErrorCodes.Distributor.MISSING_CONNECTION_STRING, "Missing Connection String");
            s_errorMessageMap.Add(ErrorCodes.Distributor.CLUSTER_INFO_UNAVAILABLE, "ClusterInfo was unavailable");
            s_errorMessageMap.Add(ErrorCodes.Distributor.SHARD_INFO_UNAVAILABLE, "ShardInfo was unavailable for Cluster: '{0}'");
            s_errorMessageMap.Add(ErrorCodes.Distributor.DATABASE_DOESNOT_EXIST, "Invalid database. The database '{0}' does not exist. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.INVALID_OPERATION, "Invalid Operation. Please provide an operation. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.CONFIG_SESSION_NOT_AVAILABLE, "Please Initialize Database before performing any operation. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.INVALID_READER_ID, "Cannot get next chunk because the reader id was null or invalid. Reader might not have been created or got disposed. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.UNEXPECTED_SHARD_DOESNOT_EXIST, "This is not an expected behaviour. Shard connection must exist. No Connection for Shard '{0}'. Make sure that the shard is configured.");
            s_errorMessageMap.Add(ErrorCodes.Distributor.COLLECTION_DOESNOT_EXIST, "Specified collection '{0}' does not exist in database '{1}'.");
            s_errorMessageMap.Add(ErrorCodes.Distributor.CHANNEL_NOT_RESPONDING, "'{0}' is not responding or may be down. ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.QUERY_OPERATION_BY_NONQUERY, "Query cannot be executed by ExecuteNonQuery() ");
            s_errorMessageMap.Add(ErrorCodes.Distributor.NONQUERY_OPERATION_BY_QUERY, "NonQuery cannot be executed by ExecuteReader()");
            s_errorMessageMap.Add(ErrorCodes.Distributor.TIMEOUT, "Request timeout at '{0}'");
            s_errorMessageMap.Add(ErrorCodes.Distributor.UNKNOWN_GETQUERYCHUNK_EXCEPTION, "While bring next chunk for Query ReaderID {0}, at Address '{1}, at Shard '{2}'");
            s_errorMessageMap.Add(ErrorCodes.Distributor.NOT_SELECT_QUERY, "'{0}' is not a valid query for ExecuteReader");
            s_errorMessageMap.Add(ErrorCodes.Distributor.DESTINATION_NULL, "Destination for sending the message cannot be null. Make sure that the shard '{0}' is running");
            s_errorMessageMap.Add(ErrorCodes.Distributor.PRIMARY_DOESNOT_EXIST, "No primary exists for shard '{0}'");
            s_errorMessageMap.Add(ErrorCodes.Distributor.DATABASE_NOT_INITIALIZED, "Database '{0}' being used is not initialized.");
            s_errorMessageMap.Add(ErrorCodes.Distributor.FAILED_TO_COMMUNICATE_WITH_SHARD, "Distributor failed to communicate with shard: '{0}'. Make sure that it is running and try again");
            s_errorMessageMap.Add(ErrorCodes.Distributor.CONFIGURATION_SERVER_NOTRESPONDING,"Failed to connect with the configuration server. Make sure that the configuration server is running.");
            s_errorMessageMap.Add(ErrorCodes.Distributor.COLLECTION_DOES_NOT_EXIST, "Specified collection '{0}' does not exist in database '{1}'.");
            s_errorMessageMap.Add(ErrorCodes.Distributor.INVALID_CONNECTION_STRING, "Invalid connection string specified. {0}");
            s_errorMessageMap.Add(ErrorCodes.Distributor.FAILED_TO_GET_SHARD, "Failed to get shard. The shard against this document might be removed. Please try the operation again");
			s_errorMessageMap.Add(ErrorCodes.Distributor.DATA_TYPE_NOT_SUPPORTED, "Data Type '{0}' is not supported.");
            s_errorMessageMap.Add(ErrorCodes.Distributor.INVALID_DOCUMENT_KEY, "Document Key invalid or null");
            s_errorMessageMap.Add(ErrorCodes.Distributor.NO_SHARD_EXIST, "No shard exist.");
            s_errorMessageMap.Add(ErrorCodes.Distributor.NO_RUNNING_NODE, "There is no running node in the cluster.");
            
			#endregion

            #region Security Exception Messages

            s_errorMessageMap.Add(ErrorCodes.Security.INVALID_TOKEN, "Token specified is invalid.");
            s_errorMessageMap.Add(ErrorCodes.Security.REMOTE_CLIENT_WITH_LOCAL_SERVER, "Server is configured for local connection but a remote connection was tried.");
            s_errorMessageMap.Add(ErrorCodes.Security.CERTIFICATE_ERROR, "Certificate provided for authentication was invalid/incomplete/corrupt.");
            s_errorMessageMap.Add(ErrorCodes.Security.NO_AUTHENTICATING_AUTHROITY, "The current security package cannot contact an authenticating authority.");
            s_errorMessageMap.Add(ErrorCodes.Security.TARGET_UNKNOWN, "The specified principle is not known in the authentication system.");
            s_errorMessageMap.Add(ErrorCodes.Security.UNAUTHORIZED_USER, "Authorization Failed: User '{0}' is not authorized to perform the operation '{1}' on '{2}'.");
            s_errorMessageMap.Add(ErrorCodes.Security.INVALID_ROLE, "Invalid Role: Role specified is not a valid built-in/custom role.");
            s_errorMessageMap.Add(ErrorCodes.Security.USER_ALREADY_EXIST, "User '{0}' already exists.");
            s_errorMessageMap.Add(ErrorCodes.Security.NO_USER_EXIST, "Specified user '{0}' does not exist.");
            s_errorMessageMap.Add(ErrorCodes.Security.NO_RESOURCE_EXIST, "No such resource exist, make sure name is entered correctly. Resource: {0}");
            s_errorMessageMap.Add(ErrorCodes.Security.USER_NOT_REGISTERED, "Login '{0}' does not exist");
            s_errorMessageMap.Add(ErrorCodes.Security.INVALID_WINDOWS_USER, "Username specified is not a valid window user: {0}");
            s_errorMessageMap.Add(ErrorCodes.Security.UNAUTHENTIC_DB_SERVER_CONNECTION, "User was not authenticated on shard: '{0}' at server: {1}");
            s_errorMessageMap.Add(ErrorCodes.Security.MIXED_MODE_AUTH_DISABLED, "Authentication failed: Mixed mode disabled");
            s_errorMessageMap.Add(ErrorCodes.Security.DATABASE_OR_CLUSTER_USER, "Login '{0}' is added as user in following resources:\n{1}\nRemove associations first before dropping the login");
            s_errorMessageMap.Add(ErrorCodes.Security.SSPI_ERROR, "Internal Error: {0}");
            s_errorMessageMap.Add(ErrorCodes.Security.LOGIN_NOT_EXIST, "Login '{0}' not found, please reconnect.");
            s_errorMessageMap.Add(ErrorCodes.Security.ERROR_READING_REGISTRY, "Unable to read registery.");
            s_errorMessageMap.Add(ErrorCodes.Security.NO_CREDENTIALS, "no credentials exist for user '{0}'");
            s_errorMessageMap.Add(ErrorCodes.Security.LAST_SYSTEM_USER, "Unable to revoke role as it is the last user");
            s_errorMessageMap.Add(ErrorCodes.Security.CONFIG_NOT_FOUND, "Config file not found at '{0}'");
            s_errorMessageMap.Add(ErrorCodes.Security.PATH_NOT_FOUND, "Security Config Path not found in service config");
            s_errorMessageMap.Add(ErrorCodes.Security.VALUE_NOT_FOUND, "Value for '{0}' not found in config");
            s_errorMessageMap.Add(ErrorCodes.Security.INVALID_PASSWORD, "Password specified is invalid. Password must not be empty, must contain at least 8 alphanumeric characters and password length must be less than 128");
            s_errorMessageMap.Add(ErrorCodes.Security.INVALID_USER, "Username specified contains invalid characters.");
            s_errorMessageMap.Add(ErrorCodes.Security.INVALID_DOMAIN_USERNAME, @"Invalid windows username specified. Give the complete name <domain\username>");

            #endregion

            #region Collection Exception Messages

            s_errorMessageMap.Add(ErrorCodes.Collection.UNKNOWN_ISSUE, "Failed due to unknown reason(s).{0}");
            s_errorMessageMap.Add(ErrorCodes.Collection.KEY_DOES_NOT_EXISTS, "Document Key '{0}' does not exists in collection '{1}'.");
            s_errorMessageMap.Add(ErrorCodes.Collection.KEY_ALREADY_EXISTS, "Document Key '{0}' already exists in collection '{1}'.");
            s_errorMessageMap.Add(ErrorCodes.Collection.DELETE_NOT_ALLOWED_CAPPED, "Delete operation is not allowed on capped collection.");
            s_errorMessageMap.Add(ErrorCodes.Collection.DOCUMENT_DOES_NOT_EXIST,"Document does't exist in the database");
            s_errorMessageMap.Add(ErrorCodes.Collection.COLLECTION_DISPODED,"The collection '{0}' has been disposed. ");
            s_errorMessageMap.Add(ErrorCodes.Collection.COLLECTION_DOESNOT_EXIST, "Specified collection '{0}' does not exist in database '{1}'.");
            s_errorMessageMap.Add(ErrorCodes.Collection.COLLECTION_OPERATION_NOTALLOWED, "Operations are not allowed on collection '{0}'. Collection Status = '{1}'.");
            s_errorMessageMap.Add(ErrorCodes.Collection.BUCKET_UNAVAILABLE, "The data could not be inserted because data bucket is unavailable due to state transfer. Retry later ");
            s_errorMessageMap.Add(ErrorCodes.Collection.DISTRIBUTION_NOT_SET, "Distribution is null.");
            
            //Attachment specific
            s_errorMessageMap.Add(ErrorCodes.Collection.FILE_NOT_FOUND, "File does not exist. server id: {0}");
            s_errorMessageMap.Add(ErrorCodes.Collection.EXCEPTION_FROM_PROVIDER, "Failed due to error in provider: {0}");
            s_errorMessageMap.Add(ErrorCodes.Collection.ATTACHMENT_NOT_FOUND, "Attachment does not exist with attachment id: {0}");
            s_errorMessageMap.Add(ErrorCodes.Collection.ERROR_INSERTING_METADATA, "Error inserting metadata for attachment id: {0}");
            s_errorMessageMap.Add(ErrorCodes.Collection.COLLECTION_NOT_AVAILABLE, "The collection {0} is not available anymore");

            #endregion

            #region Cluster Exception Messages
            s_errorMessageMap.Add(ErrorCodes.Cluster.UNKNOWN_ISSUE, "Failed due to unknown reason(s).");
            s_errorMessageMap.Add(ErrorCodes.Cluster.DESTINATION_NULL, "Destination cannot be null.");
            s_errorMessageMap.Add(ErrorCodes.Cluster.SERVER_NOT_EXIST, "No server exist in servers list.");           
            s_errorMessageMap.Add(ErrorCodes.Cluster.DESTINATION_SERVER_NOT_EXIST, "Destination:'{0}' does not exist in servers list");
            s_errorMessageMap.Add(ErrorCodes.Cluster.NOT_PRIMARY, "Shard:'{0}', Node:'{1}', the node is not primary");
            s_errorMessageMap.Add(ErrorCodes.Cluster.PRIMARY_ALREADY_EXISTS, "The primary for the shard already exists.");
            s_errorMessageMap.Add(ErrorCodes.Cluster.DATABASE_MANAGER_DISPOSED, "Database Manager is disposed.");
            #endregion

            #region Database Exception Messages
            s_errorMessageMap.Add(ErrorCodes.Database.Mode, "Cannot initialize database:'{0}' in '{1}' mode.");
            s_errorMessageMap.Add(ErrorCodes.Database.PRIMARY_RESTRICTED_ACCESS, "Node: '{0}', primary access restricted");
            s_errorMessageMap.Add(ErrorCodes.Database.DATABASE_DOESNOT_EXIST, "Database: '{0}', does not exist");
            #endregion

            #region StateTransfer Exception Message

            s_errorMessageMap.Add(ErrorCodes.StateTransfer.PRIMARY_CHANGED, "'{0}' no more primary for shard '{1}' .");
            s_errorMessageMap.Add(ErrorCodes.StateTransfer.BUCKET_DESTINATION_CHANGED, "Node: '{0}', primary access restricted");
            s_errorMessageMap.Add(ErrorCodes.StateTransfer.SHARD_UNAVAILABLE, "Database: '{0}', does not exist");
            s_errorMessageMap.Add(ErrorCodes.StateTransfer.UNKNOWN_ISSUE, "Database: '{0}', does not exist");

            #endregion
        }

        /// <summary>
        /// Gets the error message against an error code. This method should be called for simple error messages which
        /// does not contain any contextual information.
        /// This method throws Exception if error code is not mapped.
        /// </summary>
        /// <param name="errorCode">Error code</param>
        /// <returns>Error message corresponding to the error</returns>
        public static string GetErrorMessage(int errorCode)
        {
            return ResolveError(errorCode, null);
        }

        /// <summary>
        /// Gets the error message against an error code. This method should be called for error messages which
        /// contains any contextual information. For e.g. "Collection {collection_name} does not exist"
        /// This method throws Exception if error code is not mapped.
        /// </summary>
        /// <param name="errorCode">Error code</param>
        /// <param name="parameters">Contextual information to format the error mesage</param>
        /// <returns></returns>
        public static string GetErrorMessage(int errorCode,string[] parameters)
        {
            return ResolveError(errorCode, parameters);
        }

        internal static string ResolveError(int errorCode, string[] parameters)
        {
            string message;
            if (s_errorMessageMap.TryGetValue(errorCode, out message))
            {
                if (parameters == null)
                    return message;
                return String.Format(message, parameters);
            }
            return String.Format("Missing error message for code ({0}) in error to exception map", new object[] { errorCode });
        }
    }
}
