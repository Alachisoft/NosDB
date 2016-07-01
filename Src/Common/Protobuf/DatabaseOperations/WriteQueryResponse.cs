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
    public static partial class WriteQueryResponse {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_WriteQueryResponse__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse, global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_WriteQueryResponse__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static WriteQueryResponse() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChhXcml0ZVF1ZXJ5UmVzcG9uc2UucHJvdG8SIEFsYWNoaXNvZnQuTm9zREIu", 
              "Q29tbW9uLlByb3RvYnVmIi8KEldyaXRlUXVlcnlSZXNwb25zZRIZChFhZmZl", 
              "Y3RlZERvY3VtZW50cxgBIAEoA0JCCiRjb20uYWxhY2hpc29mdC5ub3NkYi5j", 
            "b21tb24ucHJvdG9idWZCGldyaXRlUXVlcnlSZXNwb25zZVByb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_WriteQueryResponse__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_WriteQueryResponse__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse, global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_WriteQueryResponse__Descriptor,
                  new string[] { "AffectedDocuments", });
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
  public sealed partial class WriteQueryResponse : pb::GeneratedMessage<WriteQueryResponse, WriteQueryResponse.Builder> {
    private WriteQueryResponse() { }
    private static readonly WriteQueryResponse defaultInstance = new WriteQueryResponse().MakeReadOnly();
    private static readonly string[] _writeQueryResponseFieldNames = new string[] { "affectedDocuments" };
    private static readonly uint[] _writeQueryResponseFieldTags = new uint[] { 8 };
    public static WriteQueryResponse DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override WriteQueryResponse DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override WriteQueryResponse ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.WriteQueryResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_WriteQueryResponse__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<WriteQueryResponse, WriteQueryResponse.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.WriteQueryResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_WriteQueryResponse__FieldAccessorTable; }
    }
    
    public const int AffectedDocumentsFieldNumber = 1;
    private bool hasAffectedDocuments;
    private long affectedDocuments_;
    public bool HasAffectedDocuments {
      get { return hasAffectedDocuments; }
    }
    public long AffectedDocuments {
      get { return affectedDocuments_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _writeQueryResponseFieldNames;
      if (hasAffectedDocuments) {
        output.WriteInt64(1, field_names[0], AffectedDocuments);
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
      if (hasAffectedDocuments) {
        size += pb::CodedOutputStream.ComputeInt64Size(1, AffectedDocuments);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static WriteQueryResponse ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static WriteQueryResponse ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static WriteQueryResponse ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static WriteQueryResponse ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static WriteQueryResponse ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static WriteQueryResponse ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static WriteQueryResponse ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static WriteQueryResponse ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static WriteQueryResponse ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static WriteQueryResponse ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private WriteQueryResponse MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(WriteQueryResponse prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<WriteQueryResponse, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(WriteQueryResponse cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private WriteQueryResponse result;
      
      private WriteQueryResponse PrepareBuilder() {
        if (resultIsReadOnly) {
          WriteQueryResponse original = result;
          result = new WriteQueryResponse();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override WriteQueryResponse MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.Descriptor; }
      }
      
      public override WriteQueryResponse DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.DefaultInstance; }
      }
      
      public override WriteQueryResponse BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is WriteQueryResponse) {
          return MergeFrom((WriteQueryResponse) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(WriteQueryResponse other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.WriteQueryResponse.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasAffectedDocuments) {
          AffectedDocuments = other.AffectedDocuments;
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
            int field_ordinal = global::System.Array.BinarySearch(_writeQueryResponseFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _writeQueryResponseFieldTags[field_ordinal];
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
              result.hasAffectedDocuments = input.ReadInt64(ref result.affectedDocuments_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasAffectedDocuments {
        get { return result.hasAffectedDocuments; }
      }
      public long AffectedDocuments {
        get { return result.AffectedDocuments; }
        set { SetAffectedDocuments(value); }
      }
      public Builder SetAffectedDocuments(long value) {
        PrepareBuilder();
        result.hasAffectedDocuments = true;
        result.affectedDocuments_ = value;
        return this;
      }
      public Builder ClearAffectedDocuments() {
        PrepareBuilder();
        result.hasAffectedDocuments = false;
        result.affectedDocuments_ = 0L;
        return this;
      }
    }
    static WriteQueryResponse() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.WriteQueryResponse.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
