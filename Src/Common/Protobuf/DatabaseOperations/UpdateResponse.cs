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
    public static partial class UpdateResponse {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateResponse__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse, global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateResponse__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static UpdateResponse() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChRVcGRhdGVSZXNwb25zZS5wcm90bxIgQWxhY2hpc29mdC5Ob3NEQi5Db21t", 
              "b24uUHJvdG9idWYiKwoOVXBkYXRlUmVzcG9uc2USGQoRYWZmZWN0ZWREb2N1", 
              "bWVudHMYASABKANCPgokY29tLmFsYWNoaXNvZnQubm9zZGIuY29tbW9uLnBy", 
            "b3RvYnVmQhZVcGRhdGVSZXNwb25zZVByb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateResponse__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateResponse__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse, global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateResponse__Descriptor,
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
  public sealed partial class UpdateResponse : pb::GeneratedMessage<UpdateResponse, UpdateResponse.Builder> {
    private UpdateResponse() { }
    private static readonly UpdateResponse defaultInstance = new UpdateResponse().MakeReadOnly();
    private static readonly string[] _updateResponseFieldNames = new string[] { "affectedDocuments" };
    private static readonly uint[] _updateResponseFieldTags = new uint[] { 8 };
    public static UpdateResponse DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override UpdateResponse DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override UpdateResponse ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.UpdateResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateResponse__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<UpdateResponse, UpdateResponse.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.UpdateResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateResponse__FieldAccessorTable; }
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
      string[] field_names = _updateResponseFieldNames;
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
    public static UpdateResponse ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static UpdateResponse ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static UpdateResponse ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static UpdateResponse ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static UpdateResponse ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static UpdateResponse ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static UpdateResponse ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static UpdateResponse ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static UpdateResponse ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static UpdateResponse ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private UpdateResponse MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(UpdateResponse prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<UpdateResponse, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(UpdateResponse cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private UpdateResponse result;
      
      private UpdateResponse PrepareBuilder() {
        if (resultIsReadOnly) {
          UpdateResponse original = result;
          result = new UpdateResponse();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override UpdateResponse MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.Descriptor; }
      }
      
      public override UpdateResponse DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.DefaultInstance; }
      }
      
      public override UpdateResponse BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is UpdateResponse) {
          return MergeFrom((UpdateResponse) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(UpdateResponse other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.UpdateResponse.DefaultInstance) return this;
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
            int field_ordinal = global::System.Array.BinarySearch(_updateResponseFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _updateResponseFieldTags[field_ordinal];
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
    static UpdateResponse() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.UpdateResponse.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
