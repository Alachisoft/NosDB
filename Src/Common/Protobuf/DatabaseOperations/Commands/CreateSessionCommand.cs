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
    public static partial class CreateSessionCommand {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand, global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionCommand__FieldAccessorTable;
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_Credential__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.Credential, global::Alachisoft.NosDB.Common.Protobuf.Credential.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_Credential__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static CreateSessionCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChpDcmVhdGVTZXNzaW9uQ29tbWFuZC5wcm90bxIgQWxhY2hpc29mdC5Ob3NE", 
              "Qi5Db21tb24uUHJvdG9idWYiagoUQ3JlYXRlU2Vzc2lvbkNvbW1hbmQSEAoI", 
              "Y2xpZW50SWQYASABKAUSQAoKY3JlZGVudGlhbBgCIAEoCzIsLkFsYWNoaXNv", 
              "ZnQuTm9zREIuQ29tbW9uLlByb3RvYnVmLkNyZWRlbnRpYWwiMAoKQ3JlZGVu", 
              "dGlhbBIQCgh1c2VyTmFtZRgBIAEoDBIQCghwYXNzd29yZBgCIAEoDEJECiRj", 
              "b20uYWxhY2hpc29mdC5ub3NkYi5jb21tb24ucHJvdG9idWZCHENyZWF0ZVNl", 
            "c3Npb25Db21tYW5kUHJvdG9jb2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand, global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionCommand__Descriptor,
                  new string[] { "ClientId", "Credential", });
          internal__static_Alachisoft_NosDB_Common_Protobuf_Credential__Descriptor = Descriptor.MessageTypes[1];
          internal__static_Alachisoft_NosDB_Common_Protobuf_Credential__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.Credential, global::Alachisoft.NosDB.Common.Protobuf.Credential.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_Credential__Descriptor,
                  new string[] { "UserName", "Password", });
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
  public sealed partial class CreateSessionCommand : pb::GeneratedMessage<CreateSessionCommand, CreateSessionCommand.Builder> {
    private CreateSessionCommand() { }
    private static readonly CreateSessionCommand defaultInstance = new CreateSessionCommand().MakeReadOnly();
    private static readonly string[] _createSessionCommandFieldNames = new string[] { "clientId", "credential" };
    private static readonly uint[] _createSessionCommandFieldTags = new uint[] { 8, 18 };
    public static CreateSessionCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override CreateSessionCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override CreateSessionCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<CreateSessionCommand, CreateSessionCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_CreateSessionCommand__FieldAccessorTable; }
    }
    
    public const int ClientIdFieldNumber = 1;
    private bool hasClientId;
    private int clientId_;
    public bool HasClientId {
      get { return hasClientId; }
    }
    public int ClientId {
      get { return clientId_; }
    }
    
    public const int CredentialFieldNumber = 2;
    private bool hasCredential;
    private global::Alachisoft.NosDB.Common.Protobuf.Credential credential_;
    public bool HasCredential {
      get { return hasCredential; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.Credential Credential {
      get { return credential_ ?? global::Alachisoft.NosDB.Common.Protobuf.Credential.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _createSessionCommandFieldNames;
      if (hasClientId) {
        output.WriteInt32(1, field_names[0], ClientId);
      }
      if (hasCredential) {
        output.WriteMessage(2, field_names[1], Credential);
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
      if (hasClientId) {
        size += pb::CodedOutputStream.ComputeInt32Size(1, ClientId);
      }
      if (hasCredential) {
        size += pb::CodedOutputStream.ComputeMessageSize(2, Credential);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static CreateSessionCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CreateSessionCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CreateSessionCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static CreateSessionCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static CreateSessionCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CreateSessionCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static CreateSessionCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static CreateSessionCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static CreateSessionCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static CreateSessionCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private CreateSessionCommand MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(CreateSessionCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<CreateSessionCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(CreateSessionCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private CreateSessionCommand result;
      
      private CreateSessionCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          CreateSessionCommand original = result;
          result = new CreateSessionCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override CreateSessionCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.Descriptor; }
      }
      
      public override CreateSessionCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.DefaultInstance; }
      }
      
      public override CreateSessionCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is CreateSessionCommand) {
          return MergeFrom((CreateSessionCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(CreateSessionCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.CreateSessionCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasClientId) {
          ClientId = other.ClientId;
        }
        if (other.HasCredential) {
          MergeCredential(other.Credential);
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
            int field_ordinal = global::System.Array.BinarySearch(_createSessionCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _createSessionCommandFieldTags[field_ordinal];
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
              result.hasClientId = input.ReadInt32(ref result.clientId_);
              break;
            }
            case 18: {
              global::Alachisoft.NosDB.Common.Protobuf.Credential.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.Credential.CreateBuilder();
              if (result.hasCredential) {
                subBuilder.MergeFrom(Credential);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              Credential = subBuilder.BuildPartial();
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasClientId {
        get { return result.hasClientId; }
      }
      public int ClientId {
        get { return result.ClientId; }
        set { SetClientId(value); }
      }
      public Builder SetClientId(int value) {
        PrepareBuilder();
        result.hasClientId = true;
        result.clientId_ = value;
        return this;
      }
      public Builder ClearClientId() {
        PrepareBuilder();
        result.hasClientId = false;
        result.clientId_ = 0;
        return this;
      }
      
      public bool HasCredential {
       get { return result.hasCredential; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.Credential Credential {
        get { return result.Credential; }
        set { SetCredential(value); }
      }
      public Builder SetCredential(global::Alachisoft.NosDB.Common.Protobuf.Credential value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasCredential = true;
        result.credential_ = value;
        return this;
      }
      public Builder SetCredential(global::Alachisoft.NosDB.Common.Protobuf.Credential.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasCredential = true;
        result.credential_ = builderForValue.Build();
        return this;
      }
      public Builder MergeCredential(global::Alachisoft.NosDB.Common.Protobuf.Credential value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasCredential &&
            result.credential_ != global::Alachisoft.NosDB.Common.Protobuf.Credential.DefaultInstance) {
            result.credential_ = global::Alachisoft.NosDB.Common.Protobuf.Credential.CreateBuilder(result.credential_).MergeFrom(value).BuildPartial();
        } else {
          result.credential_ = value;
        }
        result.hasCredential = true;
        return this;
      }
      public Builder ClearCredential() {
        PrepareBuilder();
        result.hasCredential = false;
        result.credential_ = null;
        return this;
      }
    }
    static CreateSessionCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionCommand.Descriptor, null);
    }
  }
  
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class Credential : pb::GeneratedMessage<Credential, Credential.Builder> {
    private Credential() { }
    private static readonly Credential defaultInstance = new Credential().MakeReadOnly();
    private static readonly string[] _credentialFieldNames = new string[] { "password", "userName" };
    private static readonly uint[] _credentialFieldTags = new uint[] { 18, 10 };
    public static Credential DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override Credential DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override Credential ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_Credential__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<Credential, Credential.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_Credential__FieldAccessorTable; }
    }
    
    public const int UserNameFieldNumber = 1;
    private bool hasUserName;
    private pb::ByteString userName_ = pb::ByteString.Empty;
    public bool HasUserName {
      get { return hasUserName; }
    }
    public pb::ByteString UserName {
      get { return userName_; }
    }
    
    public const int PasswordFieldNumber = 2;
    private bool hasPassword;
    private pb::ByteString password_ = pb::ByteString.Empty;
    public bool HasPassword {
      get { return hasPassword; }
    }
    public pb::ByteString Password {
      get { return password_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _credentialFieldNames;
      if (hasUserName) {
        output.WriteBytes(1, field_names[1], UserName);
      }
      if (hasPassword) {
        output.WriteBytes(2, field_names[0], Password);
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
      if (hasUserName) {
        size += pb::CodedOutputStream.ComputeBytesSize(1, UserName);
      }
      if (hasPassword) {
        size += pb::CodedOutputStream.ComputeBytesSize(2, Password);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static Credential ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Credential ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Credential ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Credential ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Credential ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Credential ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static Credential ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static Credential ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static Credential ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Credential ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private Credential MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(Credential prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<Credential, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(Credential cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private Credential result;
      
      private Credential PrepareBuilder() {
        if (resultIsReadOnly) {
          Credential original = result;
          result = new Credential();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override Credential MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.Credential.Descriptor; }
      }
      
      public override Credential DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.Credential.DefaultInstance; }
      }
      
      public override Credential BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is Credential) {
          return MergeFrom((Credential) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(Credential other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.Credential.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasUserName) {
          UserName = other.UserName;
        }
        if (other.HasPassword) {
          Password = other.Password;
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
            int field_ordinal = global::System.Array.BinarySearch(_credentialFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _credentialFieldTags[field_ordinal];
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
              result.hasUserName = input.ReadBytes(ref result.userName_);
              break;
            }
            case 18: {
              result.hasPassword = input.ReadBytes(ref result.password_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasUserName {
        get { return result.hasUserName; }
      }
      public pb::ByteString UserName {
        get { return result.UserName; }
        set { SetUserName(value); }
      }
      public Builder SetUserName(pb::ByteString value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasUserName = true;
        result.userName_ = value;
        return this;
      }
      public Builder ClearUserName() {
        PrepareBuilder();
        result.hasUserName = false;
        result.userName_ = pb::ByteString.Empty;
        return this;
      }
      
      public bool HasPassword {
        get { return result.hasPassword; }
      }
      public pb::ByteString Password {
        get { return result.Password; }
        set { SetPassword(value); }
      }
      public Builder SetPassword(pb::ByteString value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasPassword = true;
        result.password_ = value;
        return this;
      }
      public Builder ClearPassword() {
        PrepareBuilder();
        result.hasPassword = false;
        result.password_ = pb::ByteString.Empty;
        return this;
      }
    }
    static Credential() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.CreateSessionCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
