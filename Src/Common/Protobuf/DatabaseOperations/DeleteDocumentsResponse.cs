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
    public static partial class DeleteDocumentsResponse {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_DeleteDocumentsResponse__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse, global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_DeleteDocumentsResponse__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static DeleteDocumentsResponse() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "Ch1EZWxldGVEb2N1bWVudHNSZXNwb25zZS5wcm90bxIgQWxhY2hpc29mdC5O", 
              "b3NEQi5Db21tb24uUHJvdG9idWYaFEZhaWxlZERvY3VtZW50LnByb3RvImQK", 
              "F0RlbGV0ZURvY3VtZW50c1Jlc3BvbnNlEkkKD2ZhaWxlZERvY3VtZW50cxgB", 
              "IAMoCzIwLkFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmLkZhaWxl", 
              "ZERvY3VtZW50QkcKJGNvbS5hbGFjaGlzb2Z0Lm5vc2RiLmNvbW1vbi5wcm90", 
            "b2J1ZkIfRGVsZXRlRG9jdW1lbnRzUmVzcG9uc2VQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_DeleteDocumentsResponse__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_DeleteDocumentsResponse__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse, global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_DeleteDocumentsResponse__Descriptor,
                  new string[] { "FailedDocuments", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.FailedDocument.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class DeleteDocumentsResponse : pb::GeneratedMessage<DeleteDocumentsResponse, DeleteDocumentsResponse.Builder> {
    private DeleteDocumentsResponse() { }
    private static readonly DeleteDocumentsResponse defaultInstance = new DeleteDocumentsResponse().MakeReadOnly();
    private static readonly string[] _deleteDocumentsResponseFieldNames = new string[] { "failedDocuments" };
    private static readonly uint[] _deleteDocumentsResponseFieldTags = new uint[] { 10 };
    public static DeleteDocumentsResponse DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override DeleteDocumentsResponse DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override DeleteDocumentsResponse ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DeleteDocumentsResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_DeleteDocumentsResponse__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<DeleteDocumentsResponse, DeleteDocumentsResponse.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DeleteDocumentsResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_DeleteDocumentsResponse__FieldAccessorTable; }
    }
    
    public const int FailedDocumentsFieldNumber = 1;
    private pbc::PopsicleList<global::Alachisoft.NosDB.Common.Protobuf.FailedDocument> failedDocuments_ = new pbc::PopsicleList<global::Alachisoft.NosDB.Common.Protobuf.FailedDocument>();
    public scg::IList<global::Alachisoft.NosDB.Common.Protobuf.FailedDocument> FailedDocumentsList {
      get { return failedDocuments_; }
    }
    public int FailedDocumentsCount {
      get { return failedDocuments_.Count; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.FailedDocument GetFailedDocuments(int index) {
      return failedDocuments_[index];
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _deleteDocumentsResponseFieldNames;
      if (failedDocuments_.Count > 0) {
        output.WriteMessageArray(1, field_names[0], failedDocuments_);
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
      foreach (global::Alachisoft.NosDB.Common.Protobuf.FailedDocument element in FailedDocumentsList) {
        size += pb::CodedOutputStream.ComputeMessageSize(1, element);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static DeleteDocumentsResponse ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DeleteDocumentsResponse ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private DeleteDocumentsResponse MakeReadOnly() {
      failedDocuments_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(DeleteDocumentsResponse prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<DeleteDocumentsResponse, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(DeleteDocumentsResponse cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private DeleteDocumentsResponse result;
      
      private DeleteDocumentsResponse PrepareBuilder() {
        if (resultIsReadOnly) {
          DeleteDocumentsResponse original = result;
          result = new DeleteDocumentsResponse();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override DeleteDocumentsResponse MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.Descriptor; }
      }
      
      public override DeleteDocumentsResponse DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.DefaultInstance; }
      }
      
      public override DeleteDocumentsResponse BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is DeleteDocumentsResponse) {
          return MergeFrom((DeleteDocumentsResponse) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(DeleteDocumentsResponse other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.DeleteDocumentsResponse.DefaultInstance) return this;
        PrepareBuilder();
        if (other.failedDocuments_.Count != 0) {
          result.failedDocuments_.Add(other.failedDocuments_);
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
            int field_ordinal = global::System.Array.BinarySearch(_deleteDocumentsResponseFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _deleteDocumentsResponseFieldTags[field_ordinal];
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
              input.ReadMessageArray(tag, field_name, result.failedDocuments_, global::Alachisoft.NosDB.Common.Protobuf.FailedDocument.DefaultInstance, extensionRegistry);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public pbc::IPopsicleList<global::Alachisoft.NosDB.Common.Protobuf.FailedDocument> FailedDocumentsList {
        get { return PrepareBuilder().failedDocuments_; }
      }
      public int FailedDocumentsCount {
        get { return result.FailedDocumentsCount; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.FailedDocument GetFailedDocuments(int index) {
        return result.GetFailedDocuments(index);
      }
      public Builder SetFailedDocuments(int index, global::Alachisoft.NosDB.Common.Protobuf.FailedDocument value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.failedDocuments_[index] = value;
        return this;
      }
      public Builder SetFailedDocuments(int index, global::Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.failedDocuments_[index] = builderForValue.Build();
        return this;
      }
      public Builder AddFailedDocuments(global::Alachisoft.NosDB.Common.Protobuf.FailedDocument value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.failedDocuments_.Add(value);
        return this;
      }
      public Builder AddFailedDocuments(global::Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.failedDocuments_.Add(builderForValue.Build());
        return this;
      }
      public Builder AddRangeFailedDocuments(scg::IEnumerable<global::Alachisoft.NosDB.Common.Protobuf.FailedDocument> values) {
        PrepareBuilder();
        result.failedDocuments_.Add(values);
        return this;
      }
      public Builder ClearFailedDocuments() {
        PrepareBuilder();
        result.failedDocuments_.Clear();
        return this;
      }
    }
    static DeleteDocumentsResponse() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.DeleteDocumentsResponse.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
