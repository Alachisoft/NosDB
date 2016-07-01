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
    public static partial class CachingConfig {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_CachingConfig__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CachingConfig, global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_CachingConfig__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static CachingConfig() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChNDYWNoaW5nQ29uZmlnLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNvbW1v", 
              "bi5Qcm90b2J1ZiIPCg1DYWNoaW5nQ29uZmlnQj0KJGNvbS5hbGFjaGlzb2Z0", 
            "Lm5vc2RiLmNvbW1vbi5wcm90b2J1ZkIVQ2FjaGluZ0NvbmZpZ1Byb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_CachingConfig__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_CachingConfig__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CachingConfig, global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_CachingConfig__Descriptor,
                  new string[] { });
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
  public sealed partial class CachingConfig : pb::GeneratedMessage<CachingConfig, CachingConfig.Builder> {
    private CachingConfig() { }
    private static readonly CachingConfig defaultInstance = new CachingConfig().MakeReadOnly();
    private static readonly string[] _cachingConfigFieldNames = new string[] {  };
    private static readonly uint[] _cachingConfigFieldTags = new uint[] {  };
    public static CachingConfig DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override CachingConfig DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override CachingConfig ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CachingConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_CachingConfig__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<CachingConfig, CachingConfig.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CachingConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_CachingConfig__FieldAccessorTable; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _cachingConfigFieldNames;
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
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static CachingConfig ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CachingConfig ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CachingConfig ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CachingConfig ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CachingConfig ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CachingConfig ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static CachingConfig ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static CachingConfig ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static CachingConfig ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CachingConfig ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private CachingConfig MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(CachingConfig prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<CachingConfig, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(CachingConfig cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private CachingConfig result;
      
      private CachingConfig PrepareBuilder() {
        if (resultIsReadOnly) {
          CachingConfig original = result;
          result = new CachingConfig();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override CachingConfig MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.Descriptor; }
      }
      
      public override CachingConfig DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.DefaultInstance; }
      }
      
      public override CachingConfig BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is CachingConfig) {
          return MergeFrom((CachingConfig) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(CachingConfig other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.CachingConfig.DefaultInstance) return this;
        PrepareBuilder();
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
            int field_ordinal = global::System.Array.BinarySearch(_cachingConfigFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _cachingConfigFieldTags[field_ordinal];
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
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
    }
    static CachingConfig() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.CachingConfig.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
