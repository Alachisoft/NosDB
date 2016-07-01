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
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.ProtocolBuffers;
using pbc = global::Google.ProtocolBuffers.Collections;
using pbd = global::Google.ProtocolBuffers.Descriptors;
using scg = global::System.Collections.Generic;
namespace Alachisoft.NosDB.Common.Protobuf {
  
  namespace Proto {
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public static partial class Response {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_Response__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.Response, global::Alachisoft.NosDB.Common.Protobuf.Response.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_Response__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static Response() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "Cg5SZXNwb25zZS5wcm90bxIgQWxhY2hpc29mdC5Ob3NEQi5Db21tb24uUHJv", 
              "dG9idWYaHUluc2VydERvY3VtZW50c1Jlc3BvbnNlLnByb3RvGh1EZWxldGVE", 
              "b2N1bWVudHNSZXNwb25zZS5wcm90bxoaR2V0RG9jdW1lbnRzUmVzcG9uc2Uu", 
              "cHJvdG8aFFVwZGF0ZVJlc3BvbnNlLnByb3RvGhhXcml0ZVF1ZXJ5UmVzcG9u", 
              "c2UucHJvdG8aF1JlYWRRdWVyeVJlc3BvbnNlLnByb3RvGhtDcmVhdGVTZXNz", 
              "aW9uUmVzcG9uc2UucHJvdG8aFkdldENodW5rUmVzcG9uc2UucHJvdG8aHlJl", 
              "cGxhY2VEb2N1bWVudHNSZXNwb25zZS5wcm90bxocQXV0aGVudGljYXRpb25S", 
              "ZXNwb25zZS5wcm90bxoaSW5pdERhdGFiYXNlUmVzcG9uc2UucHJvdG8ikgsK", 
              "CFJlc3BvbnNlEj0KBHR5cGUYASABKA4yLy5BbGFjaGlzb2Z0Lk5vc0RCLkNv", 
              "bW1vbi5Qcm90b2J1Zi5SZXNwb25zZS5UeXBlEhEKCXJlcXVlc3RJZBgCIAEo", 
              "AxIUCgxpc1N1Y2Nlc3NmdWwYAyABKAgSEQoJZXJyb3JDb2RlGAQgASgFEhMK", 
              "C2Vycm9yUGFyYW1zGAUgAygJEloKF2luc2VydERvY3VtZW50c1Jlc3BvbnNl", 
              "GAYgASgLMjkuQWxhY2hpc29mdC5Ob3NEQi5Db21tb24uUHJvdG9idWYuSW5z", 
              "ZXJ0RG9jdW1lbnRzUmVzcG9uc2USWgoXZGVsZXRlRG9jdW1lbnRzUmVzcG9u", 
              "c2UYByABKAsyOS5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90b2J1Zi5E", 
              "ZWxldGVEb2N1bWVudHNSZXNwb25zZRJUChRnZXREb2N1bWVudHNSZXNwb25z", 
              "ZRgIIAEoCzI2LkFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmLkdl", 
              "dERvY3VtZW50c1Jlc3BvbnNlEkgKDnVwZGF0ZVJlc3BvbnNlGAkgASgLMjAu", 
              "QWxhY2hpc29mdC5Ob3NEQi5Db21tb24uUHJvdG9idWYuVXBkYXRlUmVzcG9u", 
              "c2USUAoSd3JpdGVRdWVyeVJlc3BvbnNlGAogASgLMjQuQWxhY2hpc29mdC5O", 
              "b3NEQi5Db21tb24uUHJvdG9idWYuV3JpdGVRdWVyeVJlc3BvbnNlEk4KEXJl", 
              "YWRRdWVyeVJlc3BvbnNlGAsgASgLMjMuQWxhY2hpc29mdC5Ob3NEQi5Db21t", 
              "b24uUHJvdG9idWYuUmVhZFF1ZXJ5UmVzcG9uc2USVgoVQ3JlYXRlU2Vzc2lv", 
              "blJlc3BvbnNlGAwgASgLMjcuQWxhY2hpc29mdC5Ob3NEQi5Db21tb24uUHJv", 
              "dG9idWYuQ3JlYXRlU2Vzc2lvblJlc3BvbnNlEkwKEGdldENodW5rUmVzcG9u", 
              "c2UYDSABKAsyMi5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90b2J1Zi5H", 
              "ZXRDaHVua1Jlc3BvbnNlElwKGHJlcGxhY2VEb2N1bWVudHNSZXNwb25zZRgO", 
              "IAEoCzI6LkFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmLlJlcGxh", 
              "Y2VEb2N1bWVudHNSZXNwb25zZRJYChZhdXRoZW50aWNhdGlvblJlc3BvbnNl", 
              "GA8gASgLMjguQWxhY2hpc29mdC5Ob3NEQi5Db21tb24uUHJvdG9idWYuQXV0", 
              "aGVudGljYXRpb25SZXNwb25zZRJUChRpbml0RGF0YWJhc2VSZXNwb25zZRgQ", 
              "IAEoCzI2LkFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmLkluaXRE", 
              "YXRhYmFzZVJlc3BvbnNlIscCCgRUeXBlEhQKEElOU0VSVF9ET0NVTUVOVFMQ", 
              "ARIUChBERUxFVEVfRE9DVU1FTlRTEAISEQoNR0VUX0RPQ1VNRU5UUxADEgoK", 
              "BlVQREFURRAEEg8KC1dSSVRFX1FVRVJZEAUSDgoKUkVBRF9RVUVSWRAGEhUK", 
              "EUNSRUFURV9DT0xMRUNUSU9OEAcSEwoPRFJPUF9DT0xMRUNUSU9OEAgSEgoO", 
              "Q1JFQVRFX1NFU1NJT04QCRIQCgxEUk9QX1NFU1NJT04QChIQCgxDUkVBVEVf", 
              "SU5ERVgQCxIOCgpEUk9QX0lOREVYEAwSDQoJR0VUX0NIVU5LEA0SEgoORElT", 
              "UE9TRV9SRUFERVIQDhIVChFSRVBMQUNFX0RPQ1VNRU5UUxAPEhIKDkFVVEhF", 
              "TlRJQ0FUSU9OEBASEQoNSU5JVF9EQVRBQkFTRRARQjgKJGNvbS5hbGFjaGlz", 
            "b2Z0Lm5vc2RiLmNvbW1vbi5wcm90b2J1ZkIQUmVzcG9uc2VQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_Response__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_Response__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.Response, global::Alachisoft.NosDB.Common.Protobuf.Response.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_Response__Descriptor,
                  new string[] { "Type", "RequestId", "IsSuccessful", "ErrorCode", "ErrorParams", "InsertDocumentsResponse", "DeleteDocumentsResponse", "GetDocumentsResponse", "UpdateResponse", "WriteQueryResponse", "ReadQueryResponse", "CreateSessionResponse", "GetChunkResponse", "ReplaceDocumentsResponse", "AuthenticationResponse", "InitDatabaseResponse", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.InsertDocumentsResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.DeleteDocumentsResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.GetDocumentsResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.UpdateResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.WriteQueryResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.ReadQueryResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.GetChunkResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.ReplaceDocumentsResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationResponse.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.InitDatabaseResponse.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class Response : pb::GeneratedMessage<Response, Response.Builder> {
    private Response() { }
    private static readonly Response defaultInstance = new Response().MakeReadOnly();
    private static readonly string[] _responseFieldNames = new string[] { "CreateSessionResponse", "authenticationResponse", "deleteDocumentsResponse", "errorCode", "errorParams", "getChunkResponse", "getDocumentsResponse", "initDatabaseResponse", "insertDocumentsResponse", "isSuccessful", "readQueryResponse", "replaceDocumentsResponse", "requestId", "type", "updateResponse", "writeQueryResponse" };
    private static readonly uint[] _responseFieldTags = new uint[] { 98, 122, 58, 32, 42, 106, 66, 130, 50, 24, 90, 114, 16, 8, 74, 82 };
    public static Response DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override Response DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override Response ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.Response.internal__static_Alachisoft_NosDB_Common_Protobuf_Response__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<Response, Response.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.Response.internal__static_Alachisoft_NosDB_Common_Protobuf_Response__FieldAccessorTable; }
    }
    
    #region Nested types
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public static partial class Types {
      public enum Type {
        INSERT_DOCUMENTS = 1,
        DELETE_DOCUMENTS = 2,
        GET_DOCUMENTS = 3,
        UPDATE = 4,
        WRITE_QUERY = 5,
        READ_QUERY = 6,
        CREATE_COLLECTION = 7,
        DROP_COLLECTION = 8,
        CREATE_SESSION = 9,
        DROP_SESSION = 10,
        CREATE_INDEX = 11,
        DROP_INDEX = 12,
        GET_CHUNK = 13,
        DISPOSE_READER = 14,
        REPLACE_DOCUMENTS = 15,
        AUTHENTICATION = 16,
        INIT_DATABASE = 17,
      }
      
    }
    #endregion
    
    public const int TypeFieldNumber = 1;
    private bool hasType;
    private global::Alachisoft.NosDB.Common.Protobuf.Response.Types.Type type_ = global::Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.INSERT_DOCUMENTS;
    public bool HasType {
      get { return hasType; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.Response.Types.Type Type {
      get { return type_; }
    }
    
    public const int RequestIdFieldNumber = 2;
    private bool hasRequestId;
    private long requestId_;
    public bool HasRequestId {
      get { return hasRequestId; }
    }
    public long RequestId {
      get { return requestId_; }
    }
    
    public const int IsSuccessfulFieldNumber = 3;
    private bool hasIsSuccessful;
    private bool isSuccessful_;
    public bool HasIsSuccessful {
      get { return hasIsSuccessful; }
    }
    public bool IsSuccessful {
      get { return isSuccessful_; }
    }
    
    public const int ErrorCodeFieldNumber = 4;
    private bool hasErrorCode;
    private int errorCode_;
    public bool HasErrorCode {
      get { return hasErrorCode; }
    }
    public int ErrorCode {
      get { return errorCode_; }
    }
    
    public const int ErrorParamsFieldNumber = 5;
    private pbc::PopsicleList<string> errorParams_ = new pbc::PopsicleList<string>();
    public scg::IList<string> ErrorParamsList {
      get { return pbc::Lists.AsReadOnly(errorParams_); }
    }
    public int ErrorParamsCount {
      get { return errorParams_.Count; }
    }
    public string GetErrorParams(int index) {
      return errorParams_[index];
    }
    
    public const int InsertDocumentsResponseFieldNumber = 6;
    private bool hasInsertDocumentsResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse insertDocumentsResponse_;
    public bool HasInsertDocumentsResponse {
      get { return hasInsertDocumentsResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse InsertDocumentsResponse {
      get { return insertDocumentsResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse.DefaultInstance; }
    }
    
    public const int DeleteDocumentsResponseFieldNumber = 7;
    private bool hasDeleteDocumentsResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse deleteDocumentsResponse_;
    public bool HasDeleteDocumentsResponse {
      get { return hasDeleteDocumentsResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse DeleteDocumentsResponse {
      get { return deleteDocumentsResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.DefaultInstance; }
    }
    
    public const int GetDocumentsResponseFieldNumber = 8;
    private bool hasGetDocumentsResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse getDocumentsResponse_;
    public bool HasGetDocumentsResponse {
      get { return hasGetDocumentsResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse GetDocumentsResponse {
      get { return getDocumentsResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse.DefaultInstance; }
    }
    
    public const int UpdateResponseFieldNumber = 9;
    private bool hasUpdateResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse updateResponse_;
    public bool HasUpdateResponse {
      get { return hasUpdateResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse UpdateResponse {
      get { return updateResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.DefaultInstance; }
    }
    
    public const int WriteQueryResponseFieldNumber = 10;
    private bool hasWriteQueryResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse writeQueryResponse_;
    public bool HasWriteQueryResponse {
      get { return hasWriteQueryResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse WriteQueryResponse {
      get { return writeQueryResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.DefaultInstance; }
    }
    
    public const int ReadQueryResponseFieldNumber = 11;
    private bool hasReadQueryResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse readQueryResponse_;
    public bool HasReadQueryResponse {
      get { return hasReadQueryResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse ReadQueryResponse {
      get { return readQueryResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse.DefaultInstance; }
    }
    
    public const int CreateSessionResponseFieldNumber = 12;
    private bool hasCreateSessionResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse createSessionResponse_;
    public bool HasCreateSessionResponse {
      get { return hasCreateSessionResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse CreateSessionResponse {
      get { return createSessionResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.DefaultInstance; }
    }
    
    public const int GetChunkResponseFieldNumber = 13;
    private bool hasGetChunkResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse getChunkResponse_;
    public bool HasGetChunkResponse {
      get { return hasGetChunkResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse GetChunkResponse {
      get { return getChunkResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.DefaultInstance; }
    }
    
    public const int ReplaceDocumentsResponseFieldNumber = 14;
    private bool hasReplaceDocumentsResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse replaceDocumentsResponse_;
    public bool HasReplaceDocumentsResponse {
      get { return hasReplaceDocumentsResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse ReplaceDocumentsResponse {
      get { return replaceDocumentsResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse.DefaultInstance; }
    }
    
    public const int AuthenticationResponseFieldNumber = 15;
    private bool hasAuthenticationResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse authenticationResponse_;
    public bool HasAuthenticationResponse {
      get { return hasAuthenticationResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse AuthenticationResponse {
      get { return authenticationResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.DefaultInstance; }
    }
    
    public const int InitDatabaseResponseFieldNumber = 16;
    private bool hasInitDatabaseResponse;
    private global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse initDatabaseResponse_;
    public bool HasInitDatabaseResponse {
      get { return hasInitDatabaseResponse; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse InitDatabaseResponse {
      get { return initDatabaseResponse_ ?? global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _responseFieldNames;
      if (hasType) {
        output.WriteEnum(1, field_names[13], (int) Type, Type);
      }
      if (hasRequestId) {
        output.WriteInt64(2, field_names[12], RequestId);
      }
      if (hasIsSuccessful) {
        output.WriteBool(3, field_names[9], IsSuccessful);
      }
      if (hasErrorCode) {
        output.WriteInt32(4, field_names[3], ErrorCode);
      }
      if (errorParams_.Count > 0) {
        output.WriteStringArray(5, field_names[4], errorParams_);
      }
      if (hasInsertDocumentsResponse) {
        output.WriteMessage(6, field_names[8], InsertDocumentsResponse);
      }
      if (hasDeleteDocumentsResponse) {
        output.WriteMessage(7, field_names[2], DeleteDocumentsResponse);
      }
      if (hasGetDocumentsResponse) {
        output.WriteMessage(8, field_names[6], GetDocumentsResponse);
      }
      if (hasUpdateResponse) {
        output.WriteMessage(9, field_names[14], UpdateResponse);
      }
      if (hasWriteQueryResponse) {
        output.WriteMessage(10, field_names[15], WriteQueryResponse);
      }
      if (hasReadQueryResponse) {
        output.WriteMessage(11, field_names[10], ReadQueryResponse);
      }
      if (hasCreateSessionResponse) {
        output.WriteMessage(12, field_names[0], CreateSessionResponse);
      }
      if (hasGetChunkResponse) {
        output.WriteMessage(13, field_names[5], GetChunkResponse);
      }
      if (hasReplaceDocumentsResponse) {
        output.WriteMessage(14, field_names[11], ReplaceDocumentsResponse);
      }
      if (hasAuthenticationResponse) {
        output.WriteMessage(15, field_names[1], AuthenticationResponse);
      }
      if (hasInitDatabaseResponse) {
        output.WriteMessage(16, field_names[7], InitDatabaseResponse);
      }
      UnknownFields.WriteTo(output);
    }
    
    private int memoizedSerializedSize = -1;
    public override int SerializedSize {
      get {
        int size = memoizedSerializedSize;
        if (size != -1) return size;
        return CalcSerializedSize();
      }
    }
    
    private int CalcSerializedSize() {
      int size = memoizedSerializedSize;
      if (size != -1) return size;
      
      size = 0;
      if (hasType) {
        size += pb::CodedOutputStream.ComputeEnumSize(1, (int) Type);
      }
      if (hasRequestId) {
        size += pb::CodedOutputStream.ComputeInt64Size(2, RequestId);
      }
      if (hasIsSuccessful) {
        size += pb::CodedOutputStream.ComputeBoolSize(3, IsSuccessful);
      }
      if (hasErrorCode) {
        size += pb::CodedOutputStream.ComputeInt32Size(4, ErrorCode);
      }
      {
        int dataSize = 0;
        foreach (string element in ErrorParamsList) {
          dataSize += pb::CodedOutputStream.ComputeStringSizeNoTag(element);
        }
        size += dataSize;
        size += 1 * errorParams_.Count;
      }
      if (hasInsertDocumentsResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(6, InsertDocumentsResponse);
      }
      if (hasDeleteDocumentsResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(7, DeleteDocumentsResponse);
      }
      if (hasGetDocumentsResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(8, GetDocumentsResponse);
      }
      if (hasUpdateResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(9, UpdateResponse);
      }
      if (hasWriteQueryResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(10, WriteQueryResponse);
      }
      if (hasReadQueryResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(11, ReadQueryResponse);
      }
      if (hasCreateSessionResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(12, CreateSessionResponse);
      }
      if (hasGetChunkResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(13, GetChunkResponse);
      }
      if (hasReplaceDocumentsResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(14, ReplaceDocumentsResponse);
      }
      if (hasAuthenticationResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(15, AuthenticationResponse);
      }
      if (hasInitDatabaseResponse) {
        size += pb::CodedOutputStream.ComputeMessageSize(16, InitDatabaseResponse);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static Response ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Response ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Response ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Response ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Response ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Response ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static Response ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static Response ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static Response ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Response ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private Response MakeReadOnly() {
      errorParams_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(Response prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<Response, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(Response cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private Response result;
      
      private Response PrepareBuilder() {
        if (resultIsReadOnly) {
          Response original = result;
          result = new Response();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override Response MessageBeingBuilt {
        get { return PrepareBuilder(); }
      }
      
      public override Builder Clear() {
        result = DefaultInstance;
        resultIsReadOnly = true;
        return this;
      }
      
      public override Builder Clone() {
        if (resultIsReadOnly) {
          return new Builder(result);
        } else {
          return new Builder().MergeFrom(result);
        }
      }
      
      public override pbd::MessageDescriptor DescriptorForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.Response.Descriptor; }
      }
      
      public override Response DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.Response.DefaultInstance; }
      }
      
      public override Response BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is Response) {
          return MergeFrom((Response) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(Response other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.Response.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasType) {
          Type = other.Type;
        }
        if (other.HasRequestId) {
          RequestId = other.RequestId;
        }
        if (other.HasIsSuccessful) {
          IsSuccessful = other.IsSuccessful;
        }
        if (other.HasErrorCode) {
          ErrorCode = other.ErrorCode;
        }
        if (other.errorParams_.Count != 0) {
          result.errorParams_.Add(other.errorParams_);
        }
        if (other.HasInsertDocumentsResponse) {
          MergeInsertDocumentsResponse(other.InsertDocumentsResponse);
        }
        if (other.HasDeleteDocumentsResponse) {
          MergeDeleteDocumentsResponse(other.DeleteDocumentsResponse);
        }
        if (other.HasGetDocumentsResponse) {
          MergeGetDocumentsResponse(other.GetDocumentsResponse);
        }
        if (other.HasUpdateResponse) {
          MergeUpdateResponse(other.UpdateResponse);
        }
        if (other.HasWriteQueryResponse) {
          MergeWriteQueryResponse(other.WriteQueryResponse);
        }
        if (other.HasReadQueryResponse) {
          MergeReadQueryResponse(other.ReadQueryResponse);
        }
        if (other.HasCreateSessionResponse) {
          MergeCreateSessionResponse(other.CreateSessionResponse);
        }
        if (other.HasGetChunkResponse) {
          MergeGetChunkResponse(other.GetChunkResponse);
        }
        if (other.HasReplaceDocumentsResponse) {
          MergeReplaceDocumentsResponse(other.ReplaceDocumentsResponse);
        }
        if (other.HasAuthenticationResponse) {
          MergeAuthenticationResponse(other.AuthenticationResponse);
        }
        if (other.HasInitDatabaseResponse) {
          MergeInitDatabaseResponse(other.InitDatabaseResponse);
        }
        this.MergeUnknownFields(other.UnknownFields);
        return this;
      }
      
      public override Builder MergeFrom(pb::ICodedInputStream input) {
        return MergeFrom(input, pb::ExtensionRegistry.Empty);
      }
      
      public override Builder MergeFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
        PrepareBuilder();
        pb::UnknownFieldSet.Builder unknownFields = null;
        uint tag;
        string field_name;
        while (input.ReadTag(out tag, out field_name)) {
          if(tag == 0 && field_name != null) {
            int field_ordinal = global::System.Array.BinarySearch(_responseFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _responseFieldTags[field_ordinal];
            else {
              if (unknownFields == null) {
                unknownFields = pb::UnknownFieldSet.CreateBuilder(this.UnknownFields);
              }
              ParseUnknownField(input, unknownFields, extensionRegistry, tag, field_name);
              continue;
            }
          }
          switch (tag) {
            case 0: {
              throw pb::InvalidProtocolBufferException.InvalidTag();
            }
            default: {
              if (pb::WireFormat.IsEndGroupTag(tag)) {
                if (unknownFields != null) {
                  this.UnknownFields = unknownFields.Build();
                }
                return this;
              }
              if (unknownFields == null) {
                unknownFields = pb::UnknownFieldSet.CreateBuilder(this.UnknownFields);
              }
              ParseUnknownField(input, unknownFields, extensionRegistry, tag, field_name);
              break;
            }
            case 8: {
              object unknown;
              if(input.ReadEnum(ref result.type_, out unknown)) {
                result.hasType = true;
              } else if(unknown is int) {
                if (unknownFields == null) {
                  unknownFields = pb::UnknownFieldSet.CreateBuilder(this.UnknownFields);
                }
                unknownFields.MergeVarintField(1, (ulong)(int)unknown);
              }
              break;
            }
            case 16: {
              result.hasRequestId = input.ReadInt64(ref result.requestId_);
              break;
            }
            case 24: {
              result.hasIsSuccessful = input.ReadBool(ref result.isSuccessful_);
              break;
            }
            case 32: {
              result.hasErrorCode = input.ReadInt32(ref result.errorCode_);
              break;
            }
            case 42: {
              input.ReadStringArray(tag, field_name, result.errorParams_);
              break;
            }
            case 50: {
              global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse.CreateBuilder();
              if (result.hasInsertDocumentsResponse) {
                subBuilder.MergeFrom(InsertDocumentsResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              InsertDocumentsResponse = subBuilder.BuildPartial();
              break;
            }
            case 58: {
              global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.CreateBuilder();
              if (result.hasDeleteDocumentsResponse) {
                subBuilder.MergeFrom(DeleteDocumentsResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              DeleteDocumentsResponse = subBuilder.BuildPartial();
              break;
            }
            case 66: {
              global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse.CreateBuilder();
              if (result.hasGetDocumentsResponse) {
                subBuilder.MergeFrom(GetDocumentsResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              GetDocumentsResponse = subBuilder.BuildPartial();
              break;
            }
            case 74: {
              global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.CreateBuilder();
              if (result.hasUpdateResponse) {
                subBuilder.MergeFrom(UpdateResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              UpdateResponse = subBuilder.BuildPartial();
              break;
            }
            case 82: {
              global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.CreateBuilder();
              if (result.hasWriteQueryResponse) {
                subBuilder.MergeFrom(WriteQueryResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              WriteQueryResponse = subBuilder.BuildPartial();
              break;
            }
            case 90: {
              global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse.CreateBuilder();
              if (result.hasReadQueryResponse) {
                subBuilder.MergeFrom(ReadQueryResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              ReadQueryResponse = subBuilder.BuildPartial();
              break;
            }
            case 98: {
              global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.CreateBuilder();
              if (result.hasCreateSessionResponse) {
                subBuilder.MergeFrom(CreateSessionResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              CreateSessionResponse = subBuilder.BuildPartial();
              break;
            }
            case 106: {
              global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.CreateBuilder();
              if (result.hasGetChunkResponse) {
                subBuilder.MergeFrom(GetChunkResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              GetChunkResponse = subBuilder.BuildPartial();
              break;
            }
            case 114: {
              global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse.CreateBuilder();
              if (result.hasReplaceDocumentsResponse) {
                subBuilder.MergeFrom(ReplaceDocumentsResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              ReplaceDocumentsResponse = subBuilder.BuildPartial();
              break;
            }
            case 122: {
              global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.CreateBuilder();
              if (result.hasAuthenticationResponse) {
                subBuilder.MergeFrom(AuthenticationResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              AuthenticationResponse = subBuilder.BuildPartial();
              break;
            }
            case 130: {
              global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.CreateBuilder();
              if (result.hasInitDatabaseResponse) {
                subBuilder.MergeFrom(InitDatabaseResponse);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              InitDatabaseResponse = subBuilder.BuildPartial();
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasType {
       get { return result.hasType; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.Response.Types.Type Type {
        get { return result.Type; }
        set { SetType(value); }
      }
      public Builder SetType(global::Alachisoft.NosDB.Common.Protobuf.Response.Types.Type value) {
        PrepareBuilder();
        result.hasType = true;
        result.type_ = value;
        return this;
      }
      public Builder ClearType() {
        PrepareBuilder();
        result.hasType = false;
        result.type_ = global::Alachisoft.NosDB.Common.Protobuf.Response.Types.Type.INSERT_DOCUMENTS;
        return this;
      }
      
      public bool HasRequestId {
        get { return result.hasRequestId; }
      }
      public long RequestId {
        get { return result.RequestId; }
        set { SetRequestId(value); }
      }
      public Builder SetRequestId(long value) {
        PrepareBuilder();
        result.hasRequestId = true;
        result.requestId_ = value;
        return this;
      }
      public Builder ClearRequestId() {
        PrepareBuilder();
        result.hasRequestId = false;
        result.requestId_ = 0L;
        return this;
      }
      
      public bool HasIsSuccessful {
        get { return result.hasIsSuccessful; }
      }
      public bool IsSuccessful {
        get { return result.IsSuccessful; }
        set { SetIsSuccessful(value); }
      }
      public Builder SetIsSuccessful(bool value) {
        PrepareBuilder();
        result.hasIsSuccessful = true;
        result.isSuccessful_ = value;
        return this;
      }
      public Builder ClearIsSuccessful() {
        PrepareBuilder();
        result.hasIsSuccessful = false;
        result.isSuccessful_ = false;
        return this;
      }
      
      public bool HasErrorCode {
        get { return result.hasErrorCode; }
      }
      public int ErrorCode {
        get { return result.ErrorCode; }
        set { SetErrorCode(value); }
      }
      public Builder SetErrorCode(int value) {
        PrepareBuilder();
        result.hasErrorCode = true;
        result.errorCode_ = value;
        return this;
      }
      public Builder ClearErrorCode() {
        PrepareBuilder();
        result.hasErrorCode = false;
        result.errorCode_ = 0;
        return this;
      }
      
      public pbc::IPopsicleList<string> ErrorParamsList {
        get { return PrepareBuilder().errorParams_; }
      }
      public int ErrorParamsCount {
        get { return result.ErrorParamsCount; }
      }
      public string GetErrorParams(int index) {
        return result.GetErrorParams(index);
      }
      public Builder SetErrorParams(int index, string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.errorParams_[index] = value;
        return this;
      }
      public Builder AddErrorParams(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.errorParams_.Add(value);
        return this;
      }
      public Builder AddRangeErrorParams(scg::IEnumerable<string> values) {
        PrepareBuilder();
        result.errorParams_.Add(values);
        return this;
      }
      public Builder ClearErrorParams() {
        PrepareBuilder();
        result.errorParams_.Clear();
        return this;
      }
      
      public bool HasInsertDocumentsResponse {
       get { return result.hasInsertDocumentsResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse InsertDocumentsResponse {
        get { return result.InsertDocumentsResponse; }
        set { SetInsertDocumentsResponse(value); }
      }
      public Builder SetInsertDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasInsertDocumentsResponse = true;
        result.insertDocumentsResponse_ = value;
        return this;
      }
      public Builder SetInsertDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasInsertDocumentsResponse = true;
        result.insertDocumentsResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeInsertDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasInsertDocumentsResponse &&
            result.insertDocumentsResponse_ != global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse.DefaultInstance) {
            result.insertDocumentsResponse_ = global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsResponse.CreateBuilder(result.insertDocumentsResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.insertDocumentsResponse_ = value;
        }
        result.hasInsertDocumentsResponse = true;
        return this;
      }
      public Builder ClearInsertDocumentsResponse() {
        PrepareBuilder();
        result.hasInsertDocumentsResponse = false;
        result.insertDocumentsResponse_ = null;
        return this;
      }
      
      public bool HasDeleteDocumentsResponse {
       get { return result.hasDeleteDocumentsResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse DeleteDocumentsResponse {
        get { return result.DeleteDocumentsResponse; }
        set { SetDeleteDocumentsResponse(value); }
      }
      public Builder SetDeleteDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDeleteDocumentsResponse = true;
        result.deleteDocumentsResponse_ = value;
        return this;
      }
      public Builder SetDeleteDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasDeleteDocumentsResponse = true;
        result.deleteDocumentsResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeDeleteDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasDeleteDocumentsResponse &&
            result.deleteDocumentsResponse_ != global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.DefaultInstance) {
            result.deleteDocumentsResponse_ = global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.CreateBuilder(result.deleteDocumentsResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.deleteDocumentsResponse_ = value;
        }
        result.hasDeleteDocumentsResponse = true;
        return this;
      }
      public Builder ClearDeleteDocumentsResponse() {
        PrepareBuilder();
        result.hasDeleteDocumentsResponse = false;
        result.deleteDocumentsResponse_ = null;
        return this;
      }
      
      public bool HasGetDocumentsResponse {
       get { return result.hasGetDocumentsResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse GetDocumentsResponse {
        get { return result.GetDocumentsResponse; }
        set { SetGetDocumentsResponse(value); }
      }
      public Builder SetGetDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasGetDocumentsResponse = true;
        result.getDocumentsResponse_ = value;
        return this;
      }
      public Builder SetGetDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasGetDocumentsResponse = true;
        result.getDocumentsResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeGetDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasGetDocumentsResponse &&
            result.getDocumentsResponse_ != global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse.DefaultInstance) {
            result.getDocumentsResponse_ = global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsResponse.CreateBuilder(result.getDocumentsResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.getDocumentsResponse_ = value;
        }
        result.hasGetDocumentsResponse = true;
        return this;
      }
      public Builder ClearGetDocumentsResponse() {
        PrepareBuilder();
        result.hasGetDocumentsResponse = false;
        result.getDocumentsResponse_ = null;
        return this;
      }
      
      public bool HasUpdateResponse {
       get { return result.hasUpdateResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse UpdateResponse {
        get { return result.UpdateResponse; }
        set { SetUpdateResponse(value); }
      }
      public Builder SetUpdateResponse(global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasUpdateResponse = true;
        result.updateResponse_ = value;
        return this;
      }
      public Builder SetUpdateResponse(global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasUpdateResponse = true;
        result.updateResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeUpdateResponse(global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasUpdateResponse &&
            result.updateResponse_ != global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.DefaultInstance) {
            result.updateResponse_ = global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.CreateBuilder(result.updateResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.updateResponse_ = value;
        }
        result.hasUpdateResponse = true;
        return this;
      }
      public Builder ClearUpdateResponse() {
        PrepareBuilder();
        result.hasUpdateResponse = false;
        result.updateResponse_ = null;
        return this;
      }
      
      public bool HasWriteQueryResponse {
       get { return result.hasWriteQueryResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse WriteQueryResponse {
        get { return result.WriteQueryResponse; }
        set { SetWriteQueryResponse(value); }
      }
      public Builder SetWriteQueryResponse(global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasWriteQueryResponse = true;
        result.writeQueryResponse_ = value;
        return this;
      }
      public Builder SetWriteQueryResponse(global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasWriteQueryResponse = true;
        result.writeQueryResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeWriteQueryResponse(global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasWriteQueryResponse &&
            result.writeQueryResponse_ != global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.DefaultInstance) {
            result.writeQueryResponse_ = global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.CreateBuilder(result.writeQueryResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.writeQueryResponse_ = value;
        }
        result.hasWriteQueryResponse = true;
        return this;
      }
      public Builder ClearWriteQueryResponse() {
        PrepareBuilder();
        result.hasWriteQueryResponse = false;
        result.writeQueryResponse_ = null;
        return this;
      }
      
      public bool HasReadQueryResponse {
       get { return result.hasReadQueryResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse ReadQueryResponse {
        get { return result.ReadQueryResponse; }
        set { SetReadQueryResponse(value); }
      }
      public Builder SetReadQueryResponse(global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasReadQueryResponse = true;
        result.readQueryResponse_ = value;
        return this;
      }
      public Builder SetReadQueryResponse(global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasReadQueryResponse = true;
        result.readQueryResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeReadQueryResponse(global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasReadQueryResponse &&
            result.readQueryResponse_ != global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse.DefaultInstance) {
            result.readQueryResponse_ = global::Alachisoft.NosDB.Common.Protobuf.ReadQueryResponse.CreateBuilder(result.readQueryResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.readQueryResponse_ = value;
        }
        result.hasReadQueryResponse = true;
        return this;
      }
      public Builder ClearReadQueryResponse() {
        PrepareBuilder();
        result.hasReadQueryResponse = false;
        result.readQueryResponse_ = null;
        return this;
      }
      
      public bool HasCreateSessionResponse {
       get { return result.hasCreateSessionResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse CreateSessionResponse {
        get { return result.CreateSessionResponse; }
        set { SetCreateSessionResponse(value); }
      }
      public Builder SetCreateSessionResponse(global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCreateSessionResponse = true;
        result.createSessionResponse_ = value;
        return this;
      }
      public Builder SetCreateSessionResponse(global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasCreateSessionResponse = true;
        result.createSessionResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeCreateSessionResponse(global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasCreateSessionResponse &&
            result.createSessionResponse_ != global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.DefaultInstance) {
            result.createSessionResponse_ = global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.CreateBuilder(result.createSessionResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.createSessionResponse_ = value;
        }
        result.hasCreateSessionResponse = true;
        return this;
      }
      public Builder ClearCreateSessionResponse() {
        PrepareBuilder();
        result.hasCreateSessionResponse = false;
        result.createSessionResponse_ = null;
        return this;
      }
      
      public bool HasGetChunkResponse {
       get { return result.hasGetChunkResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse GetChunkResponse {
        get { return result.GetChunkResponse; }
        set { SetGetChunkResponse(value); }
      }
      public Builder SetGetChunkResponse(global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasGetChunkResponse = true;
        result.getChunkResponse_ = value;
        return this;
      }
      public Builder SetGetChunkResponse(global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasGetChunkResponse = true;
        result.getChunkResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeGetChunkResponse(global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasGetChunkResponse &&
            result.getChunkResponse_ != global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.DefaultInstance) {
            result.getChunkResponse_ = global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.CreateBuilder(result.getChunkResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.getChunkResponse_ = value;
        }
        result.hasGetChunkResponse = true;
        return this;
      }
      public Builder ClearGetChunkResponse() {
        PrepareBuilder();
        result.hasGetChunkResponse = false;
        result.getChunkResponse_ = null;
        return this;
      }
      
      public bool HasReplaceDocumentsResponse {
       get { return result.hasReplaceDocumentsResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse ReplaceDocumentsResponse {
        get { return result.ReplaceDocumentsResponse; }
        set { SetReplaceDocumentsResponse(value); }
      }
      public Builder SetReplaceDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasReplaceDocumentsResponse = true;
        result.replaceDocumentsResponse_ = value;
        return this;
      }
      public Builder SetReplaceDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasReplaceDocumentsResponse = true;
        result.replaceDocumentsResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeReplaceDocumentsResponse(global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasReplaceDocumentsResponse &&
            result.replaceDocumentsResponse_ != global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse.DefaultInstance) {
            result.replaceDocumentsResponse_ = global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsResponse.CreateBuilder(result.replaceDocumentsResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.replaceDocumentsResponse_ = value;
        }
        result.hasReplaceDocumentsResponse = true;
        return this;
      }
      public Builder ClearReplaceDocumentsResponse() {
        PrepareBuilder();
        result.hasReplaceDocumentsResponse = false;
        result.replaceDocumentsResponse_ = null;
        return this;
      }
      
      public bool HasAuthenticationResponse {
       get { return result.hasAuthenticationResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse AuthenticationResponse {
        get { return result.AuthenticationResponse; }
        set { SetAuthenticationResponse(value); }
      }
      public Builder SetAuthenticationResponse(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasAuthenticationResponse = true;
        result.authenticationResponse_ = value;
        return this;
      }
      public Builder SetAuthenticationResponse(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasAuthenticationResponse = true;
        result.authenticationResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeAuthenticationResponse(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasAuthenticationResponse &&
            result.authenticationResponse_ != global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.DefaultInstance) {
            result.authenticationResponse_ = global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.CreateBuilder(result.authenticationResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.authenticationResponse_ = value;
        }
        result.hasAuthenticationResponse = true;
        return this;
      }
      public Builder ClearAuthenticationResponse() {
        PrepareBuilder();
        result.hasAuthenticationResponse = false;
        result.authenticationResponse_ = null;
        return this;
      }
      
      public bool HasInitDatabaseResponse {
       get { return result.hasInitDatabaseResponse; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse InitDatabaseResponse {
        get { return result.InitDatabaseResponse; }
        set { SetInitDatabaseResponse(value); }
      }
      public Builder SetInitDatabaseResponse(global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasInitDatabaseResponse = true;
        result.initDatabaseResponse_ = value;
        return this;
      }
      public Builder SetInitDatabaseResponse(global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasInitDatabaseResponse = true;
        result.initDatabaseResponse_ = builderForValue.Build();
        return this;
      }
      public Builder MergeInitDatabaseResponse(global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasInitDatabaseResponse &&
            result.initDatabaseResponse_ != global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.DefaultInstance) {
            result.initDatabaseResponse_ = global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.CreateBuilder(result.initDatabaseResponse_).MergeFrom(value).BuildPartial();
        } else {
          result.initDatabaseResponse_ = value;
        }
        result.hasInitDatabaseResponse = true;
        return this;
      }
      public Builder ClearInitDatabaseResponse() {
        PrepareBuilder();
        result.hasInitDatabaseResponse = false;
        result.initDatabaseResponse_ = null;
        return this;
      }
    }
    static Response() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.Response.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
