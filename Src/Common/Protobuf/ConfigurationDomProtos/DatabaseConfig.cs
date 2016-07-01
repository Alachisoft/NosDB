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
    public static partial class DatabaseConfig {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_DatabaseConfig__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DatabaseConfig, global::Alachisoft.NosDB.Common.Protobuf.DatabaseConfig.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_DatabaseConfig__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static DatabaseConfig() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChREYXRhYmFzZUNvbmZpZy5wcm90bxIgQWxhY2hpc29mdC5Ob3NEQi5Db21t", 
              "b24uUHJvdG9idWYaE1N0b3JhZ2VDb25maWcucHJvdG8iYAoORGF0YWJhc2VD", 
              "b25maWcSDAoEbmFtZRgBIAEoCRJACgdzdG9yYWdlGAIgASgLMi8uQWxhY2hp", 
              "c29mdC5Ob3NEQi5Db21tb24uUHJvdG9idWYuU3RvcmFnZUNvbmZpZ0I+CiRj", 
              "b20uYWxhY2hpc29mdC5ub3NkYi5jb21tb24ucHJvdG9idWZCFkRhdGFiYXNl", 
            "Q29uZmlnUHJvdG9jb2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_DatabaseConfig__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_DatabaseConfig__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DatabaseConfig, global::Alachisoft.NosDB.Common.Protobuf.DatabaseConfig.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_DatabaseConfig__Descriptor,
                  new string[] { "Name", "Storage", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.StorageConfig.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class DatabaseConfig : pb::GeneratedMessage<DatabaseConfig, DatabaseConfig.Builder> {
    private DatabaseConfig() { }
    private static readonly DatabaseConfig defaultInstance = new DatabaseConfig().MakeReadOnly();
    private static readonly string[] _databaseConfigFieldNames = new string[] { "name", "storage" };
    private static readonly uint[] _databaseConfigFieldTags = new uint[] { 10, 18 };
    public static DatabaseConfig DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override DatabaseConfig DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override DatabaseConfig ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DatabaseConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_DatabaseConfig__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<DatabaseConfig, DatabaseConfig.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DatabaseConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_DatabaseConfig__FieldAccessorTable; }
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
    
    public const int StorageFieldNumber = 2;
    private bool hasStorage;
    private global::Alachisoft.NosDB.Common.Protobuf.StorageConfig storage_;
    public bool HasStorage {
      get { return hasStorage; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.StorageConfig Storage {
      get { return storage_ ?? global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _databaseConfigFieldNames;
      if (hasName) {
        output.WriteString(1, field_names[0], Name);
      }
      if (hasStorage) {
        output.WriteMessage(2, field_names[1], Storage);
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
      if (hasStorage) {
        size += pb::CodedOutputStream.ComputeMessageSize(2, Storage);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static DatabaseConfig ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DatabaseConfig ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DatabaseConfig ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DatabaseConfig ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DatabaseConfig ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DatabaseConfig ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static DatabaseConfig ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static DatabaseConfig ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static DatabaseConfig ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DatabaseConfig ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private DatabaseConfig MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(DatabaseConfig prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<DatabaseConfig, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(DatabaseConfig cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private DatabaseConfig result;
      
      private DatabaseConfig PrepareBuilder() {
        if (resultIsReadOnly) {
          DatabaseConfig original = result;
          result = new DatabaseConfig();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override DatabaseConfig MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.DatabaseConfig.Descriptor; }
      }
      
      public override DatabaseConfig DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.DatabaseConfig.DefaultInstance; }
      }
      
      public override DatabaseConfig BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is DatabaseConfig) {
          return MergeFrom((DatabaseConfig) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(DatabaseConfig other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.DatabaseConfig.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasName) {
          Name = other.Name;
        }
        if (other.HasStorage) {
          MergeStorage(other.Storage);
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
            int field_ordinal = global::System.Array.BinarySearch(_databaseConfigFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _databaseConfigFieldTags[field_ordinal];
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
              global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.CreateBuilder();
              if (result.hasStorage) {
                subBuilder.MergeFrom(Storage);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              Storage = subBuilder.BuildPartial();
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
      
      public bool HasStorage {
       get { return result.hasStorage; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.StorageConfig Storage {
        get { return result.Storage; }
        set { SetStorage(value); }
      }
      public Builder SetStorage(global::Alachisoft.NosDB.Common.Protobuf.StorageConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasStorage = true;
        result.storage_ = value;
        return this;
      }
      public Builder SetStorage(global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasStorage = true;
        result.storage_ = builderForValue.Build();
        return this;
      }
      public Builder MergeStorage(global::Alachisoft.NosDB.Common.Protobuf.StorageConfig value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasStorage &&
            result.storage_ != global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.DefaultInstance) {
            result.storage_ = global::Alachisoft.NosDB.Common.Protobuf.StorageConfig.CreateBuilder(result.storage_).MergeFrom(value).BuildPartial();
        } else {
          result.storage_ = value;
        }
        result.hasStorage = true;
        return this;
      }
      public Builder ClearStorage() {
        PrepareBuilder();
        result.hasStorage = false;
        result.storage_ = null;
        return this;
      }
    }
    static DatabaseConfig() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.DatabaseConfig.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
