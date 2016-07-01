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
    public static partial class InitDatabaseResponse {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_InitDatabaseResponse__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse, global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_InitDatabaseResponse__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static InitDatabaseResponse() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChpJbml0RGF0YWJhc2VSZXNwb25zZS5wcm90bxIgQWxhY2hpc29mdC5Ob3NE", 
              "Qi5Db21tb24uUHJvdG9idWYiNQoUSW5pdERhdGFiYXNlUmVzcG9uc2USHQoV", 
              "aXNEYXRhYmFzZUluaXRpYWxpemVkGAEgASgIQkQKJGNvbS5hbGFjaGlzb2Z0", 
              "Lm5vc2RiLmNvbW1vbi5wcm90b2J1ZkIcSW5pdERhdGFiYXNlUmVzcG9uc2VQ", 
            "cm90b2NvbA=="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_InitDatabaseResponse__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_InitDatabaseResponse__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse, global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_InitDatabaseResponse__Descriptor,
                  new string[] { "IsDatabaseInitialized", });
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
  public sealed partial class InitDatabaseResponse : pb::GeneratedMessage<InitDatabaseResponse, InitDatabaseResponse.Builder> {
    private InitDatabaseResponse() { }
    private static readonly InitDatabaseResponse defaultInstance = new InitDatabaseResponse().MakeReadOnly();
    private static readonly string[] _initDatabaseResponseFieldNames = new string[] { "isDatabaseInitialized" };
    private static readonly uint[] _initDatabaseResponseFieldTags = new uint[] { 8 };
    public static InitDatabaseResponse DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override InitDatabaseResponse DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override InitDatabaseResponse ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.InitDatabaseResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_InitDatabaseResponse__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<InitDatabaseResponse, InitDatabaseResponse.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.InitDatabaseResponse.internal__static_Alachisoft_NosDB_Common_Protobuf_InitDatabaseResponse__FieldAccessorTable; }
    }
    
    public const int IsDatabaseInitializedFieldNumber = 1;
    private bool hasIsDatabaseInitialized;
    private bool isDatabaseInitialized_;
    public bool HasIsDatabaseInitialized {
      get { return hasIsDatabaseInitialized; }
    }
    public bool IsDatabaseInitialized {
      get { return isDatabaseInitialized_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _initDatabaseResponseFieldNames;
      if (hasIsDatabaseInitialized) {
        output.WriteBool(1, field_names[0], IsDatabaseInitialized);
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
      if (hasIsDatabaseInitialized) {
        size += pb::CodedOutputStream.ComputeBoolSize(1, IsDatabaseInitialized);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static InitDatabaseResponse ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static InitDatabaseResponse ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static InitDatabaseResponse ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static InitDatabaseResponse ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static InitDatabaseResponse ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static InitDatabaseResponse ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static InitDatabaseResponse ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static InitDatabaseResponse ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static InitDatabaseResponse ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static InitDatabaseResponse ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private InitDatabaseResponse MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(InitDatabaseResponse prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<InitDatabaseResponse, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(InitDatabaseResponse cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private InitDatabaseResponse result;
      
      private InitDatabaseResponse PrepareBuilder() {
        if (resultIsReadOnly) {
          InitDatabaseResponse original = result;
          result = new InitDatabaseResponse();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override InitDatabaseResponse MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.Descriptor; }
      }
      
      public override InitDatabaseResponse DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.DefaultInstance; }
      }
      
      public override InitDatabaseResponse BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is InitDatabaseResponse) {
          return MergeFrom((InitDatabaseResponse) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(InitDatabaseResponse other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.InitDatabaseResponse.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasIsDatabaseInitialized) {
          IsDatabaseInitialized = other.IsDatabaseInitialized;
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
            int field_ordinal = global::System.Array.BinarySearch(_initDatabaseResponseFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _initDatabaseResponseFieldTags[field_ordinal];
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
              result.hasIsDatabaseInitialized = input.ReadBool(ref result.isDatabaseInitialized_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasIsDatabaseInitialized {
        get { return result.hasIsDatabaseInitialized; }
      }
      public bool IsDatabaseInitialized {
        get { return result.IsDatabaseInitialized; }
        set { SetIsDatabaseInitialized(value); }
      }
      public Builder SetIsDatabaseInitialized(bool value) {
        PrepareBuilder();
        result.hasIsDatabaseInitialized = true;
        result.isDatabaseInitialized_ = value;
        return this;
      }
      public Builder ClearIsDatabaseInitialized() {
        PrepareBuilder();
        result.hasIsDatabaseInitialized = false;
        result.isDatabaseInitialized_ = false;
        return this;
      }
    }
    static InitDatabaseResponse() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.InitDatabaseResponse.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
