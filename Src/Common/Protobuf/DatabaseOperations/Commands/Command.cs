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
    public static partial class Command {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_Command__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.Command, global::Alachisoft.NosDB.Common.Protobuf.Command.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_Command__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static Command() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "Cg1Db21tYW5kLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90", 
              "b2J1ZhocSW5zZXJ0RG9jdW1lbnRzQ29tbWFuZC5wcm90bxocRGVsZXRlRG9j", 
              "dW1lbnRzQ29tbWFuZC5wcm90bxoZR2V0RG9jdW1lbnRzQ29tbWFuZC5wcm90", 
              "bxoTVXBkYXRlQ29tbWFuZC5wcm90bxoWUmVhZFF1ZXJ5Q29tbWFuZC5wcm90", 
              "bxoXV3JpdGVRdWVyeUNvbW1hbmQucHJvdG8aHUNyZWF0ZUNvbGxlY3Rpb25D", 
              "b21tYW5kLnByb3RvGhtEcm9wQ29sbGVjdGlvbkNvbW1hbmQucHJvdG8aGkNy", 
              "ZWF0ZVNlc3Npb25Db21tYW5kLnByb3RvGhhEcm9wU2Vzc2lvbkNvbW1hbmQu", 
              "cHJvdG8aGENyZWF0ZUluZGV4Q29tbWFuZC5wcm90bxoWRHJvcEluZGV4Q29t", 
              "bWFuZC5wcm90bxoVR2V0Q2h1bmtDb21tYW5kLnByb3RvGhpEaXNwb3NlUmVh", 
              "ZGVyQ29tbWFuZC5wcm90bxodUmVwbGFjZURvY3VtZW50c0NvbW1hbmQucHJv", 
              "dG8aG0F1dGhlbnRpY2F0aW9uQ29tbWFuZC5wcm90bxoPU2Vzc2lvbklkLnBy", 
              "b3RvGhlJbml0RGF0YWJhc2VDb21tYW5kLnByb3RvIsgPCgdDb21tYW5kEjwK", 
              "BHR5cGUYASABKA4yLi5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90b2J1", 
              "Zi5Db21tYW5kLlR5cGUSEQoJcmVxdWVzdElkGAIgASgDEhQKDGRhdGFiYXNl", 
              "TmFtZRgDIAEoCRIWCg5jb2xsZWN0aW9uTmFtZRgEIAEoCRIMCgRmbGFnGAUg", 
              "ASgFEj4KCXNlc3Npb25JZBgGIAEoCzIrLkFsYWNoaXNvZnQuTm9zREIuQ29t", 
              "bW9uLlByb3RvYnVmLlNlc3Npb25JZBISCgpub1Jlc3BvbnNlGAcgASgIElgK", 
              "Fmluc2VydERvY3VtZW50c0NvbW1hbmQYCCABKAsyOC5BbGFjaGlzb2Z0Lk5v", 
              "c0RCLkNvbW1vbi5Qcm90b2J1Zi5JbnNlcnREb2N1bWVudHNDb21tYW5kElgK", 
              "FmRlbGV0ZURvY3VtZW50c0NvbW1hbmQYCSABKAsyOC5BbGFjaGlzb2Z0Lk5v", 
              "c0RCLkNvbW1vbi5Qcm90b2J1Zi5EZWxldGVEb2N1bWVudHNDb21tYW5kElIK", 
              "E2dldERvY3VtZW50c0NvbW1hbmQYCiABKAsyNS5BbGFjaGlzb2Z0Lk5vc0RC", 
              "LkNvbW1vbi5Qcm90b2J1Zi5HZXREb2N1bWVudHNDb21tYW5kEkYKDXVwZGF0", 
              "ZUNvbW1hbmQYCyABKAsyLy5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90", 
              "b2J1Zi5VcGRhdGVDb21tYW5kEkwKEHJlYWRRdWVyeUNvbW1hbmQYDCABKAsy", 
              "Mi5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90b2J1Zi5SZWFkUXVlcnlD", 
              "b21tYW5kEk4KEXdyaXRlUXVlcnlDb21tYW5kGA0gASgLMjMuQWxhY2hpc29m", 
              "dC5Ob3NEQi5Db21tb24uUHJvdG9idWYuV3JpdGVRdWVyeUNvbW1hbmQSWgoX", 
              "Y3JlYXRlQ29sbGVjdGlvbkNvbW1hbmQYDiABKAsyOS5BbGFjaGlzb2Z0Lk5v", 
              "c0RCLkNvbW1vbi5Qcm90b2J1Zi5DcmVhdGVDb2xsZWN0aW9uQ29tbWFuZBJW", 
              "ChVkcm9wQ29sbGVjdGlvbkNvbW1hbmQYDyABKAsyNy5BbGFjaGlzb2Z0Lk5v", 
              "c0RCLkNvbW1vbi5Qcm90b2J1Zi5Ecm9wQ29sbGVjdGlvbkNvbW1hbmQSVAoU", 
              "Y3JlYXRlU2Vzc2lvbkNvbW1hbmQYECABKAsyNi5BbGFjaGlzb2Z0Lk5vc0RC", 
              "LkNvbW1vbi5Qcm90b2J1Zi5DcmVhdGVTZXNzaW9uQ29tbWFuZBJQChJkcm9w", 
              "U2Vzc2lvbkNvbW1hbmQYESABKAsyNC5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1v", 
              "bi5Qcm90b2J1Zi5Ecm9wU2Vzc2lvbkNvbW1hbmQSUAoSY3JlYXRlSW5kZXhD", 
              "b21tYW5kGBIgASgLMjQuQWxhY2hpc29mdC5Ob3NEQi5Db21tb24uUHJvdG9i", 
              "dWYuQ3JlYXRlSW5kZXhDb21tYW5kEkwKEGRyb3BJbmRleENvbW1hbmQYEyAB", 
              "KAsyMi5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90b2J1Zi5Ecm9wSW5k", 
              "ZXhDb21tYW5kEkoKD2dldENodW5rQ29tbWFuZBgUIAEoCzIxLkFsYWNoaXNv", 
              "ZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmLkdldENodW5rQ29tbWFuZBJUChRk", 
              "aXNwb3NlUmVhZGVyQ29tbWFuZBgVIAEoCzI2LkFsYWNoaXNvZnQuTm9zREIu", 
              "Q29tbW9uLlByb3RvYnVmLkRpc3Bvc2VSZWFkZXJDb21tYW5kEloKF3JlcGxh", 
              "Y2VEb2N1bWVudHNDb21tYW5kGBYgASgLMjkuQWxhY2hpc29mdC5Ob3NEQi5D", 
              "b21tb24uUHJvdG9idWYuUmVwbGFjZURvY3VtZW50c0NvbW1hbmQSVgoVYXV0", 
              "aGVudGljYXRpb25Db21tYW5kGBcgASgLMjcuQWxhY2hpc29mdC5Ob3NEQi5D", 
              "b21tb24uUHJvdG9idWYuQXV0aGVudGljYXRpb25Db21tYW5kElIKE2luaXRE", 
              "YXRhYmFzZUNvbW1hbmQYGCABKAsyNS5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1v", 
              "bi5Qcm90b2J1Zi5Jbml0RGF0YWJhc2VDb21tYW5kIscCCgRUeXBlEhQKEElO", 
              "U0VSVF9ET0NVTUVOVFMQARIUChBERUxFVEVfRE9DVU1FTlRTEAISEQoNR0VU", 
              "X0RPQ1VNRU5UUxADEgoKBlVQREFURRAEEg4KClJFQURfUVVFUlkQBRIPCgtX", 
              "UklURV9RVUVSWRAGEhUKEUNSRUFURV9DT0xMRUNUSU9OEAcSEwoPRFJPUF9D", 
              "T0xMRUNUSU9OEAgSEgoOQ1JFQVRFX1NFU1NJT04QCRIQCgxEUk9QX1NFU1NJ", 
              "T04QChIQCgxDUkVBVEVfSU5ERVgQCxIOCgpEUk9QX0lOREVYEAwSDQoJR0VU", 
              "X0NIVU5LEA0SEgoORElTUE9TRV9SRUFERVIQDhIVChFSRVBMQUNFX0RPQ1VN", 
              "RU5UUxAPEhIKDkFVVEhFTlRJQ0FUSU9OEBASEQoNSU5JVF9EQVRBQkFTRRAR", 
              "QjcKJGNvbS5hbGFjaGlzb2Z0Lm5vc2RiLmNvbW1vbi5wcm90b2J1ZkIPQ29t", 
            "bWFuZFByb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_Command__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_Command__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.Command, global::Alachisoft.NosDB.Common.Protobuf.Command.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_Command__Descriptor,
                  new string[] { "Type", "RequestId", "DatabaseName", "CollectionName", "Flag", "SessionId", "NoResponse", "InsertDocumentsCommand", "DeleteDocumentsCommand", "GetDocumentsCommand", "UpdateCommand", "ReadQueryCommand", "WriteQueryCommand", "CreateCollectionCommand", "DropCollectionCommand", "CreateSessionCommand", "DropSessionCommand", "CreateIndexCommand", "DropIndexCommand", "GetChunkCommand", "DisposeReaderCommand", "ReplaceDocumentsCommand", "AuthenticationCommand", "InitDatabaseCommand", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.InsertDocumentsCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.DeleteDocumentsCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.GetDocumentsCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.UpdateCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.ReadQueryCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.WriteQueryCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateCollectionCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.DropCollectionCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.DropSessionCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateIndexCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.DropIndexCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.GetChunkCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.DisposeReaderCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.ReplaceDocumentsCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationCommand.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.SessionId.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.InitDatabaseCommand.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class Command : pb::GeneratedMessage<Command, Command.Builder> {
    private Command() { }
    private static readonly Command defaultInstance = new Command().MakeReadOnly();
    private static readonly string[] _commandFieldNames = new string[] { "authenticationCommand", "collectionName", "createCollectionCommand", "createIndexCommand", "createSessionCommand", "databaseName", "deleteDocumentsCommand", "disposeReaderCommand", "dropCollectionCommand", "dropIndexCommand", "dropSessionCommand", "flag", "getChunkCommand", "getDocumentsCommand", "initDatabaseCommand", "insertDocumentsCommand", "noResponse", "readQueryCommand", "replaceDocumentsCommand", "requestId", "sessionId", "type", "updateCommand", "writeQueryCommand" };
    private static readonly uint[] _commandFieldTags = new uint[] { 186, 34, 114, 146, 130, 26, 74, 170, 122, 154, 138, 40, 162, 82, 194, 66, 56, 98, 178, 16, 50, 8, 90, 106 };
    public static Command DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override Command DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override Command ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.Command.internal__static_Alachisoft_NosDB_Common_Protobuf_Command__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<Command, Command.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.Command.internal__static_Alachisoft_NosDB_Common_Protobuf_Command__FieldAccessorTable; }
    }
    
    #region Nested types
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public static partial class Types {
      public enum Type {
        INSERT_DOCUMENTS = 1,
        DELETE_DOCUMENTS = 2,
        GET_DOCUMENTS = 3,
        UPDATE = 4,
        READ_QUERY = 5,
        WRITE_QUERY = 6,
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
    private global::Alachisoft.NosDB.Common.Protobuf.Command.Types.Type type_ = global::Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.INSERT_DOCUMENTS;
    public bool HasType {
      get { return hasType; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.Command.Types.Type Type {
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
    
    public const int DatabaseNameFieldNumber = 3;
    private bool hasDatabaseName;
    private string databaseName_ = "";
    public bool HasDatabaseName {
      get { return hasDatabaseName; }
    }
    public string DatabaseName {
      get { return databaseName_; }
    }
    
    public const int CollectionNameFieldNumber = 4;
    private bool hasCollectionName;
    private string collectionName_ = "";
    public bool HasCollectionName {
      get { return hasCollectionName; }
    }
    public string CollectionName {
      get { return collectionName_; }
    }
    
    public const int FlagFieldNumber = 5;
    private bool hasFlag;
    private int flag_;
    public bool HasFlag {
      get { return hasFlag; }
    }
    public int Flag {
      get { return flag_; }
    }
    
    public const int SessionIdFieldNumber = 6;
    private bool hasSessionId;
    private global::Alachisoft.NosDB.Common.Protobuf.SessionId sessionId_;
    public bool HasSessionId {
      get { return hasSessionId; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.SessionId SessionId {
      get { return sessionId_ ?? global::Alachisoft.NosDB.Common.Protobuf.SessionId.DefaultInstance; }
    }
    
    public const int NoResponseFieldNumber = 7;
    private bool hasNoResponse;
    private bool noResponse_;
    public bool HasNoResponse {
      get { return hasNoResponse; }
    }
    public bool NoResponse {
      get { return noResponse_; }
    }
    
    public const int InsertDocumentsCommandFieldNumber = 8;
    private bool hasInsertDocumentsCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand insertDocumentsCommand_;
    public bool HasInsertDocumentsCommand {
      get { return hasInsertDocumentsCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand InsertDocumentsCommand {
      get { return insertDocumentsCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand.DefaultInstance; }
    }
    
    public const int DeleteDocumentsCommandFieldNumber = 9;
    private bool hasDeleteDocumentsCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand deleteDocumentsCommand_;
    public bool HasDeleteDocumentsCommand {
      get { return hasDeleteDocumentsCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand DeleteDocumentsCommand {
      get { return deleteDocumentsCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand.DefaultInstance; }
    }
    
    public const int GetDocumentsCommandFieldNumber = 10;
    private bool hasGetDocumentsCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand getDocumentsCommand_;
    public bool HasGetDocumentsCommand {
      get { return hasGetDocumentsCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand GetDocumentsCommand {
      get { return getDocumentsCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.DefaultInstance; }
    }
    
    public const int UpdateCommandFieldNumber = 11;
    private bool hasUpdateCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand updateCommand_;
    public bool HasUpdateCommand {
      get { return hasUpdateCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand UpdateCommand {
      get { return updateCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.DefaultInstance; }
    }
    
    public const int ReadQueryCommandFieldNumber = 12;
    private bool hasReadQueryCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand readQueryCommand_;
    public bool HasReadQueryCommand {
      get { return hasReadQueryCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand ReadQueryCommand {
      get { return readQueryCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.DefaultInstance; }
    }
    
    public const int WriteQueryCommandFieldNumber = 13;
    private bool hasWriteQueryCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand writeQueryCommand_;
    public bool HasWriteQueryCommand {
      get { return hasWriteQueryCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand WriteQueryCommand {
      get { return writeQueryCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand.DefaultInstance; }
    }
    
    public const int CreateCollectionCommandFieldNumber = 14;
    private bool hasCreateCollectionCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand createCollectionCommand_;
    public bool HasCreateCollectionCommand {
      get { return hasCreateCollectionCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand CreateCollectionCommand {
      get { return createCollectionCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.DefaultInstance; }
    }
    
    public const int DropCollectionCommandFieldNumber = 15;
    private bool hasDropCollectionCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand dropCollectionCommand_;
    public bool HasDropCollectionCommand {
      get { return hasDropCollectionCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand DropCollectionCommand {
      get { return dropCollectionCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand.DefaultInstance; }
    }
    
    public const int CreateSessionCommandFieldNumber = 16;
    private bool hasCreateSessionCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand createSessionCommand_;
    public bool HasCreateSessionCommand {
      get { return hasCreateSessionCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand CreateSessionCommand {
      get { return createSessionCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.DefaultInstance; }
    }
    
    public const int DropSessionCommandFieldNumber = 17;
    private bool hasDropSessionCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand dropSessionCommand_;
    public bool HasDropSessionCommand {
      get { return hasDropSessionCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand DropSessionCommand {
      get { return dropSessionCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand.DefaultInstance; }
    }
    
    public const int CreateIndexCommandFieldNumber = 18;
    private bool hasCreateIndexCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand createIndexCommand_;
    public bool HasCreateIndexCommand {
      get { return hasCreateIndexCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand CreateIndexCommand {
      get { return createIndexCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.DefaultInstance; }
    }
    
    public const int DropIndexCommandFieldNumber = 19;
    private bool hasDropIndexCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand dropIndexCommand_;
    public bool HasDropIndexCommand {
      get { return hasDropIndexCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand DropIndexCommand {
      get { return dropIndexCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.DefaultInstance; }
    }
    
    public const int GetChunkCommandFieldNumber = 20;
    private bool hasGetChunkCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand getChunkCommand_;
    public bool HasGetChunkCommand {
      get { return hasGetChunkCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand GetChunkCommand {
      get { return getChunkCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.DefaultInstance; }
    }
    
    public const int DisposeReaderCommandFieldNumber = 21;
    private bool hasDisposeReaderCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand disposeReaderCommand_;
    public bool HasDisposeReaderCommand {
      get { return hasDisposeReaderCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand DisposeReaderCommand {
      get { return disposeReaderCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.DefaultInstance; }
    }
    
    public const int ReplaceDocumentsCommandFieldNumber = 22;
    private bool hasReplaceDocumentsCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand replaceDocumentsCommand_;
    public bool HasReplaceDocumentsCommand {
      get { return hasReplaceDocumentsCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand ReplaceDocumentsCommand {
      get { return replaceDocumentsCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.DefaultInstance; }
    }
    
    public const int AuthenticationCommandFieldNumber = 23;
    private bool hasAuthenticationCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand authenticationCommand_;
    public bool HasAuthenticationCommand {
      get { return hasAuthenticationCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand AuthenticationCommand {
      get { return authenticationCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.DefaultInstance; }
    }
    
    public const int InitDatabaseCommandFieldNumber = 24;
    private bool hasInitDatabaseCommand;
    private global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand initDatabaseCommand_;
    public bool HasInitDatabaseCommand {
      get { return hasInitDatabaseCommand; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand InitDatabaseCommand {
      get { return initDatabaseCommand_ ?? global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _commandFieldNames;
      if (hasType) {
        output.WriteEnum(1, field_names[21], (int) Type, Type);
      }
      if (hasRequestId) {
        output.WriteInt64(2, field_names[19], RequestId);
      }
      if (hasDatabaseName) {
        output.WriteString(3, field_names[5], DatabaseName);
      }
      if (hasCollectionName) {
        output.WriteString(4, field_names[1], CollectionName);
      }
      if (hasFlag) {
        output.WriteInt32(5, field_names[11], Flag);
      }
      if (hasSessionId) {
        output.WriteMessage(6, field_names[20], SessionId);
      }
      if (hasNoResponse) {
        output.WriteBool(7, field_names[16], NoResponse);
      }
      if (hasInsertDocumentsCommand) {
        output.WriteMessage(8, field_names[15], InsertDocumentsCommand);
      }
      if (hasDeleteDocumentsCommand) {
        output.WriteMessage(9, field_names[6], DeleteDocumentsCommand);
      }
      if (hasGetDocumentsCommand) {
        output.WriteMessage(10, field_names[13], GetDocumentsCommand);
      }
      if (hasUpdateCommand) {
        output.WriteMessage(11, field_names[22], UpdateCommand);
      }
      if (hasReadQueryCommand) {
        output.WriteMessage(12, field_names[17], ReadQueryCommand);
      }
      if (hasWriteQueryCommand) {
        output.WriteMessage(13, field_names[23], WriteQueryCommand);
      }
      if (hasCreateCollectionCommand) {
        output.WriteMessage(14, field_names[2], CreateCollectionCommand);
      }
      if (hasDropCollectionCommand) {
        output.WriteMessage(15, field_names[8], DropCollectionCommand);
      }
      if (hasCreateSessionCommand) {
        output.WriteMessage(16, field_names[4], CreateSessionCommand);
      }
      if (hasDropSessionCommand) {
        output.WriteMessage(17, field_names[10], DropSessionCommand);
      }
      if (hasCreateIndexCommand) {
        output.WriteMessage(18, field_names[3], CreateIndexCommand);
      }
      if (hasDropIndexCommand) {
        output.WriteMessage(19, field_names[9], DropIndexCommand);
      }
      if (hasGetChunkCommand) {
        output.WriteMessage(20, field_names[12], GetChunkCommand);
      }
      if (hasDisposeReaderCommand) {
        output.WriteMessage(21, field_names[7], DisposeReaderCommand);
      }
      if (hasReplaceDocumentsCommand) {
        output.WriteMessage(22, field_names[18], ReplaceDocumentsCommand);
      }
      if (hasAuthenticationCommand) {
        output.WriteMessage(23, field_names[0], AuthenticationCommand);
      }
      if (hasInitDatabaseCommand) {
        output.WriteMessage(24, field_names[14], InitDatabaseCommand);
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
      if (hasDatabaseName) {
        size += pb::CodedOutputStream.ComputeStringSize(3, DatabaseName);
      }
      if (hasCollectionName) {
        size += pb::CodedOutputStream.ComputeStringSize(4, CollectionName);
      }
      if (hasFlag) {
        size += pb::CodedOutputStream.ComputeInt32Size(5, Flag);
      }
      if (hasSessionId) {
        size += pb::CodedOutputStream.ComputeMessageSize(6, SessionId);
      }
      if (hasNoResponse) {
        size += pb::CodedOutputStream.ComputeBoolSize(7, NoResponse);
      }
      if (hasInsertDocumentsCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(8, InsertDocumentsCommand);
      }
      if (hasDeleteDocumentsCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(9, DeleteDocumentsCommand);
      }
      if (hasGetDocumentsCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(10, GetDocumentsCommand);
      }
      if (hasUpdateCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(11, UpdateCommand);
      }
      if (hasReadQueryCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(12, ReadQueryCommand);
      }
      if (hasWriteQueryCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(13, WriteQueryCommand);
      }
      if (hasCreateCollectionCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(14, CreateCollectionCommand);
      }
      if (hasDropCollectionCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(15, DropCollectionCommand);
      }
      if (hasCreateSessionCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(16, CreateSessionCommand);
      }
      if (hasDropSessionCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(17, DropSessionCommand);
      }
      if (hasCreateIndexCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(18, CreateIndexCommand);
      }
      if (hasDropIndexCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(19, DropIndexCommand);
      }
      if (hasGetChunkCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(20, GetChunkCommand);
      }
      if (hasDisposeReaderCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(21, DisposeReaderCommand);
      }
      if (hasReplaceDocumentsCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(22, ReplaceDocumentsCommand);
      }
      if (hasAuthenticationCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(23, AuthenticationCommand);
      }
      if (hasInitDatabaseCommand) {
        size += pb::CodedOutputStream.ComputeMessageSize(24, InitDatabaseCommand);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static Command ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Command ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Command ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Command ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Command ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Command ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static Command ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static Command ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static Command ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Command ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private Command MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(Command prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<Command, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(Command cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private Command result;
      
      private Command PrepareBuilder() {
        if (resultIsReadOnly) {
          Command original = result;
          result = new Command();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override Command MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.Command.Descriptor; }
      }
      
      public override Command DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.Command.DefaultInstance; }
      }
      
      public override Command BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is Command) {
          return MergeFrom((Command) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(Command other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.Command.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasType) {
          Type = other.Type;
        }
        if (other.HasRequestId) {
          RequestId = other.RequestId;
        }
        if (other.HasDatabaseName) {
          DatabaseName = other.DatabaseName;
        }
        if (other.HasCollectionName) {
          CollectionName = other.CollectionName;
        }
        if (other.HasFlag) {
          Flag = other.Flag;
        }
        if (other.HasSessionId) {
          MergeSessionId(other.SessionId);
        }
        if (other.HasNoResponse) {
          NoResponse = other.NoResponse;
        }
        if (other.HasInsertDocumentsCommand) {
          MergeInsertDocumentsCommand(other.InsertDocumentsCommand);
        }
        if (other.HasDeleteDocumentsCommand) {
          MergeDeleteDocumentsCommand(other.DeleteDocumentsCommand);
        }
        if (other.HasGetDocumentsCommand) {
          MergeGetDocumentsCommand(other.GetDocumentsCommand);
        }
        if (other.HasUpdateCommand) {
          MergeUpdateCommand(other.UpdateCommand);
        }
        if (other.HasReadQueryCommand) {
          MergeReadQueryCommand(other.ReadQueryCommand);
        }
        if (other.HasWriteQueryCommand) {
          MergeWriteQueryCommand(other.WriteQueryCommand);
        }
        if (other.HasCreateCollectionCommand) {
          MergeCreateCollectionCommand(other.CreateCollectionCommand);
        }
        if (other.HasDropCollectionCommand) {
          MergeDropCollectionCommand(other.DropCollectionCommand);
        }
        if (other.HasCreateSessionCommand) {
          MergeCreateSessionCommand(other.CreateSessionCommand);
        }
        if (other.HasDropSessionCommand) {
          MergeDropSessionCommand(other.DropSessionCommand);
        }
        if (other.HasCreateIndexCommand) {
          MergeCreateIndexCommand(other.CreateIndexCommand);
        }
        if (other.HasDropIndexCommand) {
          MergeDropIndexCommand(other.DropIndexCommand);
        }
        if (other.HasGetChunkCommand) {
          MergeGetChunkCommand(other.GetChunkCommand);
        }
        if (other.HasDisposeReaderCommand) {
          MergeDisposeReaderCommand(other.DisposeReaderCommand);
        }
        if (other.HasReplaceDocumentsCommand) {
          MergeReplaceDocumentsCommand(other.ReplaceDocumentsCommand);
        }
        if (other.HasAuthenticationCommand) {
          MergeAuthenticationCommand(other.AuthenticationCommand);
        }
        if (other.HasInitDatabaseCommand) {
          MergeInitDatabaseCommand(other.InitDatabaseCommand);
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
            int field_ordinal = global::System.Array.BinarySearch(_commandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _commandFieldTags[field_ordinal];
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
            case 26: {
              result.hasDatabaseName = input.ReadString(ref result.databaseName_);
              break;
            }
            case 34: {
              result.hasCollectionName = input.ReadString(ref result.collectionName_);
              break;
            }
            case 40: {
              result.hasFlag = input.ReadInt32(ref result.flag_);
              break;
            }
            case 50: {
              global::Alachisoft.NosDB.Common.Protobuf.SessionId.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.SessionId.CreateBuilder();
              if (result.hasSessionId) {
                subBuilder.MergeFrom(SessionId);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              SessionId = subBuilder.BuildPartial();
              break;
            }
            case 56: {
              result.hasNoResponse = input.ReadBool(ref result.noResponse_);
              break;
            }
            case 66: {
              global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand.CreateBuilder();
              if (result.hasInsertDocumentsCommand) {
                subBuilder.MergeFrom(InsertDocumentsCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              InsertDocumentsCommand = subBuilder.BuildPartial();
              break;
            }
            case 74: {
              global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand.CreateBuilder();
              if (result.hasDeleteDocumentsCommand) {
                subBuilder.MergeFrom(DeleteDocumentsCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              DeleteDocumentsCommand = subBuilder.BuildPartial();
              break;
            }
            case 82: {
              global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.CreateBuilder();
              if (result.hasGetDocumentsCommand) {
                subBuilder.MergeFrom(GetDocumentsCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              GetDocumentsCommand = subBuilder.BuildPartial();
              break;
            }
            case 90: {
              global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.CreateBuilder();
              if (result.hasUpdateCommand) {
                subBuilder.MergeFrom(UpdateCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              UpdateCommand = subBuilder.BuildPartial();
              break;
            }
            case 98: {
              global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.CreateBuilder();
              if (result.hasReadQueryCommand) {
                subBuilder.MergeFrom(ReadQueryCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              ReadQueryCommand = subBuilder.BuildPartial();
              break;
            }
            case 106: {
              global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand.CreateBuilder();
              if (result.hasWriteQueryCommand) {
                subBuilder.MergeFrom(WriteQueryCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              WriteQueryCommand = subBuilder.BuildPartial();
              break;
            }
            case 114: {
              global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.CreateBuilder();
              if (result.hasCreateCollectionCommand) {
                subBuilder.MergeFrom(CreateCollectionCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              CreateCollectionCommand = subBuilder.BuildPartial();
              break;
            }
            case 122: {
              global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand.CreateBuilder();
              if (result.hasDropCollectionCommand) {
                subBuilder.MergeFrom(DropCollectionCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              DropCollectionCommand = subBuilder.BuildPartial();
              break;
            }
            case 130: {
              global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.CreateBuilder();
              if (result.hasCreateSessionCommand) {
                subBuilder.MergeFrom(CreateSessionCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              CreateSessionCommand = subBuilder.BuildPartial();
              break;
            }
            case 138: {
              global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand.CreateBuilder();
              if (result.hasDropSessionCommand) {
                subBuilder.MergeFrom(DropSessionCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              DropSessionCommand = subBuilder.BuildPartial();
              break;
            }
            case 146: {
              global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.CreateBuilder();
              if (result.hasCreateIndexCommand) {
                subBuilder.MergeFrom(CreateIndexCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              CreateIndexCommand = subBuilder.BuildPartial();
              break;
            }
            case 154: {
              global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.CreateBuilder();
              if (result.hasDropIndexCommand) {
                subBuilder.MergeFrom(DropIndexCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              DropIndexCommand = subBuilder.BuildPartial();
              break;
            }
            case 162: {
              global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.CreateBuilder();
              if (result.hasGetChunkCommand) {
                subBuilder.MergeFrom(GetChunkCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              GetChunkCommand = subBuilder.BuildPartial();
              break;
            }
            case 170: {
              global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.CreateBuilder();
              if (result.hasDisposeReaderCommand) {
                subBuilder.MergeFrom(DisposeReaderCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              DisposeReaderCommand = subBuilder.BuildPartial();
              break;
            }
            case 178: {
              global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.CreateBuilder();
              if (result.hasReplaceDocumentsCommand) {
                subBuilder.MergeFrom(ReplaceDocumentsCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              ReplaceDocumentsCommand = subBuilder.BuildPartial();
              break;
            }
            case 186: {
              global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.CreateBuilder();
              if (result.hasAuthenticationCommand) {
                subBuilder.MergeFrom(AuthenticationCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              AuthenticationCommand = subBuilder.BuildPartial();
              break;
            }
            case 194: {
              global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand.CreateBuilder();
              if (result.hasInitDatabaseCommand) {
                subBuilder.MergeFrom(InitDatabaseCommand);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              InitDatabaseCommand = subBuilder.BuildPartial();
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
      public global::Alachisoft.NosDB.Common.Protobuf.Command.Types.Type Type {
        get { return result.Type; }
        set { SetType(value); }
      }
      public Builder SetType(global::Alachisoft.NosDB.Common.Protobuf.Command.Types.Type value) {
        PrepareBuilder();
        result.hasType = true;
        result.type_ = value;
        return this;
      }
      public Builder ClearType() {
        PrepareBuilder();
        result.hasType = false;
        result.type_ = global::Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.INSERT_DOCUMENTS;
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
      
      public bool HasDatabaseName {
        get { return result.hasDatabaseName; }
      }
      public string DatabaseName {
        get { return result.DatabaseName; }
        set { SetDatabaseName(value); }
      }
      public Builder SetDatabaseName(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDatabaseName = true;
        result.databaseName_ = value;
        return this;
      }
      public Builder ClearDatabaseName() {
        PrepareBuilder();
        result.hasDatabaseName = false;
        result.databaseName_ = "";
        return this;
      }
      
      public bool HasCollectionName {
        get { return result.hasCollectionName; }
      }
      public string CollectionName {
        get { return result.CollectionName; }
        set { SetCollectionName(value); }
      }
      public Builder SetCollectionName(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCollectionName = true;
        result.collectionName_ = value;
        return this;
      }
      public Builder ClearCollectionName() {
        PrepareBuilder();
        result.hasCollectionName = false;
        result.collectionName_ = "";
        return this;
      }
      
      public bool HasFlag {
        get { return result.hasFlag; }
      }
      public int Flag {
        get { return result.Flag; }
        set { SetFlag(value); }
      }
      public Builder SetFlag(int value) {
        PrepareBuilder();
        result.hasFlag = true;
        result.flag_ = value;
        return this;
      }
      public Builder ClearFlag() {
        PrepareBuilder();
        result.hasFlag = false;
        result.flag_ = 0;
        return this;
      }
      
      public bool HasSessionId {
       get { return result.hasSessionId; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.SessionId SessionId {
        get { return result.SessionId; }
        set { SetSessionId(value); }
      }
      public Builder SetSessionId(global::Alachisoft.NosDB.Common.Protobuf.SessionId value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasSessionId = true;
        result.sessionId_ = value;
        return this;
      }
      public Builder SetSessionId(global::Alachisoft.NosDB.Common.Protobuf.SessionId.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasSessionId = true;
        result.sessionId_ = builderForValue.Build();
        return this;
      }
      public Builder MergeSessionId(global::Alachisoft.NosDB.Common.Protobuf.SessionId value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasSessionId &&
            result.sessionId_ != global::Alachisoft.NosDB.Common.Protobuf.SessionId.DefaultInstance) {
            result.sessionId_ = global::Alachisoft.NosDB.Common.Protobuf.SessionId.CreateBuilder(result.sessionId_).MergeFrom(value).BuildPartial();
        } else {
          result.sessionId_ = value;
        }
        result.hasSessionId = true;
        return this;
      }
      public Builder ClearSessionId() {
        PrepareBuilder();
        result.hasSessionId = false;
        result.sessionId_ = null;
        return this;
      }
      
      public bool HasNoResponse {
        get { return result.hasNoResponse; }
      }
      public bool NoResponse {
        get { return result.NoResponse; }
        set { SetNoResponse(value); }
      }
      public Builder SetNoResponse(bool value) {
        PrepareBuilder();
        result.hasNoResponse = true;
        result.noResponse_ = value;
        return this;
      }
      public Builder ClearNoResponse() {
        PrepareBuilder();
        result.hasNoResponse = false;
        result.noResponse_ = false;
        return this;
      }
      
      public bool HasInsertDocumentsCommand {
       get { return result.hasInsertDocumentsCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand InsertDocumentsCommand {
        get { return result.InsertDocumentsCommand; }
        set { SetInsertDocumentsCommand(value); }
      }
      public Builder SetInsertDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasInsertDocumentsCommand = true;
        result.insertDocumentsCommand_ = value;
        return this;
      }
      public Builder SetInsertDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasInsertDocumentsCommand = true;
        result.insertDocumentsCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeInsertDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasInsertDocumentsCommand &&
            result.insertDocumentsCommand_ != global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand.DefaultInstance) {
            result.insertDocumentsCommand_ = global::Alachisoft.NosDB.Common.Protobuf.InsertDocumentsCommand.CreateBuilder(result.insertDocumentsCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.insertDocumentsCommand_ = value;
        }
        result.hasInsertDocumentsCommand = true;
        return this;
      }
      public Builder ClearInsertDocumentsCommand() {
        PrepareBuilder();
        result.hasInsertDocumentsCommand = false;
        result.insertDocumentsCommand_ = null;
        return this;
      }
      
      public bool HasDeleteDocumentsCommand {
       get { return result.hasDeleteDocumentsCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand DeleteDocumentsCommand {
        get { return result.DeleteDocumentsCommand; }
        set { SetDeleteDocumentsCommand(value); }
      }
      public Builder SetDeleteDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDeleteDocumentsCommand = true;
        result.deleteDocumentsCommand_ = value;
        return this;
      }
      public Builder SetDeleteDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasDeleteDocumentsCommand = true;
        result.deleteDocumentsCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeDeleteDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasDeleteDocumentsCommand &&
            result.deleteDocumentsCommand_ != global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand.DefaultInstance) {
            result.deleteDocumentsCommand_ = global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsCommand.CreateBuilder(result.deleteDocumentsCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.deleteDocumentsCommand_ = value;
        }
        result.hasDeleteDocumentsCommand = true;
        return this;
      }
      public Builder ClearDeleteDocumentsCommand() {
        PrepareBuilder();
        result.hasDeleteDocumentsCommand = false;
        result.deleteDocumentsCommand_ = null;
        return this;
      }
      
      public bool HasGetDocumentsCommand {
       get { return result.hasGetDocumentsCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand GetDocumentsCommand {
        get { return result.GetDocumentsCommand; }
        set { SetGetDocumentsCommand(value); }
      }
      public Builder SetGetDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasGetDocumentsCommand = true;
        result.getDocumentsCommand_ = value;
        return this;
      }
      public Builder SetGetDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasGetDocumentsCommand = true;
        result.getDocumentsCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeGetDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasGetDocumentsCommand &&
            result.getDocumentsCommand_ != global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.DefaultInstance) {
            result.getDocumentsCommand_ = global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.CreateBuilder(result.getDocumentsCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.getDocumentsCommand_ = value;
        }
        result.hasGetDocumentsCommand = true;
        return this;
      }
      public Builder ClearGetDocumentsCommand() {
        PrepareBuilder();
        result.hasGetDocumentsCommand = false;
        result.getDocumentsCommand_ = null;
        return this;
      }
      
      public bool HasUpdateCommand {
       get { return result.hasUpdateCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand UpdateCommand {
        get { return result.UpdateCommand; }
        set { SetUpdateCommand(value); }
      }
      public Builder SetUpdateCommand(global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasUpdateCommand = true;
        result.updateCommand_ = value;
        return this;
      }
      public Builder SetUpdateCommand(global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasUpdateCommand = true;
        result.updateCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeUpdateCommand(global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasUpdateCommand &&
            result.updateCommand_ != global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.DefaultInstance) {
            result.updateCommand_ = global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.CreateBuilder(result.updateCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.updateCommand_ = value;
        }
        result.hasUpdateCommand = true;
        return this;
      }
      public Builder ClearUpdateCommand() {
        PrepareBuilder();
        result.hasUpdateCommand = false;
        result.updateCommand_ = null;
        return this;
      }
      
      public bool HasReadQueryCommand {
       get { return result.hasReadQueryCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand ReadQueryCommand {
        get { return result.ReadQueryCommand; }
        set { SetReadQueryCommand(value); }
      }
      public Builder SetReadQueryCommand(global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasReadQueryCommand = true;
        result.readQueryCommand_ = value;
        return this;
      }
      public Builder SetReadQueryCommand(global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasReadQueryCommand = true;
        result.readQueryCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeReadQueryCommand(global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasReadQueryCommand &&
            result.readQueryCommand_ != global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.DefaultInstance) {
            result.readQueryCommand_ = global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.CreateBuilder(result.readQueryCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.readQueryCommand_ = value;
        }
        result.hasReadQueryCommand = true;
        return this;
      }
      public Builder ClearReadQueryCommand() {
        PrepareBuilder();
        result.hasReadQueryCommand = false;
        result.readQueryCommand_ = null;
        return this;
      }
      
      public bool HasWriteQueryCommand {
       get { return result.hasWriteQueryCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand WriteQueryCommand {
        get { return result.WriteQueryCommand; }
        set { SetWriteQueryCommand(value); }
      }
      public Builder SetWriteQueryCommand(global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasWriteQueryCommand = true;
        result.writeQueryCommand_ = value;
        return this;
      }
      public Builder SetWriteQueryCommand(global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasWriteQueryCommand = true;
        result.writeQueryCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeWriteQueryCommand(global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasWriteQueryCommand &&
            result.writeQueryCommand_ != global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand.DefaultInstance) {
            result.writeQueryCommand_ = global::Alachisoft.NosDB.Common.Protobuf.WriteQueryCommand.CreateBuilder(result.writeQueryCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.writeQueryCommand_ = value;
        }
        result.hasWriteQueryCommand = true;
        return this;
      }
      public Builder ClearWriteQueryCommand() {
        PrepareBuilder();
        result.hasWriteQueryCommand = false;
        result.writeQueryCommand_ = null;
        return this;
      }
      
      public bool HasCreateCollectionCommand {
       get { return result.hasCreateCollectionCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand CreateCollectionCommand {
        get { return result.CreateCollectionCommand; }
        set { SetCreateCollectionCommand(value); }
      }
      public Builder SetCreateCollectionCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCreateCollectionCommand = true;
        result.createCollectionCommand_ = value;
        return this;
      }
      public Builder SetCreateCollectionCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasCreateCollectionCommand = true;
        result.createCollectionCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeCreateCollectionCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasCreateCollectionCommand &&
            result.createCollectionCommand_ != global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.DefaultInstance) {
            result.createCollectionCommand_ = global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.CreateBuilder(result.createCollectionCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.createCollectionCommand_ = value;
        }
        result.hasCreateCollectionCommand = true;
        return this;
      }
      public Builder ClearCreateCollectionCommand() {
        PrepareBuilder();
        result.hasCreateCollectionCommand = false;
        result.createCollectionCommand_ = null;
        return this;
      }
      
      public bool HasDropCollectionCommand {
       get { return result.hasDropCollectionCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand DropCollectionCommand {
        get { return result.DropCollectionCommand; }
        set { SetDropCollectionCommand(value); }
      }
      public Builder SetDropCollectionCommand(global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDropCollectionCommand = true;
        result.dropCollectionCommand_ = value;
        return this;
      }
      public Builder SetDropCollectionCommand(global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasDropCollectionCommand = true;
        result.dropCollectionCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeDropCollectionCommand(global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasDropCollectionCommand &&
            result.dropCollectionCommand_ != global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand.DefaultInstance) {
            result.dropCollectionCommand_ = global::Alachisoft.NosDB.Common.Protobuf.DropCollectionCommand.CreateBuilder(result.dropCollectionCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.dropCollectionCommand_ = value;
        }
        result.hasDropCollectionCommand = true;
        return this;
      }
      public Builder ClearDropCollectionCommand() {
        PrepareBuilder();
        result.hasDropCollectionCommand = false;
        result.dropCollectionCommand_ = null;
        return this;
      }
      
      public bool HasCreateSessionCommand {
       get { return result.hasCreateSessionCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand CreateSessionCommand {
        get { return result.CreateSessionCommand; }
        set { SetCreateSessionCommand(value); }
      }
      public Builder SetCreateSessionCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCreateSessionCommand = true;
        result.createSessionCommand_ = value;
        return this;
      }
      public Builder SetCreateSessionCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasCreateSessionCommand = true;
        result.createSessionCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeCreateSessionCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasCreateSessionCommand &&
            result.createSessionCommand_ != global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.DefaultInstance) {
            result.createSessionCommand_ = global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.CreateBuilder(result.createSessionCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.createSessionCommand_ = value;
        }
        result.hasCreateSessionCommand = true;
        return this;
      }
      public Builder ClearCreateSessionCommand() {
        PrepareBuilder();
        result.hasCreateSessionCommand = false;
        result.createSessionCommand_ = null;
        return this;
      }
      
      public bool HasDropSessionCommand {
       get { return result.hasDropSessionCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand DropSessionCommand {
        get { return result.DropSessionCommand; }
        set { SetDropSessionCommand(value); }
      }
      public Builder SetDropSessionCommand(global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDropSessionCommand = true;
        result.dropSessionCommand_ = value;
        return this;
      }
      public Builder SetDropSessionCommand(global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasDropSessionCommand = true;
        result.dropSessionCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeDropSessionCommand(global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasDropSessionCommand &&
            result.dropSessionCommand_ != global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand.DefaultInstance) {
            result.dropSessionCommand_ = global::Alachisoft.NosDB.Common.Protobuf.DropSessionCommand.CreateBuilder(result.dropSessionCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.dropSessionCommand_ = value;
        }
        result.hasDropSessionCommand = true;
        return this;
      }
      public Builder ClearDropSessionCommand() {
        PrepareBuilder();
        result.hasDropSessionCommand = false;
        result.dropSessionCommand_ = null;
        return this;
      }
      
      public bool HasCreateIndexCommand {
       get { return result.hasCreateIndexCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand CreateIndexCommand {
        get { return result.CreateIndexCommand; }
        set { SetCreateIndexCommand(value); }
      }
      public Builder SetCreateIndexCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCreateIndexCommand = true;
        result.createIndexCommand_ = value;
        return this;
      }
      public Builder SetCreateIndexCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasCreateIndexCommand = true;
        result.createIndexCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeCreateIndexCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasCreateIndexCommand &&
            result.createIndexCommand_ != global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.DefaultInstance) {
            result.createIndexCommand_ = global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.CreateBuilder(result.createIndexCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.createIndexCommand_ = value;
        }
        result.hasCreateIndexCommand = true;
        return this;
      }
      public Builder ClearCreateIndexCommand() {
        PrepareBuilder();
        result.hasCreateIndexCommand = false;
        result.createIndexCommand_ = null;
        return this;
      }
      
      public bool HasDropIndexCommand {
       get { return result.hasDropIndexCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand DropIndexCommand {
        get { return result.DropIndexCommand; }
        set { SetDropIndexCommand(value); }
      }
      public Builder SetDropIndexCommand(global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDropIndexCommand = true;
        result.dropIndexCommand_ = value;
        return this;
      }
      public Builder SetDropIndexCommand(global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasDropIndexCommand = true;
        result.dropIndexCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeDropIndexCommand(global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasDropIndexCommand &&
            result.dropIndexCommand_ != global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.DefaultInstance) {
            result.dropIndexCommand_ = global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.CreateBuilder(result.dropIndexCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.dropIndexCommand_ = value;
        }
        result.hasDropIndexCommand = true;
        return this;
      }
      public Builder ClearDropIndexCommand() {
        PrepareBuilder();
        result.hasDropIndexCommand = false;
        result.dropIndexCommand_ = null;
        return this;
      }
      
      public bool HasGetChunkCommand {
       get { return result.hasGetChunkCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand GetChunkCommand {
        get { return result.GetChunkCommand; }
        set { SetGetChunkCommand(value); }
      }
      public Builder SetGetChunkCommand(global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasGetChunkCommand = true;
        result.getChunkCommand_ = value;
        return this;
      }
      public Builder SetGetChunkCommand(global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasGetChunkCommand = true;
        result.getChunkCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeGetChunkCommand(global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasGetChunkCommand &&
            result.getChunkCommand_ != global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.DefaultInstance) {
            result.getChunkCommand_ = global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.CreateBuilder(result.getChunkCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.getChunkCommand_ = value;
        }
        result.hasGetChunkCommand = true;
        return this;
      }
      public Builder ClearGetChunkCommand() {
        PrepareBuilder();
        result.hasGetChunkCommand = false;
        result.getChunkCommand_ = null;
        return this;
      }
      
      public bool HasDisposeReaderCommand {
       get { return result.hasDisposeReaderCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand DisposeReaderCommand {
        get { return result.DisposeReaderCommand; }
        set { SetDisposeReaderCommand(value); }
      }
      public Builder SetDisposeReaderCommand(global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDisposeReaderCommand = true;
        result.disposeReaderCommand_ = value;
        return this;
      }
      public Builder SetDisposeReaderCommand(global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasDisposeReaderCommand = true;
        result.disposeReaderCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeDisposeReaderCommand(global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasDisposeReaderCommand &&
            result.disposeReaderCommand_ != global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.DefaultInstance) {
            result.disposeReaderCommand_ = global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.CreateBuilder(result.disposeReaderCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.disposeReaderCommand_ = value;
        }
        result.hasDisposeReaderCommand = true;
        return this;
      }
      public Builder ClearDisposeReaderCommand() {
        PrepareBuilder();
        result.hasDisposeReaderCommand = false;
        result.disposeReaderCommand_ = null;
        return this;
      }
      
      public bool HasReplaceDocumentsCommand {
       get { return result.hasReplaceDocumentsCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand ReplaceDocumentsCommand {
        get { return result.ReplaceDocumentsCommand; }
        set { SetReplaceDocumentsCommand(value); }
      }
      public Builder SetReplaceDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasReplaceDocumentsCommand = true;
        result.replaceDocumentsCommand_ = value;
        return this;
      }
      public Builder SetReplaceDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasReplaceDocumentsCommand = true;
        result.replaceDocumentsCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeReplaceDocumentsCommand(global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasReplaceDocumentsCommand &&
            result.replaceDocumentsCommand_ != global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.DefaultInstance) {
            result.replaceDocumentsCommand_ = global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.CreateBuilder(result.replaceDocumentsCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.replaceDocumentsCommand_ = value;
        }
        result.hasReplaceDocumentsCommand = true;
        return this;
      }
      public Builder ClearReplaceDocumentsCommand() {
        PrepareBuilder();
        result.hasReplaceDocumentsCommand = false;
        result.replaceDocumentsCommand_ = null;
        return this;
      }
      
      public bool HasAuthenticationCommand {
       get { return result.hasAuthenticationCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand AuthenticationCommand {
        get { return result.AuthenticationCommand; }
        set { SetAuthenticationCommand(value); }
      }
      public Builder SetAuthenticationCommand(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasAuthenticationCommand = true;
        result.authenticationCommand_ = value;
        return this;
      }
      public Builder SetAuthenticationCommand(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasAuthenticationCommand = true;
        result.authenticationCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeAuthenticationCommand(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasAuthenticationCommand &&
            result.authenticationCommand_ != global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.DefaultInstance) {
            result.authenticationCommand_ = global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.CreateBuilder(result.authenticationCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.authenticationCommand_ = value;
        }
        result.hasAuthenticationCommand = true;
        return this;
      }
      public Builder ClearAuthenticationCommand() {
        PrepareBuilder();
        result.hasAuthenticationCommand = false;
        result.authenticationCommand_ = null;
        return this;
      }
      
      public bool HasInitDatabaseCommand {
       get { return result.hasInitDatabaseCommand; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand InitDatabaseCommand {
        get { return result.InitDatabaseCommand; }
        set { SetInitDatabaseCommand(value); }
      }
      public Builder SetInitDatabaseCommand(global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasInitDatabaseCommand = true;
        result.initDatabaseCommand_ = value;
        return this;
      }
      public Builder SetInitDatabaseCommand(global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasInitDatabaseCommand = true;
        result.initDatabaseCommand_ = builderForValue.Build();
        return this;
      }
      public Builder MergeInitDatabaseCommand(global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasInitDatabaseCommand &&
            result.initDatabaseCommand_ != global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand.DefaultInstance) {
            result.initDatabaseCommand_ = global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseCommand.CreateBuilder(result.initDatabaseCommand_).MergeFrom(value).BuildPartial();
        } else {
          result.initDatabaseCommand_ = value;
        }
        result.hasInitDatabaseCommand = true;
        return this;
      }
      public Builder ClearInitDatabaseCommand() {
        PrepareBuilder();
        result.hasInitDatabaseCommand = false;
        result.initDatabaseCommand_ = null;
        return this;
      }
    }
    static Command() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.Command.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
