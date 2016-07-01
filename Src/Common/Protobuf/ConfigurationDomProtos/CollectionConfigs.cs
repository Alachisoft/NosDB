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
    public static partial class CollectionConfigs {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_CollectionConfigs__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs, global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_CollectionConfigs__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static CollectionConfigs() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChdDb2xsZWN0aW9uQ29uZmlncy5wcm90bxIgQWxhY2hpc29mdC5Ob3NEQi5D", 
              "b21tb24uUHJvdG9idWYaHUNyZWF0ZUNvbGxlY3Rpb25Db21tYW5kLnByb3Rv", 
              "Im8KEUNvbGxlY3Rpb25Db25maWdzEloKF2NyZWF0ZUNvbGxlY3Rpb25Db21t", 
              "YW5kGAEgAygLMjkuQWxhY2hpc29mdC5Ob3NEQi5Db21tb24uUHJvdG9idWYu", 
              "Q3JlYXRlQ29sbGVjdGlvbkNvbW1hbmRCQQokY29tLmFsYWNoaXNvZnQubm9z", 
            "ZGIuY29tbW9uLnByb3RvYnVmQhlDb2xsZWN0aW9uQ29uZmlnc1Byb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_CollectionConfigs__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_CollectionConfigs__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs, global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_CollectionConfigs__Descriptor,
                  new string[] { "CreateCollectionCommand", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateCollectionCommand.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class CollectionConfigs : pb::GeneratedMessage<CollectionConfigs, CollectionConfigs.Builder> {
    private CollectionConfigs() { }
    private static readonly CollectionConfigs defaultInstance = new CollectionConfigs().MakeReadOnly();
    private static readonly string[] _collectionConfigsFieldNames = new string[] { "createCollectionCommand" };
    private static readonly uint[] _collectionConfigsFieldTags = new uint[] { 10 };
    public static CollectionConfigs DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override CollectionConfigs DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override CollectionConfigs ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CollectionConfigs.internal__static_Alachisoft_NosDB_Common_Protobuf_CollectionConfigs__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<CollectionConfigs, CollectionConfigs.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CollectionConfigs.internal__static_Alachisoft_NosDB_Common_Protobuf_CollectionConfigs__FieldAccessorTable; }
    }
    
    public const int CreateCollectionCommandFieldNumber = 1;
    private pbc::PopsicleList<global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand> createCollectionCommand_ = new pbc::PopsicleList<global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand>();
    public scg::IList<global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand> CreateCollectionCommandList {
      get { return createCollectionCommand_; }
    }
    public int CreateCollectionCommandCount {
      get { return createCollectionCommand_.Count; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand GetCreateCollectionCommand(int index) {
      return createCollectionCommand_[index];
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _collectionConfigsFieldNames;
      if (createCollectionCommand_.Count > 0) {
        output.WriteMessageArray(1, field_names[0], createCollectionCommand_);
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
      foreach (global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand element in CreateCollectionCommandList) {
        size += pb::CodedOutputStream.ComputeMessageSize(1, element);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static CollectionConfigs ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CollectionConfigs ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CollectionConfigs ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CollectionConfigs ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CollectionConfigs ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CollectionConfigs ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static CollectionConfigs ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static CollectionConfigs ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static CollectionConfigs ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CollectionConfigs ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private CollectionConfigs MakeReadOnly() {
      createCollectionCommand_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(CollectionConfigs prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<CollectionConfigs, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(CollectionConfigs cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private CollectionConfigs result;
      
      private CollectionConfigs PrepareBuilder() {
        if (resultIsReadOnly) {
          CollectionConfigs original = result;
          result = new CollectionConfigs();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override CollectionConfigs MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.Descriptor; }
      }
      
      public override CollectionConfigs DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.DefaultInstance; }
      }
      
      public override CollectionConfigs BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is CollectionConfigs) {
          return MergeFrom((CollectionConfigs) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(CollectionConfigs other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.CollectionConfigs.DefaultInstance) return this;
        PrepareBuilder();
        if (other.createCollectionCommand_.Count != 0) {
          result.createCollectionCommand_.Add(other.createCollectionCommand_);
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
            int field_ordinal = global::System.Array.BinarySearch(_collectionConfigsFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _collectionConfigsFieldTags[field_ordinal];
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
              input.ReadMessageArray(tag, field_name, result.createCollectionCommand_, global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.DefaultInstance, extensionRegistry);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public pbc::IPopsicleList<global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand> CreateCollectionCommandList {
        get { return PrepareBuilder().createCollectionCommand_; }
      }
      public int CreateCollectionCommandCount {
        get { return result.CreateCollectionCommandCount; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand GetCreateCollectionCommand(int index) {
        return result.GetCreateCollectionCommand(index);
      }
      public Builder SetCreateCollectionCommand(int index, global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.createCollectionCommand_[index] = value;
        return this;
      }
      public Builder SetCreateCollectionCommand(int index, global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.createCollectionCommand_[index] = builderForValue.Build();
        return this;
      }
      public Builder AddCreateCollectionCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.createCollectionCommand_.Add(value);
        return this;
      }
      public Builder AddCreateCollectionCommand(global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.createCollectionCommand_.Add(builderForValue.Build());
        return this;
      }
      public Builder AddRangeCreateCollectionCommand(scg::IEnumerable<global::Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand> values) {
        PrepareBuilder();
        result.createCollectionCommand_.Add(values);
        return this;
      }
      public Builder ClearCreateCollectionCommand() {
        PrepareBuilder();
        result.createCollectionCommand_.Clear();
        return this;
      }
    }
    static CollectionConfigs() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.CollectionConfigs.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
