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
    public static partial class LMDBConfig {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_LMDBConfig__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig, global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_LMDBConfig__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static LMDBConfig() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChBMTURCQ29uZmlnLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Q", 
              "cm90b2J1ZiLWAwoKTE1EQkNvbmZpZxJfChRlbnZpcm9ubWVudE9wZW5GbGFn", 
              "cxgBIAEoDjJBLkFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmLkxN", 
              "REJDb25maWcuRW52aXJvbm1lbnRPcGVuRmxhZ3MSEgoKbWF4UmVhZGVycxgC", 
              "IAEoBRIRCgltYXhUYWJsZXMYAyABKAUSGwoTaW5pdGlhbERhdGFiYXNlU2l6", 
              "ZRgEIAEoAxIWCg5leHBhbnNpb25SYXRpbxgFIAEoAxIbChNjdXJyZW50RGF0", 
              "YWJhc2VTaXplGAYgASgDIu0BChRFbnZpcm9ubWVudE9wZW5GbGFncxIICgRO", 
              "b25lEAASDAoIRml4ZWRNYXAQARIOCghOb1N1YkRpchCAgAESDAoGTm9TeW5j", 
              "EICABBIOCghSZWFkT25seRCAgAgSEAoKTm9NZXRhU3luYxCAgBASDgoIV3Jp", 
              "dGVNYXAQgIAgEg4KCE1hcEFzeW5jEICAQBIbChROb1RocmVhZExvY2FsU3Rv", 
              "cmFnZRCAgIABEg0KBk5vTG9jaxCAgIACEhIKC05vUmVhZEFoZWFkEICAgAQS", 
              "HQoWTm9NZW1vcnlJbml0aWFsaXphdGlvbhCAgIAIQjoKJGNvbS5hbGFjaGlz", 
            "b2Z0Lm5vc2RiLmNvbW1vbi5wcm90b2J1ZkISTE1EQkNvbmZpZ1Byb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_LMDBConfig__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_LMDBConfig__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig, global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_LMDBConfig__Descriptor,
                  new string[] { "EnvironmentOpenFlags", "MaxReaders", "MaxTables", "InitialDatabaseSize", "ExpansionRatio", "CurrentDatabaseSize", });
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
  public sealed partial class LMDBConfig : pb::GeneratedMessage<LMDBConfig, LMDBConfig.Builder> {
    private LMDBConfig() { }
    private static readonly LMDBConfig defaultInstance = new LMDBConfig().MakeReadOnly();
    private static readonly string[] _lMDBConfigFieldNames = new string[] { "currentDatabaseSize", "environmentOpenFlags", "expansionRatio", "initialDatabaseSize", "maxReaders", "maxTables" };
    private static readonly uint[] _lMDBConfigFieldTags = new uint[] { 48, 8, 40, 32, 16, 24 };
    public static LMDBConfig DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override LMDBConfig DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override LMDBConfig ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.LMDBConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_LMDBConfig__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<LMDBConfig, LMDBConfig.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.LMDBConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_LMDBConfig__FieldAccessorTable; }
    }
    
    #region Nested types
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public static partial class Types {
      public enum EnvironmentOpenFlags {
        None = 0,
        FixedMap = 1,
        NoSubDir = 16384,
        NoSync = 65536,
        ReadOnly = 131072,
        NoMetaSync = 262144,
        WriteMap = 524288,
        MapAsync = 1048576,
        NoThreadLocalStorage = 2097152,
        NoLock = 4194304,
        NoReadAhead = 8388608,
        NoMemoryInitialization = 16777216,
      }
      
    }
    #endregion
    
    public const int EnvironmentOpenFlagsFieldNumber = 1;
    private bool hasEnvironmentOpenFlags;
    private global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Types.EnvironmentOpenFlags environmentOpenFlags_ = global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Types.EnvironmentOpenFlags.None;
    public bool HasEnvironmentOpenFlags {
      get { return hasEnvironmentOpenFlags; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Types.EnvironmentOpenFlags EnvironmentOpenFlags {
      get { return environmentOpenFlags_; }
    }
    
    public const int MaxReadersFieldNumber = 2;
    private bool hasMaxReaders;
    private int maxReaders_;
    public bool HasMaxReaders {
      get { return hasMaxReaders; }
    }
    public int MaxReaders {
      get { return maxReaders_; }
    }
    
    public const int MaxTablesFieldNumber = 3;
    private bool hasMaxTables;
    private int maxTables_;
    public bool HasMaxTables {
      get { return hasMaxTables; }
    }
    public int MaxTables {
      get { return maxTables_; }
    }
    
    public const int InitialDatabaseSizeFieldNumber = 4;
    private bool hasInitialDatabaseSize;
    private long initialDatabaseSize_;
    public bool HasInitialDatabaseSize {
      get { return hasInitialDatabaseSize; }
    }
    public long InitialDatabaseSize {
      get { return initialDatabaseSize_; }
    }
    
    public const int ExpansionRatioFieldNumber = 5;
    private bool hasExpansionRatio;
    private long expansionRatio_;
    public bool HasExpansionRatio {
      get { return hasExpansionRatio; }
    }
    public long ExpansionRatio {
      get { return expansionRatio_; }
    }
    
    public const int CurrentDatabaseSizeFieldNumber = 6;
    private bool hasCurrentDatabaseSize;
    private long currentDatabaseSize_;
    public bool HasCurrentDatabaseSize {
      get { return hasCurrentDatabaseSize; }
    }
    public long CurrentDatabaseSize {
      get { return currentDatabaseSize_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _lMDBConfigFieldNames;
      if (hasEnvironmentOpenFlags) {
        output.WriteEnum(1, field_names[1], (int) EnvironmentOpenFlags, EnvironmentOpenFlags);
      }
      if (hasMaxReaders) {
        output.WriteInt32(2, field_names[4], MaxReaders);
      }
      if (hasMaxTables) {
        output.WriteInt32(3, field_names[5], MaxTables);
      }
      if (hasInitialDatabaseSize) {
        output.WriteInt64(4, field_names[3], InitialDatabaseSize);
      }
      if (hasExpansionRatio) {
        output.WriteInt64(5, field_names[2], ExpansionRatio);
      }
      if (hasCurrentDatabaseSize) {
        output.WriteInt64(6, field_names[0], CurrentDatabaseSize);
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
      if (hasEnvironmentOpenFlags) {
        size += pb::CodedOutputStream.ComputeEnumSize(1, (int) EnvironmentOpenFlags);
      }
      if (hasMaxReaders) {
        size += pb::CodedOutputStream.ComputeInt32Size(2, MaxReaders);
      }
      if (hasMaxTables) {
        size += pb::CodedOutputStream.ComputeInt32Size(3, MaxTables);
      }
      if (hasInitialDatabaseSize) {
        size += pb::CodedOutputStream.ComputeInt64Size(4, InitialDatabaseSize);
      }
      if (hasExpansionRatio) {
        size += pb::CodedOutputStream.ComputeInt64Size(5, ExpansionRatio);
      }
      if (hasCurrentDatabaseSize) {
        size += pb::CodedOutputStream.ComputeInt64Size(6, CurrentDatabaseSize);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static LMDBConfig ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static LMDBConfig ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static LMDBConfig ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static LMDBConfig ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static LMDBConfig ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static LMDBConfig ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static LMDBConfig ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static LMDBConfig ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static LMDBConfig ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static LMDBConfig ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private LMDBConfig MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(LMDBConfig prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<LMDBConfig, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(LMDBConfig cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private LMDBConfig result;
      
      private LMDBConfig PrepareBuilder() {
        if (resultIsReadOnly) {
          LMDBConfig original = result;
          result = new LMDBConfig();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override LMDBConfig MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Descriptor; }
      }
      
      public override LMDBConfig DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.DefaultInstance; }
      }
      
      public override LMDBConfig BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is LMDBConfig) {
          return MergeFrom((LMDBConfig) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(LMDBConfig other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasEnvironmentOpenFlags) {
          EnvironmentOpenFlags = other.EnvironmentOpenFlags;
        }
        if (other.HasMaxReaders) {
          MaxReaders = other.MaxReaders;
        }
        if (other.HasMaxTables) {
          MaxTables = other.MaxTables;
        }
        if (other.HasInitialDatabaseSize) {
          InitialDatabaseSize = other.InitialDatabaseSize;
        }
        if (other.HasExpansionRatio) {
          ExpansionRatio = other.ExpansionRatio;
        }
        if (other.HasCurrentDatabaseSize) {
          CurrentDatabaseSize = other.CurrentDatabaseSize;
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
            int field_ordinal = global::System.Array.BinarySearch(_lMDBConfigFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _lMDBConfigFieldTags[field_ordinal];
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
              object unknown;
              if(input.ReadEnum(ref result.environmentOpenFlags_, out unknown)) {
                result.hasEnvironmentOpenFlags = true;
              } else if(unknown is int) {
                if (unknownFields == null) {
                  unknownFields = pb::UnknownFieldSet.CreateBuilder(this.UnknownFields);
                }
                unknownFields.MergeVarintField(1, (ulong)(int)unknown);
              }
              break;
            }
            case 16: {
              result.hasMaxReaders = input.ReadInt32(ref result.maxReaders_);
              break;
            }
            case 24: {
              result.hasMaxTables = input.ReadInt32(ref result.maxTables_);
              break;
            }
            case 32: {
              result.hasInitialDatabaseSize = input.ReadInt64(ref result.initialDatabaseSize_);
              break;
            }
            case 40: {
              result.hasExpansionRatio = input.ReadInt64(ref result.expansionRatio_);
              break;
            }
            case 48: {
              result.hasCurrentDatabaseSize = input.ReadInt64(ref result.currentDatabaseSize_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasEnvironmentOpenFlags {
       get { return result.hasEnvironmentOpenFlags; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Types.EnvironmentOpenFlags EnvironmentOpenFlags {
        get { return result.EnvironmentOpenFlags; }
        set { SetEnvironmentOpenFlags(value); }
      }
      public Builder SetEnvironmentOpenFlags(global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Types.EnvironmentOpenFlags value) {
        PrepareBuilder();
        result.hasEnvironmentOpenFlags = true;
        result.environmentOpenFlags_ = value;
        return this;
      }
      public Builder ClearEnvironmentOpenFlags() {
        PrepareBuilder();
        result.hasEnvironmentOpenFlags = false;
        result.environmentOpenFlags_ = global::Alachisoft.NosDB.Common.Protobuf.LMDBConfig.Types.EnvironmentOpenFlags.None;
        return this;
      }
      
      public bool HasMaxReaders {
        get { return result.hasMaxReaders; }
      }
      public int MaxReaders {
        get { return result.MaxReaders; }
        set { SetMaxReaders(value); }
      }
      public Builder SetMaxReaders(int value) {
        PrepareBuilder();
        result.hasMaxReaders = true;
        result.maxReaders_ = value;
        return this;
      }
      public Builder ClearMaxReaders() {
        PrepareBuilder();
        result.hasMaxReaders = false;
        result.maxReaders_ = 0;
        return this;
      }
      
      public bool HasMaxTables {
        get { return result.hasMaxTables; }
      }
      public int MaxTables {
        get { return result.MaxTables; }
        set { SetMaxTables(value); }
      }
      public Builder SetMaxTables(int value) {
        PrepareBuilder();
        result.hasMaxTables = true;
        result.maxTables_ = value;
        return this;
      }
      public Builder ClearMaxTables() {
        PrepareBuilder();
        result.hasMaxTables = false;
        result.maxTables_ = 0;
        return this;
      }
      
      public bool HasInitialDatabaseSize {
        get { return result.hasInitialDatabaseSize; }
      }
      public long InitialDatabaseSize {
        get { return result.InitialDatabaseSize; }
        set { SetInitialDatabaseSize(value); }
      }
      public Builder SetInitialDatabaseSize(long value) {
        PrepareBuilder();
        result.hasInitialDatabaseSize = true;
        result.initialDatabaseSize_ = value;
        return this;
      }
      public Builder ClearInitialDatabaseSize() {
        PrepareBuilder();
        result.hasInitialDatabaseSize = false;
        result.initialDatabaseSize_ = 0L;
        return this;
      }
      
      public bool HasExpansionRatio {
        get { return result.hasExpansionRatio; }
      }
      public long ExpansionRatio {
        get { return result.ExpansionRatio; }
        set { SetExpansionRatio(value); }
      }
      public Builder SetExpansionRatio(long value) {
        PrepareBuilder();
        result.hasExpansionRatio = true;
        result.expansionRatio_ = value;
        return this;
      }
      public Builder ClearExpansionRatio() {
        PrepareBuilder();
        result.hasExpansionRatio = false;
        result.expansionRatio_ = 0L;
        return this;
      }
      
      public bool HasCurrentDatabaseSize {
        get { return result.hasCurrentDatabaseSize; }
      }
      public long CurrentDatabaseSize {
        get { return result.CurrentDatabaseSize; }
        set { SetCurrentDatabaseSize(value); }
      }
      public Builder SetCurrentDatabaseSize(long value) {
        PrepareBuilder();
        result.hasCurrentDatabaseSize = true;
        result.currentDatabaseSize_ = value;
        return this;
      }
      public Builder ClearCurrentDatabaseSize() {
        PrepareBuilder();
        result.hasCurrentDatabaseSize = false;
        result.currentDatabaseSize_ = 0L;
        return this;
      }
    }
    static LMDBConfig() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.LMDBConfig.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
