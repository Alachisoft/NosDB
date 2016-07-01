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
    public static partial class GetDocumentsCommand {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_GetDocumentsCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand, global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_GetDocumentsCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static GetDocumentsCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChlHZXREb2N1bWVudHNDb21tYW5kLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RC", 
              "LkNvbW1vbi5Qcm90b2J1ZiIqChNHZXREb2N1bWVudHNDb21tYW5kEhMKC2Rv", 
              "Y3VtZW50SWRzGAEgAygJQkMKJGNvbS5hbGFjaGlzb2Z0Lm5vc2RiLmNvbW1v", 
            "bi5wcm90b2J1ZkIbR2V0RG9jdW1lbnRzQ29tbWFuZFByb3RvY29s"));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_GetDocumentsCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_GetDocumentsCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand, global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_GetDocumentsCommand__Descriptor,
                  new string[] { "DocumentIds", });
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
  public sealed partial class GetDocumentsCommand : pb::GeneratedMessage<GetDocumentsCommand, GetDocumentsCommand.Builder> {
    private GetDocumentsCommand() { }
    private static readonly GetDocumentsCommand defaultInstance = new GetDocumentsCommand().MakeReadOnly();
    private static readonly string[] _getDocumentsCommandFieldNames = new string[] { "documentIds" };
    private static readonly uint[] _getDocumentsCommandFieldTags = new uint[] { 10 };
    public static GetDocumentsCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override GetDocumentsCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override GetDocumentsCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.GetDocumentsCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_GetDocumentsCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<GetDocumentsCommand, GetDocumentsCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.GetDocumentsCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_GetDocumentsCommand__FieldAccessorTable; }
    }
    
    public const int DocumentIdsFieldNumber = 1;
    private pbc::PopsicleList<string> documentIds_ = new pbc::PopsicleList<string>();
    public scg::IList<string> DocumentIdsList {
      get { return pbc::Lists.AsReadOnly(documentIds_); }
    }
    public int DocumentIdsCount {
      get { return documentIds_.Count; }
    }
    public string GetDocumentIds(int index) {
      return documentIds_[index];
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _getDocumentsCommandFieldNames;
      if (documentIds_.Count > 0) {
        output.WriteStringArray(1, field_names[0], documentIds_);
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
        foreach (string element in DocumentIdsList) {
          dataSize += pb::CodedOutputStream.ComputeStringSizeNoTag(element);
        }
        size += dataSize;
        size += 1 * documentIds_.Count;
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static GetDocumentsCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static GetDocumentsCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static GetDocumentsCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static GetDocumentsCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static GetDocumentsCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static GetDocumentsCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static GetDocumentsCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static GetDocumentsCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static GetDocumentsCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static GetDocumentsCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private GetDocumentsCommand MakeReadOnly() {
      documentIds_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(GetDocumentsCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<GetDocumentsCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(GetDocumentsCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private GetDocumentsCommand result;
      
      private GetDocumentsCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          GetDocumentsCommand original = result;
          result = new GetDocumentsCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override GetDocumentsCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.Descriptor; }
      }
      
      public override GetDocumentsCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.DefaultInstance; }
      }
      
      public override GetDocumentsCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is GetDocumentsCommand) {
          return MergeFrom((GetDocumentsCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(GetDocumentsCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.GetDocumentsCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.documentIds_.Count != 0) {
          result.documentIds_.Add(other.documentIds_);
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
            int field_ordinal = global::System.Array.BinarySearch(_getDocumentsCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _getDocumentsCommandFieldTags[field_ordinal];
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
              input.ReadStringArray(tag, field_name, result.documentIds_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public pbc::IPopsicleList<string> DocumentIdsList {
        get { return PrepareBuilder().documentIds_; }
      }
      public int DocumentIdsCount {
        get { return result.DocumentIdsCount; }
      }
      public string GetDocumentIds(int index) {
        return result.GetDocumentIds(index);
      }
      public Builder SetDocumentIds(int index, string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.documentIds_[index] = value;
        return this;
      }
      public Builder AddDocumentIds(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.documentIds_.Add(value);
        return this;
      }
      public Builder AddRangeDocumentIds(scg::IEnumerable<string> values) {
        PrepareBuilder();
        result.documentIds_.Add(values);
        return this;
      }
      public Builder ClearDocumentIds() {
        PrepareBuilder();
        result.documentIds_.Clear();
        return this;
      }
    }
    static GetDocumentsCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.GetDocumentsCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
