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
    public static partial class GetChunkCommand {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand, global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static GetChunkCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChVHZXRDaHVua0NvbW1hbmQucHJvdG8SIEFsYWNoaXNvZnQuTm9zREIuQ29t", 
              "bW9uLlByb3RvYnVmIjkKD0dldENodW5rQ29tbWFuZBITCgtsYXN0Q2h1bmtJ", 
              "ZBgBIAEoBRIRCglyZWFkZXJVSUQYAiABKAlCPwokY29tLmFsYWNoaXNvZnQu", 
              "bm9zZGIuY29tbW9uLnByb3RvYnVmQhdHZXRDaHVua0NvbW1hbmRQcm90b2Nv", 
            "bA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand, global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkCommand__Descriptor,
                  new string[] { "LastChunkId", "ReaderUID", });
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
  public sealed partial class GetChunkCommand : pb::GeneratedMessage<GetChunkCommand, GetChunkCommand.Builder> {
    private GetChunkCommand() { }
    private static readonly GetChunkCommand defaultInstance = new GetChunkCommand().MakeReadOnly();
    private static readonly string[] _getChunkCommandFieldNames = new string[] { "lastChunkId", "readerUID" };
    private static readonly uint[] _getChunkCommandFieldTags = new uint[] { 8, 18 };
    public static GetChunkCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override GetChunkCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override GetChunkCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.GetChunkCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<GetChunkCommand, GetChunkCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.GetChunkCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_GetChunkCommand__FieldAccessorTable; }
    }
    
    public const int LastChunkIdFieldNumber = 1;
    private bool hasLastChunkId;
    private int lastChunkId_;
    public bool HasLastChunkId {
      get { return hasLastChunkId; }
    }
    public int LastChunkId {
      get { return lastChunkId_; }
    }
    
    public const int ReaderUIDFieldNumber = 2;
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
      string[] field_names = _getChunkCommandFieldNames;
      if (hasLastChunkId) {
        output.WriteInt32(1, field_names[0], LastChunkId);
      }
      if (hasReaderUID) {
        output.WriteString(2, field_names[1], ReaderUID);
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
      if (hasLastChunkId) {
        size += pb::CodedOutputStream.ComputeInt32Size(1, LastChunkId);
      }
      if (hasReaderUID) {
        size += pb::CodedOutputStream.ComputeStringSize(2, ReaderUID);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static GetChunkCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static GetChunkCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static GetChunkCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static GetChunkCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static GetChunkCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static GetChunkCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static GetChunkCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static GetChunkCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static GetChunkCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static GetChunkCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private GetChunkCommand MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(GetChunkCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<GetChunkCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(GetChunkCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private GetChunkCommand result;
      
      private GetChunkCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          GetChunkCommand original = result;
          result = new GetChunkCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override GetChunkCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.Descriptor; }
      }
      
      public override GetChunkCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.DefaultInstance; }
      }
      
      public override GetChunkCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is GetChunkCommand) {
          return MergeFrom((GetChunkCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(GetChunkCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.GetChunkCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasLastChunkId) {
          LastChunkId = other.LastChunkId;
        }
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
            int field_ordinal = global::System.Array.BinarySearch(_getChunkCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _getChunkCommandFieldTags[field_ordinal];
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
              result.hasLastChunkId = input.ReadInt32(ref result.lastChunkId_);
              break;
            }
            case 18: {
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
      
      
      public bool HasLastChunkId {
        get { return result.hasLastChunkId; }
      }
      public int LastChunkId {
        get { return result.LastChunkId; }
        set { SetLastChunkId(value); }
      }
      public Builder SetLastChunkId(int value) {
        PrepareBuilder();
        result.hasLastChunkId = true;
        result.lastChunkId_ = value;
        return this;
      }
      public Builder ClearLastChunkId() {
        PrepareBuilder();
        result.hasLastChunkId = false;
        result.lastChunkId_ = 0;
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
    static GetChunkCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.GetChunkCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
