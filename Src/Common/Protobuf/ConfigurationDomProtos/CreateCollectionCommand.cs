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
    public static partial class CreateCollectionCommand {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_CreateCollectionCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand, global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_CreateCollectionCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static CreateCollectionCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "Ch1DcmVhdGVDb2xsZWN0aW9uQ29tbWFuZC5wcm90bxIgQWxhY2hpc29mdC5O", 
              "b3NEQi5Db21tb24uUHJvdG9idWYaE0luZGljZXNDb25maWcucHJvdG8aE0Nh", 
              "Y2hpbmdDb25maWcucHJvdG8iuwEKF0NyZWF0ZUNvbGxlY3Rpb25Db21tYW5k", 
              "EhYKDmNvbGxlY3Rpb25OYW1lGAEgASgJEkYKDWluZGljZXNDb25maWcYAiAB", 
              "KAsyLy5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90b2J1Zi5JbmRpY2Vz", 
              "Q29uZmlnEkAKB2NhY2hpbmcYAyABKAsyLy5BbGFjaGlzb2Z0Lk5vc0RCLkNv", 
              "bW1vbi5Qcm90b2J1Zi5DYWNoaW5nQ29uZmlnQkcKJGNvbS5hbGFjaGlzb2Z0", 
              "Lm5vc2RiLmNvbW1vbi5wcm90b2J1ZkIfQ3JlYXRlQ29sbGVjdGlvbkNvbW1h", 
            "bmRQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_CreateCollectionCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_CreateCollectionCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand, global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_CreateCollectionCommand__Descriptor,
                  new string[] { "CollectionName", "IndicesConfig", "Caching", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.IndicesConfig.Descriptor, 
            global::Alachisoft.NosDB.Common.Protobuf.Proto.CachingConfig.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class CreateCollectionCommand : pb::GeneratedMessage<CreateCollectionCommand, CreateCollectionCommand.Builder> {
    private CreateCollectionCommand() { }
    private static readonly CreateCollectionCommand defaultInstance = new CreateCollectionCommand().MakeReadOnly();
    private static readonly string[] _createCollectionCommandFieldNames = new string[] { "caching", "collectionName", "indicesConfig" };
    private static readonly uint[] _createCollectionCommandFieldTags = new uint[] { 26, 10, 18 };
    public static CreateCollectionCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override CreateCollectionCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override CreateCollectionCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateCollectionCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_CreateCollectionCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<CreateCollectionCommand, CreateCollectionCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateCollectionCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_CreateCollectionCommand__FieldAccessorTable; }
    }
    
    public const int CollectionNameFieldNumber = 1;
    private bool hasCollectionName;
    private string collectionName_ = "";
    public bool HasCollectionName {
      get { return hasCollectionName; }
    }
    public string CollectionName {
      get { return collectionName_; }
    }
    
    public const int IndicesConfigFieldNumber = 2;
    private bool hasIndicesConfig;
    private global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig indicesConfig_;
    public bool HasIndicesConfig {
      get { return hasIndicesConfig; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig IndicesConfig {
      get { return indicesConfig_ ?? global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.DefaultInstance; }
    }
    
    public const int CachingFieldNumber = 3;
    private bool hasCaching;
    private global::Alachisoft.NosDB.Common.Protobuf.CachingConfig caching_;
    public bool HasCaching {
      get { return hasCaching; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.CachingConfig Caching {
      get { return caching_ ?? global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _createCollectionCommandFieldNames;
      if (hasCollectionName) {
        output.WriteString(1, field_names[1], CollectionName);
      }
      if (hasIndicesConfig) {
        output.WriteMessage(2, field_names[2], IndicesConfig);
      }
      if (hasCaching) {
        output.WriteMessage(3, field_names[0], Caching);
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
      if (hasCollectionName) {
        size += pb::CodedOutputStream.ComputeStringSize(1, CollectionName);
      }
      if (hasIndicesConfig) {
        size += pb::CodedOutputStream.ComputeMessageSize(2, IndicesConfig);
      }
      if (hasCaching) {
        size += pb::CodedOutputStream.ComputeMessageSize(3, Caching);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static CreateCollectionCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CreateCollectionCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CreateCollectionCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CreateCollectionCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CreateCollectionCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CreateCollectionCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static CreateCollectionCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static CreateCollectionCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static CreateCollectionCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CreateCollectionCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private CreateCollectionCommand MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(CreateCollectionCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<CreateCollectionCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(CreateCollectionCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private CreateCollectionCommand result;
      
      private CreateCollectionCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          CreateCollectionCommand original = result;
          result = new CreateCollectionCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override CreateCollectionCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Descriptor; }
      }
      
      public override CreateCollectionCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.DefaultInstance; }
      }
      
      public override CreateCollectionCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is CreateCollectionCommand) {
          return MergeFrom((CreateCollectionCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(CreateCollectionCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasCollectionName) {
          CollectionName = other.CollectionName;
        }
        if (other.HasIndicesConfig) {
          MergeIndicesConfig(other.IndicesConfig);
        }
        if (other.HasCaching) {
          MergeCaching(other.Caching);
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
            int field_ordinal = global::System.Array.BinarySearch(_createCollectionCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _createCollectionCommandFieldTags[field_ordinal];
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
              result.hasCollectionName = input.ReadString(ref result.collectionName_);
              break;
            }
            case 18: {
              global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.CreateBuilder();
              if (result.hasIndicesConfig) {
                subBuilder.MergeFrom(IndicesConfig);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              IndicesConfig = subBuilder.BuildPartial();
              break;
            }
            case 26: {
              global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.CreateBuilder();
              if (result.hasCaching) {
                subBuilder.MergeFrom(Caching);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              Caching = subBuilder.BuildPartial();
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
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
      
      public bool HasIndicesConfig {
       get { return result.hasIndicesConfig; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig IndicesConfig {
        get { return result.IndicesConfig; }
        set { SetIndicesConfig(value); }
      }
      public Builder SetIndicesConfig(global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasIndicesConfig = true;
        result.indicesConfig_ = value;
        return this;
      }
      public Builder SetIndicesConfig(global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasIndicesConfig = true;
        result.indicesConfig_ = builderForValue.Build();
        return this;
      }
      public Builder MergeIndicesConfig(global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasIndicesConfig &&
            result.indicesConfig_ != global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.DefaultInstance) {
            result.indicesConfig_ = global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.CreateBuilder(result.indicesConfig_).MergeFrom(value).BuildPartial();
        } else {
          result.indicesConfig_ = value;
        }
        result.hasIndicesConfig = true;
        return this;
      }
      public Builder ClearIndicesConfig() {
        PrepareBuilder();
        result.hasIndicesConfig = false;
        result.indicesConfig_ = null;
        return this;
      }
      
      public bool HasCaching {
       get { return result.hasCaching; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.CachingConfig Caching {
        get { return result.Caching; }
        set { SetCaching(value); }
      }
      public Builder SetCaching(global::Alachisoft.NosDB.Common.Protobuf.CachingConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCaching = true;
        result.caching_ = value;
        return this;
      }
      public Builder SetCaching(global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasCaching = true;
        result.caching_ = builderForValue.Build();
        return this;
      }
      public Builder MergeCaching(global::Alachisoft.NosDB.Common.Protobuf.CachingConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasCaching &&
            result.caching_ != global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.DefaultInstance) {
            result.caching_ = global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.CreateBuilder(result.caching_).MergeFrom(value).BuildPartial();
        } else {
          result.caching_ = value;
        }
        result.hasCaching = true;
        return this;
      }
      public Builder ClearCaching() {
        PrepareBuilder();
        result.hasCaching = false;
        result.caching_ = null;
        return this;
      }
    }
    static CreateCollectionCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateCollectionCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
