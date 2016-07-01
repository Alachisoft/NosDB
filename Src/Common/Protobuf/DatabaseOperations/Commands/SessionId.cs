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
    public static partial class SessionId {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_SessionId__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.SessionId, global::Alachisoft.NosDB.Common.Protobuf.SessionId.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_SessionId__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static SessionId() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "Cg9TZXNzaW9uSWQucHJvdG8SIEFsYWNoaXNvZnQuTm9zREIuQ29tbW9uLlBy", 
              "b3RvYnVmIj0KCVNlc3Npb25JZBIXCg9yb3V0ZXJTZXNzaW9uSWQYASABKAkS", 
              "FwoPY2xpZW50U2Vzc2lvbklkGAIgASgJQjkKJGNvbS5hbGFjaGlzb2Z0Lm5v", 
            "c2RiLmNvbW1vbi5wcm90b2J1ZkIRU2Vzc2lvbklkUHJvdG9jb2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_SessionId__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_SessionId__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.SessionId, global::Alachisoft.NosDB.Common.Protobuf.SessionId.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_SessionId__Descriptor,
                  new string[] { "RouterSessionId", "ClientSessionId", });
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
  public sealed partial class SessionId : pb::GeneratedMessage<SessionId, SessionId.Builder> {
    private SessionId() { }
    private static readonly SessionId defaultInstance = new SessionId().MakeReadOnly();
    private static readonly string[] _sessionIdFieldNames = new string[] { "clientSessionId", "routerSessionId" };
    private static readonly uint[] _sessionIdFieldTags = new uint[] { 18, 10 };
    public static SessionId DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override SessionId DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override SessionId ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.SessionId.internal__static_Alachisoft_NosDB_Common_Protobuf_SessionId__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<SessionId, SessionId.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.SessionId.internal__static_Alachisoft_NosDB_Common_Protobuf_SessionId__FieldAccessorTable; }
    }
    
    public const int RouterSessionIdFieldNumber = 1;
    private bool hasRouterSessionId;
    private string routerSessionId_ = "";
    public bool HasRouterSessionId {
      get { return hasRouterSessionId; }
    }
    public string RouterSessionId {
      get { return routerSessionId_; }
    }
    
    public const int ClientSessionIdFieldNumber = 2;
    private bool hasClientSessionId;
    private string clientSessionId_ = "";
    public bool HasClientSessionId {
      get { return hasClientSessionId; }
    }
    public string ClientSessionId {
      get { return clientSessionId_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _sessionIdFieldNames;
      if (hasRouterSessionId) {
        output.WriteString(1, field_names[1], RouterSessionId);
      }
      if (hasClientSessionId) {
        output.WriteString(2, field_names[0], ClientSessionId);
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
      if (hasRouterSessionId) {
        size += pb::CodedOutputStream.ComputeStringSize(1, RouterSessionId);
      }
      if (hasClientSessionId) {
        size += pb::CodedOutputStream.ComputeStringSize(2, ClientSessionId);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static SessionId ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static SessionId ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static SessionId ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static SessionId ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static SessionId ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static SessionId ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static SessionId ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static SessionId ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static SessionId ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static SessionId ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private SessionId MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(SessionId prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<SessionId, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(SessionId cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private SessionId result;
      
      private SessionId PrepareBuilder() {
        if (resultIsReadOnly) {
          SessionId original = result;
          result = new SessionId();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override SessionId MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.SessionId.Descriptor; }
      }
      
      public override SessionId DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.SessionId.DefaultInstance; }
      }
      
      public override SessionId BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is SessionId) {
          return MergeFrom((SessionId) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(SessionId other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.SessionId.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasRouterSessionId) {
          RouterSessionId = other.RouterSessionId;
        }
        if (other.HasClientSessionId) {
          ClientSessionId = other.ClientSessionId;
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
            int field_ordinal = global::System.Array.BinarySearch(_sessionIdFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _sessionIdFieldTags[field_ordinal];
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
              result.hasRouterSessionId = input.ReadString(ref result.routerSessionId_);
              break;
            }
            case 18: {
              result.hasClientSessionId = input.ReadString(ref result.clientSessionId_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasRouterSessionId {
        get { return result.hasRouterSessionId; }
      }
      public string RouterSessionId {
        get { return result.RouterSessionId; }
        set { SetRouterSessionId(value); }
      }
      public Builder SetRouterSessionId(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasRouterSessionId = true;
        result.routerSessionId_ = value;
        return this;
      }
      public Builder ClearRouterSessionId() {
        PrepareBuilder();
        result.hasRouterSessionId = false;
        result.routerSessionId_ = "";
        return this;
      }
      
      public bool HasClientSessionId {
        get { return result.hasClientSessionId; }
      }
      public string ClientSessionId {
        get { return result.ClientSessionId; }
        set { SetClientSessionId(value); }
      }
      public Builder SetClientSessionId(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasClientSessionId = true;
        result.clientSessionId_ = value;
        return this;
      }
      public Builder ClearClientSessionId() {
        PrepareBuilder();
        result.hasClientSessionId = false;
        result.clientSessionId_ = "";
        return this;
      }
    }
    static SessionId() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.SessionId.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
