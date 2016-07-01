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
    public static partial class StorageConfig {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_StorageConfig__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.StorageConfig, global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_StorageConfig__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static StorageConfig() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChNTdG9yYWdlQ29uZmlnLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNvbW1v", 
              "bi5Qcm90b2J1ZhoXQ29sbGVjdGlvbkNvbmZpZ3MucHJvdG8aG1N0b3JhZ2VQ", 
              "cm92aWRlckNvbmZpZy5wcm90byKrAQoNU3RvcmFnZUNvbmZpZxJICgtjb2xs", 
              "ZWN0aW9ucxgBIAEoCzIzLkFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlByb3Rv", 
              "YnVmLkNvbGxlY3Rpb25Db25maWdzElAKD3N0b3JhZ2VQcm92aWRlchgCIAEo", 
              "CzI3LkFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmLlN0b3JhZ2VQ", 
              "cm92aWRlckNvbmZpZ0I9CiRjb20uYWxhY2hpc29mdC5ub3NkYi5jb21tb24u", 
            "cHJvdG9idWZCFVN0b3JhZ2VDb25maWdQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_StorageConfig__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_StorageConfig__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.StorageConfig, global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_StorageConfig__Descriptor,
                  new string[] { "Collections", "StorageProvider", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.CollectionConfigs.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.StorageProviderConfig.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class StorageConfig : pb::GeneratedMessage<StorageConfig, StorageConfig.Builder> {
    private StorageConfig() { }
    private static readonly StorageConfig defaultInstance = new StorageConfig().MakeReadOnly();
    private static readonly string[] _storageConfigFieldNames = new string[] { "collections", "storageProvider" };
    private static readonly uint[] _storageConfigFieldTags = new uint[] { 10, 18 };
    public static StorageConfig DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override StorageConfig DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override StorageConfig ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.StorageConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_StorageConfig__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<StorageConfig, StorageConfig.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.StorageConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_StorageConfig__FieldAccessorTable; }
    }
    
    public const int CollectionsFieldNumber = 1;
    private bool hasCollections;
    private global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs collections_;
    public bool HasCollections {
      get { return hasCollections; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs Collections {
      get { return collections_ ?? global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.DefaultInstance; }
    }
    
    public const int StorageProviderFieldNumber = 2;
    private bool hasStorageProvider;
    private global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig storageProvider_;
    public bool HasStorageProvider {
      get { return hasStorageProvider; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig StorageProvider {
      get { return storageProvider_ ?? global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _storageConfigFieldNames;
      if (hasCollections) {
        output.WriteMessage(1, field_names[0], Collections);
      }
      if (hasStorageProvider) {
        output.WriteMessage(2, field_names[1], StorageProvider);
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
      if (hasCollections) {
        size += pb::CodedOutputStream.ComputeMessageSize(1, Collections);
      }
      if (hasStorageProvider) {
        size += pb::CodedOutputStream.ComputeMessageSize(2, StorageProvider);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static StorageConfig ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static StorageConfig ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static StorageConfig ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static StorageConfig ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static StorageConfig ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static StorageConfig ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static StorageConfig ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static StorageConfig ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static StorageConfig ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static StorageConfig ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private StorageConfig MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(StorageConfig prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<StorageConfig, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(StorageConfig cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private StorageConfig result;
      
      private StorageConfig PrepareBuilder() {
        if (resultIsReadOnly) {
          StorageConfig original = result;
          result = new StorageConfig();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override StorageConfig MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.Descriptor; }
      }
      
      public override StorageConfig DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.DefaultInstance; }
      }
      
      public override StorageConfig BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is StorageConfig) {
          return MergeFrom((StorageConfig) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(StorageConfig other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasCollections) {
          MergeCollections(other.Collections);
        }
        if (other.HasStorageProvider) {
          MergeStorageProvider(other.StorageProvider);
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
            int field_ordinal = global::System.Array.BinarySearch(_storageConfigFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _storageConfigFieldTags[field_ordinal];
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
              global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.CreateBuilder();
              if (result.hasCollections) {
                subBuilder.MergeFrom(Collections);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              Collections = subBuilder.BuildPartial();
              break;
            }
            case 18: {
              global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.CreateBuilder();
              if (result.hasStorageProvider) {
                subBuilder.MergeFrom(StorageProvider);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              StorageProvider = subBuilder.BuildPartial();
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasCollections {
       get { return result.hasCollections; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs Collections {
        get { return result.Collections; }
        set { SetCollections(value); }
      }
      public Builder SetCollections(global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCollections = true;
        result.collections_ = value;
        return this;
      }
      public Builder SetCollections(global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasCollections = true;
        result.collections_ = builderForValue.Build();
        return this;
      }
      public Builder MergeCollections(global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasCollections &&
            result.collections_ != global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.DefaultInstance) {
            result.collections_ = global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.CreateBuilder(result.collections_).MergeFrom(value).BuildPartial();
        } else {
          result.collections_ = value;
        }
        result.hasCollections = true;
        return this;
      }
      public Builder ClearCollections() {
        PrepareBuilder();
        result.hasCollections = false;
        result.collections_ = null;
        return this;
      }
      
      public bool HasStorageProvider {
       get { return result.hasStorageProvider; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig StorageProvider {
        get { return result.StorageProvider; }
        set { SetStorageProvider(value); }
      }
      public Builder SetStorageProvider(global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasStorageProvider = true;
        result.storageProvider_ = value;
        return this;
      }
      public Builder SetStorageProvider(global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasStorageProvider = true;
        result.storageProvider_ = builderForValue.Build();
        return this;
      }
      public Builder MergeStorageProvider(global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasStorageProvider &&
            result.storageProvider_ != global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.DefaultInstance) {
            result.storageProvider_ = global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.CreateBuilder(result.storageProvider_).MergeFrom(value).BuildPartial();
        } else {
          result.storageProvider_ = value;
        }
        result.hasStorageProvider = true;
        return this;
      }
      public Builder ClearStorageProvider() {
        PrepareBuilder();
        result.hasStorageProvider = false;
        result.storageProvider_ = null;
        return this;
      }
    }
    static StorageConfig() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.StorageConfig.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
