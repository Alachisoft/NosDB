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
    public static partial class AuthenticationToken {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationToken__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken, global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationToken__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static AuthenticationToken() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChlBdXRoZW50aWNhdGlvblRva2VuLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RC", 
              "LkNvbW1vbi5Qcm90b2J1ZiI0ChNBdXRoZW50aWNhdGlvblRva2VuEg4KBnN0", 
              "YXR1cxgBIAEoBRINCgV0b2tlbhgCIAEoDEJDCiRjb20uYWxhY2hpc29mdC5u", 
              "b3NkYi5jb21tb24ucHJvdG9idWZCG0F1dGhlbnRpY2F0aW9uVG9rZW5Qcm90", 
            "b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationToken__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationToken__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken, global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationToken__Descriptor,
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
  public sealed partial class AuthenticationToken : pb::GeneratedMessage<AuthenticationToken, AuthenticationToken.Builder> {
    private AuthenticationToken() { }
    private static readonly AuthenticationToken defaultInstance = new AuthenticationToken().MakeReadOnly();
    private static readonly string[] _authenticationTokenFieldNames = new string[] { "status", "token" };
    private static readonly uint[] _authenticationTokenFieldTags = new uint[] { 8, 18 };
    public static AuthenticationToken DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override AuthenticationToken DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override AuthenticationToken ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationToken.internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationToken__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<AuthenticationToken, AuthenticationToken.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationToken.internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationToken__FieldAccessorTable; }
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
      string[] field_names = _authenticationTokenFieldNames;
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
    public static AuthenticationToken ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static AuthenticationToken ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationToken ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static AuthenticationToken ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationToken ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static AuthenticationToken ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationToken ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static AuthenticationToken ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static AuthenticationToken ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static AuthenticationToken ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private AuthenticationToken MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(AuthenticationToken prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<AuthenticationToken, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(AuthenticationToken cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private AuthenticationToken result;
      
      private AuthenticationToken PrepareBuilder() {
        if (resultIsReadOnly) {
          AuthenticationToken original = result;
          result = new AuthenticationToken();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override AuthenticationToken MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.Descriptor; }
      }
      
      public override AuthenticationToken DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.DefaultInstance; }
      }
      
      public override AuthenticationToken BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is AuthenticationToken) {
          return MergeFrom((AuthenticationToken) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(AuthenticationToken other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.DefaultInstance) return this;
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
            int field_ordinal = global::System.Array.BinarySearch(_authenticationTokenFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _authenticationTokenFieldTags[field_ordinal];
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
    static AuthenticationToken() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationToken.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
