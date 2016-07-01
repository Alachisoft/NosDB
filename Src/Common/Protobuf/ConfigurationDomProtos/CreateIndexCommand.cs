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
    public static partial class CreateIndexCommand {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_CreateIndexCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand, global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_CreateIndexCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static CreateIndexCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChhDcmVhdGVJbmRleENvbW1hbmQucHJvdG8SIEFsYWNoaXNvZnQuTm9zREIu", 
              "Q29tbW9uLlByb3RvYnVmGhlJbmRleEF0dHJpYnV0ZVByb3RvLnByb3RvIp8B", 
              "ChJDcmVhdGVJbmRleENvbW1hbmQSEQoJaW5kZXhOYW1lGAEgASgJEkkKCmF0", 
              "dHJpYnV0ZXMYAiABKAsyNS5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Qcm90", 
              "b2J1Zi5JbmRleEF0dHJpYnV0ZVByb3RvEhMKC2NhY2hlUG9saWN5GAMgASgJ", 
              "EhYKDmpvdXJuYWxFbmFibGVkGAQgASgIQkIKJGNvbS5hbGFjaGlzb2Z0Lm5v", 
              "c2RiLmNvbW1vbi5wcm90b2J1ZkIaQ3JlYXRlSW5kZXhDb21tYW5kUHJvdG9j", 
            "b2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_CreateIndexCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_CreateIndexCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand, global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_CreateIndexCommand__Descriptor,
                  new string[] { "IndexName", "Attributes", "CachePolicy", "JournalEnabled", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.IndexAttributeProto.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class CreateIndexCommand : pb::GeneratedMessage<CreateIndexCommand, CreateIndexCommand.Builder> {
    private CreateIndexCommand() { }
    private static readonly CreateIndexCommand defaultInstance = new CreateIndexCommand().MakeReadOnly();
    private static readonly string[] _createIndexCommandFieldNames = new string[] { "attributes", "cachePolicy", "indexName", "journalEnabled" };
    private static readonly uint[] _createIndexCommandFieldTags = new uint[] { 18, 26, 10, 32 };
    public static CreateIndexCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override CreateIndexCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override CreateIndexCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateIndexCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_CreateIndexCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<CreateIndexCommand, CreateIndexCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateIndexCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_CreateIndexCommand__FieldAccessorTable; }
    }
    
    public const int IndexNameFieldNumber = 1;
    private bool hasIndexName;
    private string indexName_ = "";
    public bool HasIndexName {
      get { return hasIndexName; }
    }
    public string IndexName {
      get { return indexName_; }
    }
    
    public const int AttributesFieldNumber = 2;
    private bool hasAttributes;
    private global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto attributes_;
    public bool HasAttributes {
      get { return hasAttributes; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto Attributes {
      get { return attributes_ ?? global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.DefaultInstance; }
    }
    
    public const int CachePolicyFieldNumber = 3;
    private bool hasCachePolicy;
    private string cachePolicy_ = "";
    public bool HasCachePolicy {
      get { return hasCachePolicy; }
    }
    public string CachePolicy {
      get { return cachePolicy_; }
    }
    
    public const int JournalEnabledFieldNumber = 4;
    private bool hasJournalEnabled;
    private bool journalEnabled_;
    public bool HasJournalEnabled {
      get { return hasJournalEnabled; }
    }
    public bool JournalEnabled {
      get { return journalEnabled_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _createIndexCommandFieldNames;
      if (hasIndexName) {
        output.WriteString(1, field_names[2], IndexName);
      }
      if (hasAttributes) {
        output.WriteMessage(2, field_names[0], Attributes);
      }
      if (hasCachePolicy) {
        output.WriteString(3, field_names[1], CachePolicy);
      }
      if (hasJournalEnabled) {
        output.WriteBool(4, field_names[3], JournalEnabled);
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
      if (hasIndexName) {
        size += pb::CodedOutputStream.ComputeStringSize(1, IndexName);
      }
      if (hasAttributes) {
        size += pb::CodedOutputStream.ComputeMessageSize(2, Attributes);
      }
      if (hasCachePolicy) {
        size += pb::CodedOutputStream.ComputeStringSize(3, CachePolicy);
      }
      if (hasJournalEnabled) {
        size += pb::CodedOutputStream.ComputeBoolSize(4, JournalEnabled);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static CreateIndexCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CreateIndexCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CreateIndexCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CreateIndexCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CreateIndexCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CreateIndexCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static CreateIndexCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static CreateIndexCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static CreateIndexCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CreateIndexCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private CreateIndexCommand MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(CreateIndexCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<CreateIndexCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(CreateIndexCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private CreateIndexCommand result;
      
      private CreateIndexCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          CreateIndexCommand original = result;
          result = new CreateIndexCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override CreateIndexCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Descriptor; }
      }
      
      public override CreateIndexCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.DefaultInstance; }
      }
      
      public override CreateIndexCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is CreateIndexCommand) {
          return MergeFrom((CreateIndexCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(CreateIndexCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasIndexName) {
          IndexName = other.IndexName;
        }
        if (other.HasAttributes) {
          MergeAttributes(other.Attributes);
        }
        if (other.HasCachePolicy) {
          CachePolicy = other.CachePolicy;
        }
        if (other.HasJournalEnabled) {
          JournalEnabled = other.JournalEnabled;
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
            int field_ordinal = global::System.Array.BinarySearch(_createIndexCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _createIndexCommandFieldTags[field_ordinal];
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
              result.hasIndexName = input.ReadString(ref result.indexName_);
              break;
            }
            case 18: {
              global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.CreateBuilder();
              if (result.hasAttributes) {
                subBuilder.MergeFrom(Attributes);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              Attributes = subBuilder.BuildPartial();
              break;
            }
            case 26: {
              result.hasCachePolicy = input.ReadString(ref result.cachePolicy_);
              break;
            }
            case 32: {
              result.hasJournalEnabled = input.ReadBool(ref result.journalEnabled_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasIndexName {
        get { return result.hasIndexName; }
      }
      public string IndexName {
        get { return result.IndexName; }
        set { SetIndexName(value); }
      }
      public Builder SetIndexName(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasIndexName = true;
        result.indexName_ = value;
        return this;
      }
      public Builder ClearIndexName() {
        PrepareBuilder();
        result.hasIndexName = false;
        result.indexName_ = "";
        return this;
      }
      
      public bool HasAttributes {
       get { return result.hasAttributes; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto Attributes {
        get { return result.Attributes; }
        set { SetAttributes(value); }
      }
      public Builder SetAttributes(global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasAttributes = true;
        result.attributes_ = value;
        return this;
      }
      public Builder SetAttributes(global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasAttributes = true;
        result.attributes_ = builderForValue.Build();
        return this;
      }
      public Builder MergeAttributes(global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasAttributes &&
            result.attributes_ != global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.DefaultInstance) {
            result.attributes_ = global::Alachisoft.NosDB.Common.Protobuf.IndexAttributeProto.CreateBuilder(result.attributes_).MergeFrom(value).BuildPartial();
        } else {
          result.attributes_ = value;
        }
        result.hasAttributes = true;
        return this;
      }
      public Builder ClearAttributes() {
        PrepareBuilder();
        result.hasAttributes = false;
        result.attributes_ = null;
        return this;
      }
      
      public bool HasCachePolicy {
        get { return result.hasCachePolicy; }
      }
      public string CachePolicy {
        get { return result.CachePolicy; }
        set { SetCachePolicy(value); }
      }
      public Builder SetCachePolicy(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCachePolicy = true;
        result.cachePolicy_ = value;
        return this;
      }
      public Builder ClearCachePolicy() {
        PrepareBuilder();
        result.hasCachePolicy = false;
        result.cachePolicy_ = "";
        return this;
      }
      
      public bool HasJournalEnabled {
        get { return result.hasJournalEnabled; }
      }
      public bool JournalEnabled {
        get { return result.JournalEnabled; }
        set { SetJournalEnabled(value); }
      }
      public Builder SetJournalEnabled(bool value) {
        PrepareBuilder();
        result.hasJournalEnabled = true;
        result.journalEnabled_ = value;
        return this;
      }
      public Builder ClearJournalEnabled() {
        PrepareBuilder();
        result.hasJournalEnabled = false;
        result.journalEnabled_ = false;
        return this;
      }
    }
    static CreateIndexCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateIndexCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
