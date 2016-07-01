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
    public static partial class AuthenticationResponse {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationResponse__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse, global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationResponse__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static AuthenticationResponse() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChxBdXRoZW50aWNhdGlvblJlc3BvbnNlLnByb3RvEiBBbGFjaGlzb2Z0Lk5v", 
              "c0RCLkNvbW1vbi5Qcm90b2J1ZhohUmVzcG9uc2VBdXRoZW50aWNhdGlvblRv", 
              "a2VuLnByb3RvInQKFkF1dGhlbnRpY2F0aW9uUmVzcG9uc2USWgoTYXV0aGVu", 
              "dGljYXRpb25Ub2tlbhgBIAEoCzI9LkFsYWNoaXNvZnQuTm9zREIuQ29tbW9u", 
              "LlByb3RvYnVmLlJlc3BvbnNlQXV0aGVudGljYXRpb25Ub2tlbkJGCiRjb20u", 
              "YWxhY2hpc29mdC5ub3NkYi5jb21tb24ucHJvdG9idWZCHkF1dGhlbnRpY2F0", 
            "aW9uUmVzcG9uc2VQcm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationResponse__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationResponse__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse, global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationResponse__Descriptor,
                  new string[] { "AuthenticationToken", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.ResponseAuthenticationToken.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class AuthenticationResponse : pb::GeneratedMessage<AuthenticationResponse, AuthenticationResponse.Builder> {
    private AuthenticationResponse() { }
    private static readonly AuthenticationResponse defaultInstance = new AuthenticationResponse().MakeReadOnly();
    private static readonly string[] _authenticationResponseFieldNames = new string[] { "authenticationToken" };
    private static readonly uint[] _authenticationResponseFieldTags = new uint[] { 10 };
    public static AuthenticationResponse DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override AuthenticationResponse DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override AuthenticationResponse ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationResponse__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<AuthenticationResponse, AuthenticationResponse.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationResponse__FieldAccessorTable; }
    }
    
    public const int AuthenticationTokenFieldNumber = 1;
    private bool hasAuthenticationToken;
    private global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken authenticationToken_;
    public bool HasAuthenticationToken {
      get { return hasAuthenticationToken; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken AuthenticationToken {
      get { return authenticationToken_ ?? global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _authenticationResponseFieldNames;
      if (hasAuthenticationToken) {
        output.WriteMessage(1, field_names[0], AuthenticationToken);
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
      if (hasAuthenticationToken) {
        size += pb::CodedOutputStream.ComputeMessageSize(1, AuthenticationToken);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static AuthenticationResponse ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static AuthenticationResponse ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationResponse ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static AuthenticationResponse ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationResponse ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static AuthenticationResponse ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationResponse ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static AuthenticationResponse ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static AuthenticationResponse ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static AuthenticationResponse ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private AuthenticationResponse MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(AuthenticationResponse prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<AuthenticationResponse, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(AuthenticationResponse cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private AuthenticationResponse result;
      
      private AuthenticationResponse PrepareBuilder() {
        if (resultIsReadOnly) {
          AuthenticationResponse original = result;
          result = new AuthenticationResponse();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override AuthenticationResponse MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.Descriptor; }
      }
      
      public override AuthenticationResponse DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.DefaultInstance; }
      }
      
      public override AuthenticationResponse BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is AuthenticationResponse) {
          return MergeFrom((AuthenticationResponse) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(AuthenticationResponse other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.AuthenticationResponse.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasAuthenticationToken) {
          MergeAuthenticationToken(other.AuthenticationToken);
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
            int field_ordinal = global::System.Array.BinarySearch(_authenticationResponseFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _authenticationResponseFieldTags[field_ordinal];
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
              global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.CreateBuilder();
              if (result.hasAuthenticationToken) {
                subBuilder.MergeFrom(AuthenticationToken);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              AuthenticationToken = subBuilder.BuildPartial();
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasAuthenticationToken {
       get { return result.hasAuthenticationToken; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken AuthenticationToken {
        get { return result.AuthenticationToken; }
        set { SetAuthenticationToken(value); }
      }
      public Builder SetAuthenticationToken(global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasAuthenticationToken = true;
        result.authenticationToken_ = value;
        return this;
      }
      public Builder SetAuthenticationToken(global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasAuthenticationToken = true;
        result.authenticationToken_ = builderForValue.Build();
        return this;
      }
      public Builder MergeAuthenticationToken(global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasAuthenticationToken &&
            result.authenticationToken_ != global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.DefaultInstance) {
            result.authenticationToken_ = global::Alachisoft.NosDB.Common.Protobuf.ResponseAuthenticationToken.CreateBuilder(result.authenticationToken_).MergeFrom(value).BuildPartial();
        } else {
          result.authenticationToken_ = value;
        }
        result.hasAuthenticationToken = true;
        return this;
      }
      public Builder ClearAuthenticationToken() {
        PrepareBuilder();
        result.hasAuthenticationToken = false;
        result.authenticationToken_ = null;
        return this;
      }
    }
    static AuthenticationResponse() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationResponse.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
