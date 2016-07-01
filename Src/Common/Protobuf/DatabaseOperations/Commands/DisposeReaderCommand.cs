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
    public static partial class DisposeReaderCommand {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_DisposeReaderCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand, global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_DisposeReaderCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static DisposeReaderCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChpEaXNwb3NlUmVhZGVyQ29tbWFuZC5wcm90bxIgQWxhY2hpc29mdC5Ob3NE", 
              "Qi5Db21tb24uUHJvdG9idWYiKQoURGlzcG9zZVJlYWRlckNvbW1hbmQSEQoJ", 
              "cmVhZGVyVUlEGAEgASgJQkQKJGNvbS5hbGFjaGlzb2Z0Lm5vc2RiLmNvbW1v", 
            "bi5wcm90b2J1ZkIcRGlzcG9zZVJlYWRlckNvbW1hbmRQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_DisposeReaderCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_DisposeReaderCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand, global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_DisposeReaderCommand__Descriptor,
                  new string[] { "ReaderUID", });
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
  public sealed partial class DisposeReaderCommand : pb::GeneratedMessage<DisposeReaderCommand, DisposeReaderCommand.Builder> {
    private DisposeReaderCommand() { }
    private static readonly DisposeReaderCommand defaultInstance = new DisposeReaderCommand().MakeReadOnly();
    private static readonly string[] _disposeReaderCommandFieldNames = new string[] { "readerUID" };
    private static readonly uint[] _disposeReaderCommandFieldTags = new uint[] { 10 };
    public static DisposeReaderCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override DisposeReaderCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override DisposeReaderCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DisposeReaderCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_DisposeReaderCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<DisposeReaderCommand, DisposeReaderCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DisposeReaderCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_DisposeReaderCommand__FieldAccessorTable; }
    }
    
    public const int ReaderUIDFieldNumber = 1;
    private bool hasReaderUID;
    private string readerUID_ = "";
    public bool HasReaderUID {
      get { return hasReaderUID; }
    }
    public string ReaderUID {
      get { return readerUID_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _disposeReaderCommandFieldNames;
      if (hasReaderUID) {
        output.WriteString(1, field_names[0], ReaderUID);
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
      if (hasReaderUID) {
        size += pb::CodedOutputStream.ComputeStringSize(1, ReaderUID);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static DisposeReaderCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DisposeReaderCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DisposeReaderCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DisposeReaderCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DisposeReaderCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DisposeReaderCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static DisposeReaderCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static DisposeReaderCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static DisposeReaderCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DisposeReaderCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private DisposeReaderCommand MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(DisposeReaderCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<DisposeReaderCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(DisposeReaderCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private DisposeReaderCommand result;
      
      private DisposeReaderCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          DisposeReaderCommand original = result;
          result = new DisposeReaderCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override DisposeReaderCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.Descriptor; }
      }
      
      public override DisposeReaderCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.DefaultInstance; }
      }
      
      public override DisposeReaderCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is DisposeReaderCommand) {
          return MergeFrom((DisposeReaderCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(DisposeReaderCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.DisposeReaderCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasReaderUID) {
          ReaderUID = other.ReaderUID;
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
            int field_ordinal = global::System.Array.BinarySearch(_disposeReaderCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _disposeReaderCommandFieldTags[field_ordinal];
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
              result.hasReaderUID = input.ReadString(ref result.readerUID_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasReaderUID {
        get { return result.hasReaderUID; }
      }
      public string ReaderUID {
        get { return result.ReaderUID; }
        set { SetReaderUID(value); }
      }
      public Builder SetReaderUID(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasReaderUID = true;
        result.readerUID_ = value;
        return this;
      }
      public Builder ClearReaderUID() {
        PrepareBuilder();
        result.hasReaderUID = false;
        result.readerUID_ = "";
        return this;
      }
    }
    static DisposeReaderCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.DisposeReaderCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
