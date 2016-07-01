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
    public static partial class ReplaceDocumentsCommand {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_ReplaceDocumentsCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand, global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_ReplaceDocumentsCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static ReplaceDocumentsCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "Ch1SZXBsYWNlRG9jdW1lbnRzQ29tbWFuZC5wcm90bxIgQWxhY2hpc29mdC5O", 
              "b3NEQi5Db21tb24uUHJvdG9idWYiLAoXUmVwbGFjZURvY3VtZW50c0NvbW1h", 
              "bmQSEQoJZG9jdW1lbnRzGAEgAygJQkcKJGNvbS5hbGFjaGlzb2Z0Lm5vc2Ri", 
              "LmNvbW1vbi5wcm90b2J1ZkIfUmVwbGFjZURvY3VtZW50c0NvbW1hbmRQcm90", 
            "b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_ReplaceDocumentsCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_ReplaceDocumentsCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand, global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_ReplaceDocumentsCommand__Descriptor,
                  new string[] { "Documents", });
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
  public sealed partial class ReplaceDocumentsCommand : pb::GeneratedMessage<ReplaceDocumentsCommand, ReplaceDocumentsCommand.Builder> {
    private ReplaceDocumentsCommand() { }
    private static readonly ReplaceDocumentsCommand defaultInstance = new ReplaceDocumentsCommand().MakeReadOnly();
    private static readonly string[] _replaceDocumentsCommandFieldNames = new string[] { "documents" };
    private static readonly uint[] _replaceDocumentsCommandFieldTags = new uint[] { 10 };
    public static ReplaceDocumentsCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override ReplaceDocumentsCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override ReplaceDocumentsCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.ReplaceDocumentsCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_ReplaceDocumentsCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<ReplaceDocumentsCommand, ReplaceDocumentsCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.ReplaceDocumentsCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_ReplaceDocumentsCommand__FieldAccessorTable; }
    }
    
    public const int DocumentsFieldNumber = 1;
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
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _replaceDocumentsCommandFieldNames;
      if (documents_.Count > 0) {
        output.WriteStringArray(1, field_names[0], documents_);
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
      {
        int dataSize = 0;
        foreach (string element in DocumentsList) {
          dataSize += pb::CodedOutputStream.ComputeStringSizeNoTag(element);
        }
        size += dataSize;
        size += 1 * documents_.Count;
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static ReplaceDocumentsCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static ReplaceDocumentsCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private ReplaceDocumentsCommand MakeReadOnly() {
      documents_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(ReplaceDocumentsCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<ReplaceDocumentsCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(ReplaceDocumentsCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private ReplaceDocumentsCommand result;
      
      private ReplaceDocumentsCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          ReplaceDocumentsCommand original = result;
          result = new ReplaceDocumentsCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override ReplaceDocumentsCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.Descriptor; }
      }
      
      public override ReplaceDocumentsCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.DefaultInstance; }
      }
      
      public override ReplaceDocumentsCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is ReplaceDocumentsCommand) {
          return MergeFrom((ReplaceDocumentsCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(ReplaceDocumentsCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.ReplaceDocumentsCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.documents_.Count != 0) {
          result.documents_.Add(other.documents_);
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
            int field_ordinal = global::System.Array.BinarySearch(_replaceDocumentsCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _replaceDocumentsCommandFieldTags[field_ordinal];
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
              input.ReadStringArray(tag, field_name, result.documents_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
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
    }
    static ReplaceDocumentsCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.ReplaceDocumentsCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
