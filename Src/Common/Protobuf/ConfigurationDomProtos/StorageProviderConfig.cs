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
    public static partial class StorageProviderConfig {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_StorageProviderConfig__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig, global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_StorageProviderConfig__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static StorageProviderConfig() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChtTdG9yYWdlUHJvdmlkZXJDb25maWcucHJvdG8SIEFsYWNoaXNvZnQuTm9z", 
              "REIuQ29tbW9uLlByb3RvYnVmGhBMTURCQ29uZmlnLnByb3RvGhFFU0VOVENv", 
              "bmZpZy5wcm90byLwAQoVU3RvcmFnZVByb3ZpZGVyQ29uZmlnEhcKD21heERh", 
              "dGFiYXNlU2l6ZRgBIAEoAxIaChJwcm92aWRlclR5cGVTdHJpbmcYAiABKAkS", 
              "GAoQaXNNdWx0aUZpbGVTdG9yZRgDIAEoCBJCCgxsTURCUHJvdmlkZXIYBCAB", 
              "KAsyLC5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90b2J1Zi5MTURCQ29u", 
              "ZmlnEkQKDWVTRU5UUHJvdmlkZXIYBSABKAsyLS5BbGFjaGlzb2Z0Lk5vc0RC", 
              "LkNvbW1vbi5Qcm90b2J1Zi5FU0VOVENvbmZpZ0JFCiRjb20uYWxhY2hpc29m", 
              "dC5ub3NkYi5jb21tb24ucHJvdG9idWZCHVN0b3JhZ2VQcm92aWRlckNvbmZp", 
            "Z1Byb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_StorageProviderConfig__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_StorageProviderConfig__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig, global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_StorageProviderConfig__Descriptor,
                  new string[] { "MaxDatabaseSize", "ProviderTypeString", "IsMultiFileStore", "LMDBProvider", "ESENTProvider", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.LMDBConfig.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.ESENTConfig.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class StorageProviderConfig : pb::GeneratedMessage<StorageProviderConfig, StorageProviderConfig.Builder> {
    private StorageProviderConfig() { }
    private static readonly StorageProviderConfig defaultInstance = new StorageProviderConfig().MakeReadOnly();
    private static readonly string[] _storageProviderConfigFieldNames = new string[] { "eSENTProvider", "isMultiFileStore", "lMDBProvider", "maxDatabaseSize", "providerTypeString" };
    private static readonly uint[] _storageProviderConfigFieldTags = new uint[] { 42, 24, 34, 8, 18 };
    public static StorageProviderConfig DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override StorageProviderConfig DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override StorageProviderConfig ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.StorageProviderConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_StorageProviderConfig__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<StorageProviderConfig, StorageProviderConfig.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.StorageProviderConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_StorageProviderConfig__FieldAccessorTable; }
    }
    
    public const int MaxDatabaseSizeFieldNumber = 1;
    private bool hasMaxDatabaseSize;
    private long maxDatabaseSize_;
    public bool HasMaxDatabaseSize {
      get { return hasMaxDatabaseSize; }
    }
    public long MaxDatabaseSize {
      get { return maxDatabaseSize_; }
    }
    
    public const int ProviderTypeStringFieldNumber = 2;
    private bool hasProviderTypeString;
    private string providerTypeString_ = "";
    public bool HasProviderTypeString {
      get { return hasProviderTypeString; }
    }
    public string ProviderTypeString {
      get { return providerTypeString_; }
    }
    
    public const int IsMultiFileStoreFieldNumber = 3;
    private bool hasIsMultiFileStore;
    private bool isMultiFileStore_;
    public bool HasIsMultiFileStore {
      get { return hasIsMultiFileStore; }
    }
    public bool IsMultiFileStore {
      get { return isMultiFileStore_; }
    }
    
    public const int LMDBProviderFieldNumber = 4;
    private bool hasLMDBProvider;
    private global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig lMDBProvider_;
    public bool HasLMDBProvider {
      get { return hasLMDBProvider; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig LMDBProvider {
      get { return lMDBProvider_ ?? global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.DefaultInstance; }
    }
    
    public const int ESENTProviderFieldNumber = 5;
    private bool hasESENTProvider;
    private global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig eSENTProvider_;
    public bool HasESENTProvider {
      get { return hasESENTProvider; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig ESENTProvider {
      get { return eSENTProvider_ ?? global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _storageProviderConfigFieldNames;
      if (hasMaxDatabaseSize) {
        output.WriteInt64(1, field_names[3], MaxDatabaseSize);
      }
      if (hasProviderTypeString) {
        output.WriteString(2, field_names[4], ProviderTypeString);
      }
      if (hasIsMultiFileStore) {
        output.WriteBool(3, field_names[1], IsMultiFileStore);
      }
      if (hasLMDBProvider) {
        output.WriteMessage(4, field_names[2], LMDBProvider);
      }
      if (hasESENTProvider) {
        output.WriteMessage(5, field_names[0], ESENTProvider);
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
      if (hasMaxDatabaseSize) {
        size += pb::CodedOutputStream.ComputeInt64Size(1, MaxDatabaseSize);
      }
      if (hasProviderTypeString) {
        size += pb::CodedOutputStream.ComputeStringSize(2, ProviderTypeString);
      }
      if (hasIsMultiFileStore) {
        size += pb::CodedOutputStream.ComputeBoolSize(3, IsMultiFileStore);
      }
      if (hasLMDBProvider) {
        size += pb::CodedOutputStream.ComputeMessageSize(4, LMDBProvider);
      }
      if (hasESENTProvider) {
        size += pb::CodedOutputStream.ComputeMessageSize(5, ESENTProvider);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static StorageProviderConfig ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static StorageProviderConfig ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static StorageProviderConfig ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static StorageProviderConfig ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static StorageProviderConfig ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static StorageProviderConfig ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static StorageProviderConfig ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static StorageProviderConfig ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static StorageProviderConfig ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static StorageProviderConfig ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private StorageProviderConfig MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(StorageProviderConfig prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<StorageProviderConfig, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(StorageProviderConfig cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private StorageProviderConfig result;
      
      private StorageProviderConfig PrepareBuilder() {
        if (resultIsReadOnly) {
          StorageProviderConfig original = result;
          result = new StorageProviderConfig();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override StorageProviderConfig MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.Descriptor; }
      }
      
      public override StorageProviderConfig DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.DefaultInstance; }
      }
      
      public override StorageProviderConfig BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is StorageProviderConfig) {
          return MergeFrom((StorageProviderConfig) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(StorageProviderConfig other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.StorageProviderConfig.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasMaxDatabaseSize) {
          MaxDatabaseSize = other.MaxDatabaseSize;
        }
        if (other.HasProviderTypeString) {
          ProviderTypeString = other.ProviderTypeString;
        }
        if (other.HasIsMultiFileStore) {
          IsMultiFileStore = other.IsMultiFileStore;
        }
        if (other.HasLMDBProvider) {
          MergeLMDBProvider(other.LMDBProvider);
        }
        if (other.HasESENTProvider) {
          MergeESENTProvider(other.ESENTProvider);
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
            int field_ordinal = global::System.Array.BinarySearch(_storageProviderConfigFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _storageProviderConfigFieldTags[field_ordinal];
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
              result.hasMaxDatabaseSize = input.ReadInt64(ref result.maxDatabaseSize_);
              break;
            }
            case 18: {
              result.hasProviderTypeString = input.ReadString(ref result.providerTypeString_);
              break;
            }
            case 24: {
              result.hasIsMultiFileStore = input.ReadBool(ref result.isMultiFileStore_);
              break;
            }
            case 34: {
              global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.CreateBuilder();
              if (result.hasLMDBProvider) {
                subBuilder.MergeFrom(LMDBProvider);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              LMDBProvider = subBuilder.BuildPartial();
              break;
            }
            case 42: {
              global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig.CreateBuilder();
              if (result.hasESENTProvider) {
                subBuilder.MergeFrom(ESENTProvider);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              ESENTProvider = subBuilder.BuildPartial();
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasMaxDatabaseSize {
        get { return result.hasMaxDatabaseSize; }
      }
      public long MaxDatabaseSize {
        get { return result.MaxDatabaseSize; }
        set { SetMaxDatabaseSize(value); }
      }
      public Builder SetMaxDatabaseSize(long value) {
        PrepareBuilder();
        result.hasMaxDatabaseSize = true;
        result.maxDatabaseSize_ = value;
        return this;
      }
      public Builder ClearMaxDatabaseSize() {
        PrepareBuilder();
        result.hasMaxDatabaseSize = false;
        result.maxDatabaseSize_ = 0L;
        return this;
      }
      
      public bool HasProviderTypeString {
        get { return result.hasProviderTypeString; }
      }
      public string ProviderTypeString {
        get { return result.ProviderTypeString; }
        set { SetProviderTypeString(value); }
      }
      public Builder SetProviderTypeString(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasProviderTypeString = true;
        result.providerTypeString_ = value;
        return this;
      }
      public Builder ClearProviderTypeString() {
        PrepareBuilder();
        result.hasProviderTypeString = false;
        result.providerTypeString_ = "";
        return this;
      }
      
      public bool HasIsMultiFileStore {
        get { return result.hasIsMultiFileStore; }
      }
      public bool IsMultiFileStore {
        get { return result.IsMultiFileStore; }
        set { SetIsMultiFileStore(value); }
      }
      public Builder SetIsMultiFileStore(bool value) {
        PrepareBuilder();
        result.hasIsMultiFileStore = true;
        result.isMultiFileStore_ = value;
        return this;
      }
      public Builder ClearIsMultiFileStore() {
        PrepareBuilder();
        result.hasIsMultiFileStore = false;
        result.isMultiFileStore_ = false;
        return this;
      }
      
      public bool HasLMDBProvider {
       get { return result.hasLMDBProvider; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig LMDBProvider {
        get { return result.LMDBProvider; }
        set { SetLMDBProvider(value); }
      }
      public Builder SetLMDBProvider(global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasLMDBProvider = true;
        result.lMDBProvider_ = value;
        return this;
      }
      public Builder SetLMDBProvider(global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasLMDBProvider = true;
        result.lMDBProvider_ = builderForValue.Build();
        return this;
      }
      public Builder MergeLMDBProvider(global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasLMDBProvider &&
            result.lMDBProvider_ != global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.DefaultInstance) {
            result.lMDBProvider_ = global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.CreateBuilder(result.lMDBProvider_).MergeFrom(value).BuildPartial();
        } else {
          result.lMDBProvider_ = value;
        }
        result.hasLMDBProvider = true;
        return this;
      }
      public Builder ClearLMDBProvider() {
        PrepareBuilder();
        result.hasLMDBProvider = false;
        result.lMDBProvider_ = null;
        return this;
      }
      
      public bool HasESENTProvider {
       get { return result.hasESENTProvider; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig ESENTProvider {
        get { return result.ESENTProvider; }
        set { SetESENTProvider(value); }
      }
      public Builder SetESENTProvider(global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasESENTProvider = true;
        result.eSENTProvider_ = value;
        return this;
      }
      public Builder SetESENTProvider(global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasESENTProvider = true;
        result.eSENTProvider_ = builderForValue.Build();
        return this;
      }
      public Builder MergeESENTProvider(global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasESENTProvider &&
            result.eSENTProvider_ != global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig.DefaultInstance) {
            result.eSENTProvider_ = global::Alachisoft.NosDB.Common.Protobuf.ESENTConfig.CreateBuilder(result.eSENTProvider_).MergeFrom(value).BuildPartial();
        } else {
          result.eSENTProvider_ = value;
        }
        result.hasESENTProvider = true;
        return this;
      }
      public Builder ClearESENTProvider() {
        PrepareBuilder();
        result.hasESENTProvider = false;
        result.eSENTProvider_ = null;
        return this;
      }
    }
    static StorageProviderConfig() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.StorageProviderConfig.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
