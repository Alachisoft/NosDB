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
    public static partial class ResponseAuthenticationToken {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_ResponseAuthenticationToken__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken, global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_ResponseAuthenticationToken__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static ResponseAuthenticationToken() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "CiFSZXNwb25zZUF1dGhlbnRpY2F0aW9uVG9rZW4ucHJvdG8SIEFsYWNoaXNv", 
              "ZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmIjwKG1Jlc3BvbnNlQXV0aGVudGlj", 
              "YXRpb25Ub2tlbhIOCgZzdGF0dXMYASABKAUSDQoFdG9rZW4YAiABKAxCSwok", 
              "Y29tLmFsYWNoaXNvZnQubm9zZGIuY29tbW9uLnByb3RvYnVmQiNSZXNwb25z", 
            "ZUF1dGhlbnRpY2F0aW9uVG9rZW5Qcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_ResponseAuthenticationToken__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_ResponseAuthenticationToken__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken, global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_ResponseAuthenticationToken__Descriptor,
                  new string[] { "Status", "Token", });
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
  public sealed partial class ResponseAuthenticationToken : pb::GeneratedMessage<ResponseAuthenticationToken, ResponseAuthenticationToken.Builder> {
    private ResponseAuthenticationToken() { }
    private static readonly ResponseAuthenticationToken defaultInstance = new ResponseAuthenticationToken().MakeReadOnly();
    private static readonly string[] _responseAuthenticationTokenFieldNames = new string[] { "status", "token" };
    private static readonly uint[] _responseAuthenticationTokenFieldTags = new uint[] { 8, 18 };
    public static ResponseAuthenticationToken DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override ResponseAuthenticationToken DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override ResponseAuthenticationToken ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.ResponseAuthenticationToken.internal__static_Alachisoft_NosDB_Common_Protobuf_ResponseAuthenticationToken__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<ResponseAuthenticationToken, ResponseAuthenticationToken.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.ResponseAuthenticationToken.internal__static_Alachisoft_NosDB_Common_Protobuf_ResponseAuthenticationToken__FieldAccessorTable; }
    }
    
    public const int StatusFieldNumber = 1;
    private bool hasStatus;
    private int status_;
    public bool HasStatus {
      get { return hasStatus; }
    }
    public int Status {
      get { return status_; }
    }
    
    public const int TokenFieldNumber = 2;
    private bool hasToken;
    private pb::ByteString token_ = pb::ByteString.Empty;
    public bool HasToken {
      get { return hasToken; }
    }
    public pb::ByteString Token {
      get { return token_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _responseAuthenticationTokenFieldNames;
      if (hasStatus) {
        output.WriteInt32(1, field_names[0], Status);
      }
      if (hasToken) {
        output.WriteBytes(2, field_names[1], Token);
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
      if (hasStatus) {
        size += pb::CodedOutputStream.ComputeInt32Size(1, Status);
      }
      if (hasToken) {
        size += pb::CodedOutputStream.ComputeBytesSize(2, Token);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static ResponseAuthenticationToken ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static ResponseAuthenticationToken ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private ResponseAuthenticationToken MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(ResponseAuthenticationToken prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<ResponseAuthenticationToken, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(ResponseAuthenticationToken cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private ResponseAuthenticationToken result;
      
      private ResponseAuthenticationToken PrepareBuilder() {
        if (resultIsReadOnly) {
          ResponseAuthenticationToken original = result;
          result = new ResponseAuthenticationToken();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override ResponseAuthenticationToken MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.Descriptor; }
      }
      
      public override ResponseAuthenticationToken DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.DefaultInstance; }
      }
      
      public override ResponseAuthenticationToken BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is ResponseAuthenticationToken) {
          return MergeFrom((ResponseAuthenticationToken) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(ResponseAuthenticationToken other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasStatus) {
          Status = other.Status;
        }
        if (other.HasToken) {
          Token = other.Token;
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
            int field_ordinal = global::System.Array.BinarySearch(_responseAuthenticationTokenFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _responseAuthenticationTokenFieldTags[field_ordinal];
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
              result.hasStatus = input.ReadInt32(ref result.status_);
              break;
            }
            case 18: {
              result.hasToken = input.ReadBytes(ref result.token_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasStatus {
        get { return result.hasStatus; }
      }
      public int Status {
        get { return result.Status; }
        set { SetStatus(value); }
      }
      public Builder SetStatus(int value) {
        PrepareBuilder();
        result.hasStatus = true;
        result.status_ = value;
        return this;
      }
      public Builder ClearStatus() {
        PrepareBuilder();
        result.hasStatus = false;
        result.status_ = 0;
        return this;
      }
      
      public bool HasToken {
        get { return result.hasToken; }
      }
      public pb::ByteString Token {
        get { return result.Token; }
        set { SetToken(value); }
      }
      public Builder SetToken(pb::ByteString value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasToken = true;
        result.token_ = value;
        return this;
      }
      public Builder ClearToken() {
        PrepareBuilder();
        result.hasToken = false;
        result.token_ = pb::ByteString.Empty;
        return this;
      }
    }
    static ResponseAuthenticationToken() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.ResponseAuthenticationToken.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
