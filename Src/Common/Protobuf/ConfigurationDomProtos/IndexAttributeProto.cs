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
    public static partial class IndexAttributeProto {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_IndexAttributeProto__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto, global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_IndexAttributeProto__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static IndexAttributeProto() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChlJbmRleEF0dHJpYnV0ZVByb3RvLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RC", 
              "LkNvbW1vbi5Qcm90b2J1ZiIyChNJbmRleEF0dHJpYnV0ZVByb3RvEgwKBG5h", 
              "bWUYASABKAkSDQoFb3JkZXIYAiABKAlCQwokY29tLmFsYWNoaXNvZnQubm9z", 
              "ZGIuY29tbW9uLnByb3RvYnVmQhtJbmRleEF0dHJpYnV0ZVByb3RvUHJvdG9j", 
            "b2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_IndexAttributeProto__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_IndexAttributeProto__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto, global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_IndexAttributeProto__Descriptor,
                  new string[] { "Name", "Order", });
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
  public sealed partial class IndexAttributeProto : pb::GeneratedMessage<IndexAttributeProto, IndexAttributeProto.Builder> {
    private IndexAttributeProto() { }
    private static readonly IndexAttributeProto defaultInstance = new IndexAttributeProto().MakeReadOnly();
    private static readonly string[] _indexAttributeProtoFieldNames = new string[] { "name", "order" };
    private static readonly uint[] _indexAttributeProtoFieldTags = new uint[] { 10, 18 };
    public static IndexAttributeProto DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override IndexAttributeProto DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override IndexAttributeProto ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.IndexAttributeProto.internal__static_Alachisoft_NosDB_Common_Protobuf_IndexAttributeProto__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<IndexAttributeProto, IndexAttributeProto.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.IndexAttributeProto.internal__static_Alachisoft_NosDB_Common_Protobuf_IndexAttributeProto__FieldAccessorTable; }
    }
    
    public const int NameFieldNumber = 1;
    private bool hasName;
    private string name_ = "";
    public bool HasName {
      get { return hasName; }
    }
    public string Name {
      get { return name_; }
    }
    
    public const int OrderFieldNumber = 2;
    private bool hasOrder;
    private string order_ = "";
    public bool HasOrder {
      get { return hasOrder; }
    }
    public string Order {
      get { return order_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _indexAttributeProtoFieldNames;
      if (hasName) {
        output.WriteString(1, field_names[0], Name);
      }
      if (hasOrder) {
        output.WriteString(2, field_names[1], Order);
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
      if (hasName) {
        size += pb::CodedOutputStream.ComputeStringSize(1, Name);
      }
      if (hasOrder) {
        size += pb::CodedOutputStream.ComputeStringSize(2, Order);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static IndexAttributeProto ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static IndexAttributeProto ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static IndexAttributeProto ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static IndexAttributeProto ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static IndexAttributeProto ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static IndexAttributeProto ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static IndexAttributeProto ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static IndexAttributeProto ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static IndexAttributeProto ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static IndexAttributeProto ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private IndexAttributeProto MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(IndexAttributeProto prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<IndexAttributeProto, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(IndexAttributeProto cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private IndexAttributeProto result;
      
      private IndexAttributeProto PrepareBuilder() {
        if (resultIsReadOnly) {
          IndexAttributeProto original = result;
          result = new IndexAttributeProto();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override IndexAttributeProto MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.Descriptor; }
      }
      
      public override IndexAttributeProto DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.DefaultInstance; }
      }
      
      public override IndexAttributeProto BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is IndexAttributeProto) {
          return MergeFrom((IndexAttributeProto) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(IndexAttributeProto other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasName) {
          Name = other.Name;
        }
        if (other.HasOrder) {
          Order = other.Order;
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
            int field_ordinal = global::System.Array.BinarySearch(_indexAttributeProtoFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _indexAttributeProtoFieldTags[field_ordinal];
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
              result.hasName = input.ReadString(ref result.name_);
              break;
            }
            case 18: {
              result.hasOrder = input.ReadString(ref result.order_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasName {
        get { return result.hasName; }
      }
      public string Name {
        get { return result.Name; }
        set { SetName(value); }
      }
      public Builder SetName(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasName = true;
        result.name_ = value;
        return this;
      }
      public Builder ClearName() {
        PrepareBuilder();
        result.hasName = false;
        result.name_ = "";
        return this;
      }
      
      public bool HasOrder {
        get { return result.hasOrder; }
      }
      public string Order {
        get { return result.Order; }
        set { SetOrder(value); }
      }
      public Builder SetOrder(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasOrder = true;
        result.order_ = value;
        return this;
      }
      public Builder ClearOrder() {
        PrepareBuilder();
        result.hasOrder = false;
        result.order_ = "";
        return this;
      }
    }
    static IndexAttributeProto() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.IndexAttributeProto.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
