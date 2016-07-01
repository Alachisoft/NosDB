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
    public static partial class AuthenticationCommand {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand, global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static AuthenticationCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChtBdXRoZW50aWNhdGlvbkNvbW1hbmQucHJvdG8SIEFsYWNoaXNvZnQuTm9z", 
              "REIuQ29tbW9uLlByb3RvYnVmGhlBdXRoZW50aWNhdGlvblRva2VuLnByb3Rv", 
              "IpgBChVBdXRoZW50aWNhdGlvbkNvbW1hbmQSUgoTYXV0aGVudGljYXRpb25U", 
              "b2tlbhgBIAEoCzI1LkFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlByb3RvYnVm", 
              "LkF1dGhlbnRpY2F0aW9uVG9rZW4SGAoQY29ubmVjdGlvblN0cmluZxgCIAEo", 
              "CRIRCglwcm9jZXNzSUQYAyABKAlCRQokY29tLmFsYWNoaXNvZnQubm9zZGIu", 
              "Y29tbW9uLnByb3RvYnVmQh1BdXRoZW50aWNhdGlvbkNvbW1hbmRQcm90b2Nv", 
            "bA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand, global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationCommand__Descriptor,
                  new string[] { "AuthenticationToken", "ConnectionString", "ProcessID", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationToken.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class AuthenticationCommand : pb::GeneratedMessage<AuthenticationCommand, AuthenticationCommand.Builder> {
    private AuthenticationCommand() { }
    private static readonly AuthenticationCommand defaultInstance = new AuthenticationCommand().MakeReadOnly();
    private static readonly string[] _authenticationCommandFieldNames = new string[] { "authenticationToken", "connectionString", "processID" };
    private static readonly uint[] _authenticationCommandFieldTags = new uint[] { 10, 18, 26 };
    public static AuthenticationCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override AuthenticationCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override AuthenticationCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<AuthenticationCommand, AuthenticationCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_AuthenticationCommand__FieldAccessorTable; }
    }
    
    public const int AuthenticationTokenFieldNumber = 1;
    private bool hasAuthenticationToken;
    private global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken authenticationToken_;
    public bool HasAuthenticationToken {
      get { return hasAuthenticationToken; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken AuthenticationToken {
      get { return authenticationToken_ ?? global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.DefaultInstance; }
    }
    
    public const int ConnectionStringFieldNumber = 2;
    private bool hasConnectionString;
    private string connectionString_ = "";
    public bool HasConnectionString {
      get { return hasConnectionString; }
    }
    public string ConnectionString {
      get { return connectionString_; }
    }
    
    public const int ProcessIDFieldNumber = 3;
    private bool hasProcessID;
    private string processID_ = "";
    public bool HasProcessID {
      get { return hasProcessID; }
    }
    public string ProcessID {
      get { return processID_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _authenticationCommandFieldNames;
      if (hasAuthenticationToken) {
        output.WriteMessage(1, field_names[0], AuthenticationToken);
      }
      if (hasConnectionString) {
        output.WriteString(2, field_names[1], ConnectionString);
      }
      if (hasProcessID) {
        output.WriteString(3, field_names[2], ProcessID);
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
      if (hasConnectionString) {
        size += pb::CodedOutputStream.ComputeStringSize(2, ConnectionString);
      }
      if (hasProcessID) {
        size += pb::CodedOutputStream.ComputeStringSize(3, ProcessID);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static AuthenticationCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static AuthenticationCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static AuthenticationCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static AuthenticationCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static AuthenticationCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static AuthenticationCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static AuthenticationCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static AuthenticationCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private AuthenticationCommand MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(AuthenticationCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<AuthenticationCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(AuthenticationCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private AuthenticationCommand result;
      
      private AuthenticationCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          AuthenticationCommand original = result;
          result = new AuthenticationCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override AuthenticationCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.Descriptor; }
      }
      
      public override AuthenticationCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.DefaultInstance; }
      }
      
      public override AuthenticationCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is AuthenticationCommand) {
          return MergeFrom((AuthenticationCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(AuthenticationCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.AuthenticationCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasAuthenticationToken) {
          MergeAuthenticationToken(other.AuthenticationToken);
        }
        if (other.HasConnectionString) {
          ConnectionString = other.ConnectionString;
        }
        if (other.HasProcessID) {
          ProcessID = other.ProcessID;
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
            int field_ordinal = global::System.Array.BinarySearch(_authenticationCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _authenticationCommandFieldTags[field_ordinal];
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
              global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.CreateBuilder();
              if (result.hasAuthenticationToken) {
                subBuilder.MergeFrom(AuthenticationToken);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              AuthenticationToken = subBuilder.BuildPartial();
              break;
            }
            case 18: {
              result.hasConnectionString = input.ReadString(ref result.connectionString_);
              break;
            }
            case 26: {
              result.hasProcessID = input.ReadString(ref result.processID_);
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
      public global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken AuthenticationToken {
        get { return result.AuthenticationToken; }
        set { SetAuthenticationToken(value); }
      }
      public Builder SetAuthenticationToken(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasAuthenticationToken = true;
        result.authenticationToken_ = value;
        return this;
      }
      public Builder SetAuthenticationToken(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasAuthenticationToken = true;
        result.authenticationToken_ = builderForValue.Build();
        return this;
      }
      public Builder MergeAuthenticationToken(global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasAuthenticationToken &&
            result.authenticationToken_ != global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.DefaultInstance) {
            result.authenticationToken_ = global::Alachisoft.NosDB.Common.Protobuf.AuthenticationToken.CreateBuilder(result.authenticationToken_).MergeFrom(value).BuildPartial();
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
      
      public bool HasConnectionString {
        get { return result.hasConnectionString; }
      }
      public string ConnectionString {
        get { return result.ConnectionString; }
        set { SetConnectionString(value); }
      }
      public Builder SetConnectionString(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasConnectionString = true;
        result.connectionString_ = value;
        return this;
      }
      public Builder ClearConnectionString() {
        PrepareBuilder();
        result.hasConnectionString = false;
        result.connectionString_ = "";
        return this;
      }
      
      public bool HasProcessID {
        get { return result.hasProcessID; }
      }
      public string ProcessID {
        get { return result.ProcessID; }
        set { SetProcessID(value); }
      }
      public Builder SetProcessID(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasProcessID = true;
        result.processID_ = value;
        return this;
      }
      public Builder ClearProcessID() {
        PrepareBuilder();
        result.hasProcessID = false;
        result.processID_ = "";
        return this;
      }
    }
    static AuthenticationCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.AuthenticationCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
