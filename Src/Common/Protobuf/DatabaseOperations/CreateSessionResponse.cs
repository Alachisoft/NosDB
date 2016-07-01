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
    public static partial class CreateSessionResponse {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionResponse__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse, global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionResponse__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static CreateSessionResponse() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChtDcmVhdGVTZXNzaW9uUmVzcG9uc2UucHJvdG8SIEFsYWNoaXNvZnQuTm9z", 
              "REIuQ29tbW9uLlByb3RvYnVmIioKFUNyZWF0ZVNlc3Npb25SZXNwb25zZRIR", 
              "CglzZXNzaW9uSWQYASABKAlCRQokY29tLmFsYWNoaXNvZnQubm9zZGIuY29t", 
            "bW9uLnByb3RvYnVmQh1DcmVhdGVTZXNzaW9uUmVzcG9uc2VQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionResponse__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionResponse__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse, global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionResponse__Descriptor,
                  new string[] { "SessionId", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class CreateSessionResponse : pb::GeneratedMessage<CreateSessionResponse, CreateSessionResponse.Builder> {
    private CreateSessionResponse() { }
    private static readonly CreateSessionResponse defaultInstance = new CreateSessionResponse().MakeReadOnly();
    private static readonly string[] _createSessionResponseFieldNames = new string[] { "sessionId" };
    private static readonly uint[] _createSessionResponseFieldTags = new uint[] { 10 };
    public static CreateSessionResponse DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override CreateSessionResponse DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override CreateSessionResponse ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionResponse__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<CreateSessionResponse, CreateSessionResponse.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionResponse__FieldAccessorTable; }
    }
    
    public const int SessionIdFieldNumber = 1;
    private bool hasSessionId;
    private string sessionId_ = "";
    public bool HasSessionId {
      get { return hasSessionId; }
    }
    public string SessionId {
      get { return sessionId_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _createSessionResponseFieldNames;
      if (hasSessionId) {
        output.WriteString(1, field_names[0], SessionId);
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
      if (hasSessionId) {
        size += pb::CodedOutputStream.ComputeStringSize(1, SessionId);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static CreateSessionResponse ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CreateSessionResponse ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CreateSessionResponse ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CreateSessionResponse ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CreateSessionResponse ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CreateSessionResponse ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static CreateSessionResponse ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static CreateSessionResponse ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static CreateSessionResponse ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CreateSessionResponse ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private CreateSessionResponse MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(CreateSessionResponse prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<CreateSessionResponse, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(CreateSessionResponse cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private CreateSessionResponse result;
      
      private CreateSessionResponse PrepareBuilder() {
        if (resultIsReadOnly) {
          CreateSessionResponse original = result;
          result = new CreateSessionResponse();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override CreateSessionResponse MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.Descriptor; }
      }
      
      public override CreateSessionResponse DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.DefaultInstance; }
      }
      
      public override CreateSessionResponse BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is CreateSessionResponse) {
          return MergeFrom((CreateSessionResponse) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(CreateSessionResponse other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.CreateSessionResponse.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasSessionId) {
          SessionId = other.SessionId;
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
            int field_ordinal = global::System.Array.BinarySearch(_createSessionResponseFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _createSessionResponseFieldTags[field_ordinal];
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
            case 10: {
              result.hasSessionId = input.ReadString(ref result.sessionId_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasSessionId {
        get { return result.hasSessionId; }
      }
      public string SessionId {
        get { return result.SessionId; }
        set { SetSessionId(value); }
      }
      public Builder SetSessionId(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasSessionId = true;
        result.sessionId_ = value;
        return this;
      }
      public Builder ClearSessionId() {
        PrepareBuilder();
        result.hasSessionId = false;
        result.sessionId_ = "";
        return this;
      }
    }
    static CreateSessionResponse() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionResponse.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
