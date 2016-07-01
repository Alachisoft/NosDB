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
    public static partial class FailedDocument {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_FailedDocument__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.FailedDocument, global::Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_FailedDocument__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static FailedDocument() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChRGYWlsZWREb2N1bWVudC5wcm90bxIgQWxhY2hpc29mdC5Ob3NEQi5Db21t", 
              "b24uUHJvdG9idWYiTAoORmFpbGVkRG9jdW1lbnQSEgoKZG9jdW1lbnRJZBgB", 
              "IAEoCRIRCgllcnJvckNvZGUYAiABKAUSEwoLZXJyb3JQYXJhbXMYAyADKAlC", 
              "PwokY29tLmFsYWNoaXNvZnQubm9zZGIuY29tbW9uLnByb3RvYnVmQhdGYWls", 
            "ZWREb2N1bWVudHNQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_FailedDocument__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_FailedDocument__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.FailedDocument, global::Alachisoft.NosDB.Common.Protobuf.FailedDocument.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_FailedDocument__Descriptor,
                  new string[] { "DocumentId", "ErrorCode", "ErrorParams", });
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
  public sealed partial class FailedDocument : pb::GeneratedMessage<FailedDocument, FailedDocument.Builder> {
    private FailedDocument() { }
    private static readonly FailedDocument defaultInstance = new FailedDocument().MakeReadOnly();
    private static readonly string[] _failedDocumentFieldNames = new string[] { "documentId", "errorCode", "errorParams" };
    private static readonly uint[] _failedDocumentFieldTags = new uint[] { 10, 16, 26 };
    public static FailedDocument DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override FailedDocument DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override FailedDocument ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.FailedDocument.internal__static_Alachisoft_NosDB_Common_Protobuf_FailedDocument__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<FailedDocument, FailedDocument.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.FailedDocument.internal__static_Alachisoft_NosDB_Common_Protobuf_FailedDocument__FieldAccessorTable; }
    }
    
    public const int DocumentIdFieldNumber = 1;
    private bool hasDocumentId;
    private string documentId_ = "";
    public bool HasDocumentId {
      get { return hasDocumentId; }
    }
    public string DocumentId {
      get { return documentId_; }
    }
    
    public const int ErrorCodeFieldNumber = 2;
    private bool hasErrorCode;
    private int errorCode_;
    public bool HasErrorCode {
      get { return hasErrorCode; }
    }
    public int ErrorCode {
      get { return errorCode_; }
    }
    
    public const int ErrorParamsFieldNumber = 3;
    private pbc::PopsicleList<string> errorParams_ = new pbc::PopsicleList<string>();
    public scg::IList<string> ErrorParamsList {
      get { return pbc::Lists.AsReadOnly(errorParams_); }
    }
    public int ErrorParamsCount {
      get { return errorParams_.Count; }
    }
    public string GetErrorParams(int index) {
      return errorParams_[index];
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _failedDocumentFieldNames;
      if (hasDocumentId) {
        output.WriteString(1, field_names[0], DocumentId);
      }
      if (hasErrorCode) {
        output.WriteInt32(2, field_names[1], ErrorCode);
      }
      if (errorParams_.Count > 0) {
        output.WriteStringArray(3, field_names[2], errorParams_);
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
      if (hasDocumentId) {
        size += pb::CodedOutputStream.ComputeStringSize(1, DocumentId);
      }
      if (hasErrorCode) {
        size += pb::CodedOutputStream.ComputeInt32Size(2, ErrorCode);
      }
      {
        int dataSize = 0;
        foreach (string element in ErrorParamsList) {
          dataSize += pb::CodedOutputStream.ComputeStringSizeNoTag(element);
        }
        size += dataSize;
        size += 1 * errorParams_.Count;
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static FailedDocument ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static FailedDocument ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static FailedDocument ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static FailedDocument ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static FailedDocument ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static FailedDocument ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static FailedDocument ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static FailedDocument ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static FailedDocument ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static FailedDocument ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private FailedDocument MakeReadOnly() {
      errorParams_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(FailedDocument prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<FailedDocument, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(FailedDocument cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private FailedDocument result;
      
      private FailedDocument PrepareBuilder() {
        if (resultIsReadOnly) {
          FailedDocument original = result;
          result = new FailedDocument();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override FailedDocument MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.FailedDocument.Descriptor; }
      }
      
      public override FailedDocument DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.FailedDocument.DefaultInstance; }
      }
      
      public override FailedDocument BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is FailedDocument) {
          return MergeFrom((FailedDocument) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(FailedDocument other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.FailedDocument.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasDocumentId) {
          DocumentId = other.DocumentId;
        }
        if (other.HasErrorCode) {
          ErrorCode = other.ErrorCode;
        }
        if (other.errorParams_.Count != 0) {
          result.errorParams_.Add(other.errorParams_);
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
            int field_ordinal = global::System.Array.BinarySearch(_failedDocumentFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _failedDocumentFieldTags[field_ordinal];
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
              result.hasDocumentId = input.ReadString(ref result.documentId_);
              break;
            }
            case 16: {
              result.hasErrorCode = input.ReadInt32(ref result.errorCode_);
              break;
            }
            case 26: {
              input.ReadStringArray(tag, field_name, result.errorParams_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasDocumentId {
        get { return result.hasDocumentId; }
      }
      public string DocumentId {
        get { return result.DocumentId; }
        set { SetDocumentId(value); }
      }
      public Builder SetDocumentId(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasDocumentId = true;
        result.documentId_ = value;
        return this;
      }
      public Builder ClearDocumentId() {
        PrepareBuilder();
        result.hasDocumentId = false;
        result.documentId_ = "";
        return this;
      }
      
      public bool HasErrorCode {
        get { return result.hasErrorCode; }
      }
      public int ErrorCode {
        get { return result.ErrorCode; }
        set { SetErrorCode(value); }
      }
      public Builder SetErrorCode(int value) {
        PrepareBuilder();
        result.hasErrorCode = true;
        result.errorCode_ = value;
        return this;
      }
      public Builder ClearErrorCode() {
        PrepareBuilder();
        result.hasErrorCode = false;
        result.errorCode_ = 0;
        return this;
      }
      
      public pbc::IPopsicleList<string> ErrorParamsList {
        get { return PrepareBuilder().errorParams_; }
      }
      public int ErrorParamsCount {
        get { return result.ErrorParamsCount; }
      }
      public string GetErrorParams(int index) {
        return result.GetErrorParams(index);
      }
      public Builder SetErrorParams(int index, string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.errorParams_[index] = value;
        return this;
      }
      public Builder AddErrorParams(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.errorParams_.Add(value);
        return this;
      }
      public Builder AddRangeErrorParams(scg::IEnumerable<string> values) {
        PrepareBuilder();
        result.errorParams_.Add(values);
        return this;
      }
      public Builder ClearErrorParams() {
        PrepareBuilder();
        result.errorParams_.Clear();
        return this;
      }
    }
    static FailedDocument() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.FailedDocument.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
