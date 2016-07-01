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
    public static partial class IndicesConfig {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_IndicesConfig__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig, global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_IndicesConfig__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static IndicesConfig() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChNJbmRpY2VzQ29uZmlnLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNvbW1v", 
              "bi5Qcm90b2J1ZhoYQ3JlYXRlSW5kZXhDb21tYW5kLnByb3RvImEKDUluZGlj", 
              "ZXNDb25maWcSUAoSY3JlYXRlSW5kZXhDb21tYW5kGAEgAygLMjQuQWxhY2hp", 
              "c29mdC5Ob3NEQi5Db21tb24uUHJvdG9idWYuQ3JlYXRlSW5kZXhDb21tYW5k", 
              "Qj0KJGNvbS5hbGFjaGlzb2Z0Lm5vc2RiLmNvbW1vbi5wcm90b2J1ZkIVSW5k", 
            "aWNlc0NvbmZpZ1Byb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_IndicesConfig__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_IndicesConfig__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig, global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_IndicesConfig__Descriptor,
                  new string[] { "CreateIndexCommand", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateIndexCommand.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class IndicesConfig : pb::GeneratedMessage<IndicesConfig, IndicesConfig.Builder> {
    private IndicesConfig() { }
    private static readonly IndicesConfig defaultInstance = new IndicesConfig().MakeReadOnly();
    private static readonly string[] _indicesConfigFieldNames = new string[] { "createIndexCommand" };
    private static readonly uint[] _indicesConfigFieldTags = new uint[] { 10 };
    public static IndicesConfig DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override IndicesConfig DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override IndicesConfig ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.IndicesConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_IndicesConfig__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<IndicesConfig, IndicesConfig.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.IndicesConfig.internal__static_Alachisoft_NosDB_Common_Protobuf_IndicesConfig__FieldAccessorTable; }
    }
    
    public const int CreateIndexCommandFieldNumber = 1;
    private pbc::PopsicleList<global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand> createIndexCommand_ = new pbc::PopsicleList<global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand>();
    public scg::IList<global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand> CreateIndexCommandList {
      get { return createIndexCommand_; }
    }
    public int CreateIndexCommandCount {
      get { return createIndexCommand_.Count; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand GetCreateIndexCommand(int index) {
      return createIndexCommand_[index];
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _indicesConfigFieldNames;
      if (createIndexCommand_.Count > 0) {
        output.WriteMessageArray(1, field_names[0], createIndexCommand_);
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
      foreach (global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand element in CreateIndexCommandList) {
        size += pb::CodedOutputStream.ComputeMessageSize(1, element);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static IndicesConfig ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static IndicesConfig ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static IndicesConfig ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static IndicesConfig ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static IndicesConfig ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static IndicesConfig ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static IndicesConfig ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static IndicesConfig ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static IndicesConfig ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static IndicesConfig ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private IndicesConfig MakeReadOnly() {
      createIndexCommand_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(IndicesConfig prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<IndicesConfig, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(IndicesConfig cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private IndicesConfig result;
      
      private IndicesConfig PrepareBuilder() {
        if (resultIsReadOnly) {
          IndicesConfig original = result;
          result = new IndicesConfig();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override IndicesConfig MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.Descriptor; }
      }
      
      public override IndicesConfig DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.DefaultInstance; }
      }
      
      public override IndicesConfig BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is IndicesConfig) {
          return MergeFrom((IndicesConfig) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(IndicesConfig other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.IndicesConfig.DefaultInstance) return this;
        PrepareBuilder();
        if (other.createIndexCommand_.Count != 0) {
          result.createIndexCommand_.Add(other.createIndexCommand_);
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
            int field_ordinal = global::System.Array.BinarySearch(_indicesConfigFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _indicesConfigFieldTags[field_ordinal];
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
              input.ReadMessageArray(tag, field_name, result.createIndexCommand_, global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.DefaultInstance, extensionRegistry);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public pbc::IPopsicleList<global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand> CreateIndexCommandList {
        get { return PrepareBuilder().createIndexCommand_; }
      }
      public int CreateIndexCommandCount {
        get { return result.CreateIndexCommandCount; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand GetCreateIndexCommand(int index) {
        return result.GetCreateIndexCommand(index);
      }
      public Builder SetCreateIndexCommand(int index, global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.createIndexCommand_[index] = value;
        return this;
      }
      public Builder SetCreateIndexCommand(int index, global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.createIndexCommand_[index] = builderForValue.Build();
        return this;
      }
      public Builder AddCreateIndexCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.createIndexCommand_.Add(value);
        return this;
      }
      public Builder AddCreateIndexCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.createIndexCommand_.Add(builderForValue.Build());
        return this;
      }
      public Builder AddRangeCreateIndexCommand(scg::IEnumerable<global::Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand> values) {
        PrepareBuilder();
        result.createIndexCommand_.Add(values);
        return this;
      }
      public Builder ClearCreateIndexCommand() {
        PrepareBuilder();
        result.createIndexCommand_.Clear();
        return this;
      }
    }
    static IndicesConfig() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.IndicesConfig.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
