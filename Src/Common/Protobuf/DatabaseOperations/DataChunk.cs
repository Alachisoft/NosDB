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
    public static partial class DataChunk {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_DataChunk__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DataChunk, global::Alachisoft.NosDB.Common.Protobuf.DataChunk.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_DataChunk__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static DataChunk() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "Cg9EYXRhQ2h1bmsucHJvdG8SIEFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlBy", 
              "b3RvYnVmIlcKCURhdGFDaHVuaxIPCgdjaHVua0lkGAEgASgFEhEKCXJlYWRl", 
              "clVJZBgCIAEoCRIRCglkb2N1bWVudHMYAyADKAkSEwoLaXNMYXN0Q2h1bmsY", 
              "BCABKAhCOQokY29tLmFsYWNoaXNvZnQubm9zZGIuY29tbW9uLnByb3RvYnVm", 
            "QhFEYXRhQ2h1bmtQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_DataChunk__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_DataChunk__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DataChunk, global::Alachisoft.NosDB.Common.Protobuf.DataChunk.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_DataChunk__Descriptor,
                  new string[] { "ChunkId", "ReaderUId", "Documents", "IsLastChunk", });
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
  public sealed partial class DataChunk : pb::GeneratedMessage<DataChunk, DataChunk.Builder> {
    private DataChunk() { }
    private static readonly DataChunk defaultInstance = new DataChunk().MakeReadOnly();
    private static readonly string[] _dataChunkFieldNames = new string[] { "chunkId", "documents", "isLastChunk", "readerUId" };
    private static readonly uint[] _dataChunkFieldTags = new uint[] { 8, 26, 32, 18 };
    public static DataChunk DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override DataChunk DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override DataChunk ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DataChunk.internal__static_Alachisoft_NosDB_Common_Protobuf_DataChunk__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<DataChunk, DataChunk.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DataChunk.internal__static_Alachisoft_NosDB_Common_Protobuf_DataChunk__FieldAccessorTable; }
    }
    
    public const int ChunkIdFieldNumber = 1;
    private bool hasChunkId;
    private int chunkId_;
    public bool HasChunkId {
      get { return hasChunkId; }
    }
    public int ChunkId {
      get { return chunkId_; }
    }
    
    public const int ReaderUIdFieldNumber = 2;
    private bool hasReaderUId;
    private string readerUId_ = "";
    public bool HasReaderUId {
      get { return hasReaderUId; }
    }
    public string ReaderUId {
      get { return readerUId_; }
    }
    
    public const int DocumentsFieldNumber = 3;
    private pbc::PopsicleList<string> documents_ = new pbc::PopsicleList<string>();
    public scg::IList<string> DocumentsList {
      get { return pbc::Lists.AsReadOnly(documents_); }
    }
    public int DocumentsCount {
      get { return documents_.Count; }
    }
    public string GetDocuments(int index) {
      return documents_[index];
    }
    
    public const int IsLastChunkFieldNumber = 4;
    private bool hasIsLastChunk;
    private bool isLastChunk_;
    public bool HasIsLastChunk {
      get { return hasIsLastChunk; }
    }
    public bool IsLastChunk {
      get { return isLastChunk_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _dataChunkFieldNames;
      if (hasChunkId) {
        output.WriteInt32(1, field_names[0], ChunkId);
      }
      if (hasReaderUId) {
        output.WriteString(2, field_names[3], ReaderUId);
      }
      if (documents_.Count > 0) {
        output.WriteStringArray(3, field_names[1], documents_);
      }
      if (hasIsLastChunk) {
        output.WriteBool(4, field_names[2], IsLastChunk);
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
      if (hasChunkId) {
        size += pb::CodedOutputStream.ComputeInt32Size(1, ChunkId);
      }
      if (hasReaderUId) {
        size += pb::CodedOutputStream.ComputeStringSize(2, ReaderUId);
      }
      {
        int dataSize = 0;
        foreach (string element in DocumentsList) {
          dataSize += pb::CodedOutputStream.ComputeStringSizeNoTag(element);
        }
        size += dataSize;
        size += 1 * documents_.Count;
      }
      if (hasIsLastChunk) {
        size += pb::CodedOutputStream.ComputeBoolSize(4, IsLastChunk);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static DataChunk ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DataChunk ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DataChunk ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DataChunk ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DataChunk ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DataChunk ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static DataChunk ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static DataChunk ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static DataChunk ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DataChunk ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private DataChunk MakeReadOnly() {
      documents_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(DataChunk prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<DataChunk, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(DataChunk cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private DataChunk result;
      
      private DataChunk PrepareBuilder() {
        if (resultIsReadOnly) {
          DataChunk original = result;
          result = new DataChunk();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override DataChunk MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.DataChunk.Descriptor; }
      }
      
      public override DataChunk DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.DataChunk.DefaultInstance; }
      }
      
      public override DataChunk BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is DataChunk) {
          return MergeFrom((DataChunk) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(DataChunk other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.DataChunk.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasChunkId) {
          ChunkId = other.ChunkId;
        }
        if (other.HasReaderUId) {
          ReaderUId = other.ReaderUId;
        }
        if (other.documents_.Count != 0) {
          result.documents_.Add(other.documents_);
        }
        if (other.HasIsLastChunk) {
          IsLastChunk = other.IsLastChunk;
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
            int field_ordinal = global::System.Array.BinarySearch(_dataChunkFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _dataChunkFieldTags[field_ordinal];
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
              result.hasChunkId = input.ReadInt32(ref result.chunkId_);
              break;
            }
            case 18: {
              result.hasReaderUId = input.ReadString(ref result.readerUId_);
              break;
            }
            case 26: {
              input.ReadStringArray(tag, field_name, result.documents_);
              break;
            }
            case 32: {
              result.hasIsLastChunk = input.ReadBool(ref result.isLastChunk_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasChunkId {
        get { return result.hasChunkId; }
      }
      public int ChunkId {
        get { return result.ChunkId; }
        set { SetChunkId(value); }
      }
      public Builder SetChunkId(int value) {
        PrepareBuilder();
        result.hasChunkId = true;
        result.chunkId_ = value;
        return this;
      }
      public Builder ClearChunkId() {
        PrepareBuilder();
        result.hasChunkId = false;
        result.chunkId_ = 0;
        return this;
      }
      
      public bool HasReaderUId {
        get { return result.hasReaderUId; }
      }
      public string ReaderUId {
        get { return result.ReaderUId; }
        set { SetReaderUId(value); }
      }
      public Builder SetReaderUId(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasReaderUId = true;
        result.readerUId_ = value;
        return this;
      }
      public Builder ClearReaderUId() {
        PrepareBuilder();
        result.hasReaderUId = false;
        result.readerUId_ = "";
        return this;
      }
      
      public pbc::IPopsicleList<string> DocumentsList {
        get { return PrepareBuilder().documents_; }
      }
      public int DocumentsCount {
        get { return result.DocumentsCount; }
      }
      public string GetDocuments(int index) {
        return result.GetDocuments(index);
      }
      public Builder SetDocuments(int index, string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.documents_[index] = value;
        return this;
      }
      public Builder AddDocuments(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.documents_.Add(value);
        return this;
      }
      public Builder AddRangeDocuments(scg::IEnumerable<string> values) {
        PrepareBuilder();
        result.documents_.Add(values);
        return this;
      }
      public Builder ClearDocuments() {
        PrepareBuilder();
        result.documents_.Clear();
        return this;
      }
      
      public bool HasIsLastChunk {
        get { return result.hasIsLastChunk; }
      }
      public bool IsLastChunk {
        get { return result.IsLastChunk; }
        set { SetIsLastChunk(value); }
      }
      public Builder SetIsLastChunk(bool value) {
        PrepareBuilder();
        result.hasIsLastChunk = true;
        result.isLastChunk_ = value;
        return this;
      }
      public Builder ClearIsLastChunk() {
        PrepareBuilder();
        result.hasIsLastChunk = false;
        result.isLastChunk_ = false;
        return this;
      }
    }
    static DataChunk() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.DataChunk.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
