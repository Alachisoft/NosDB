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
    public static partial class GetChunkResponse {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkResponse__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse, global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkResponse__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static GetChunkResponse() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChZHZXRDaHVua1Jlc3BvbnNlLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNv", 
              "bW1vbi5Qcm90b2J1ZhoPRGF0YUNodW5rLnByb3RvIlIKEEdldENodW5rUmVz", 
              "cG9uc2USPgoJZGF0YUNodW5rGAEgASgLMisuQWxhY2hpc29mdC5Ob3NEQi5D", 
              "b21tb24uUHJvdG9idWYuRGF0YUNodW5rQkAKJGNvbS5hbGFjaGlzb2Z0Lm5v", 
            "c2RiLmNvbW1vbi5wcm90b2J1ZkIYR2V0Q2h1bmtSZXNwb25zZVByb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkResponse__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkResponse__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse, global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkResponse__Descriptor,
                  new string[] { "DataChunk", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.DataChunk.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class GetChunkResponse : pb::GeneratedMessage<GetChunkResponse, GetChunkResponse.Builder> {
    private GetChunkResponse() { }
    private static readonly GetChunkResponse defaultInstance = new GetChunkResponse().MakeReadOnly();
    private static readonly string[] _getChunkResponseFieldNames = new string[] { "dataChunk" };
    private static readonly uint[] _getChunkResponseFieldTags = new uint[] { 10 };
    public static GetChunkResponse DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override GetChunkResponse DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override GetChunkResponse ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.GetChunkResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkResponse__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<GetChunkResponse, GetChunkResponse.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.GetChunkResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkResponse__FieldAccessorTable; }
    }
    
    public const int DataChunkFieldNumber = 1;
    private bool hasDataChunk;
    private global::Alachisoft.NosDB.Common.Protobuf.DataChunk dataChunk_;
    public bool HasDataChunk {
      get { return hasDataChunk; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.DataChunk DataChunk {
      get { return dataChunk_ ?? global::Alachisoft.NosDB.Common.Protobuf.DataChunk.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _getChunkResponseFieldNames;
      if (hasDataChunk) {
        output.WriteMessage(1, field_names[0], DataChunk);
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
      if (hasDataChunk) {
        size += pb::CodedOutputStream.ComputeMessageSize(1, DataChunk);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static GetChunkResponse ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static GetChunkResponse ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static GetChunkResponse ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static GetChunkResponse ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static GetChunkResponse ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static GetChunkResponse ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static GetChunkResponse ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static GetChunkResponse ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static GetChunkResponse ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static GetChunkResponse ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private GetChunkResponse MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(GetChunkResponse prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<GetChunkResponse, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(GetChunkResponse cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private GetChunkResponse result;
      
      private GetChunkResponse PrepareBuilder() {
        if (resultIsReadOnly) {
          GetChunkResponse original = result;
          result = new GetChunkResponse();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override GetChunkResponse MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.Descriptor; }
      }
      
      public override GetChunkResponse DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.DefaultInstance; }
      }
      
      public override GetChunkResponse BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is GetChunkResponse) {
          return MergeFrom((GetChunkResponse) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(GetChunkResponse other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.GetChunkResponse.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasDataChunk) {
          MergeDataChunk(other.DataChunk);
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
            int field_ordinal = global::System.Array.BinarySearch(_getChunkResponseFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _getChunkResponseFieldTags[field_ordinal];
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
              global::Alachisoft.NosDB.Common.Protobuf.DataChunk.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.DataChunk.CreateBuilder();
              if (result.hasDataChunk) {
                subBuilder.MergeFrom(DataChunk);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              DataChunk = subBuilder.BuildPartial();
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasDataChunk {
       get { return result.hasDataChunk; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.DataChunk DataChunk {
        get { return result.DataChunk; }
        set { SetDataChunk(value); }
      }
      public Builder SetDataChunk(global::Alachisoft.NosDB.Common.Protobuf.DataChunk value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDataChunk = true;
        result.dataChunk_ = value;
        return this;
      }
      public Builder SetDataChunk(global::Alachisoft.NosDB.Common.Protobuf.DataChunk.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasDataChunk = true;
        result.dataChunk_ = builderForValue.Build();
        return this;
      }
      public Builder MergeDataChunk(global::Alachisoft.NosDB.Common.Protobuf.DataChunk value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasDataChunk &&
            result.dataChunk_ != global::Alachisoft.NosDB.Common.Protobuf.DataChunk.DefaultInstance) {
            result.dataChunk_ = global::Alachisoft.NosDB.Common.Protobuf.DataChunk.CreateBuilder(result.dataChunk_).MergeFrom(value).BuildPartial();
        } else {
          result.dataChunk_ = value;
        }
        result.hasDataChunk = true;
        return this;
      }
      public Builder ClearDataChunk() {
        PrepareBuilder();
        result.hasDataChunk = false;
        result.dataChunk_ = null;
        return this;
      }
    }
    static GetChunkResponse() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.GetChunkResponse.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
