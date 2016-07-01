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
    public static partial class UpdateCommand {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand, global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static UpdateCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChNVcGRhdGVDb21tYW5kLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNvbW1v", 
              "bi5Qcm90b2J1ZhoSUXVlcnlCdWlsZGVyLnByb3RvIk4KDVVwZGF0ZUNvbW1h", 
              "bmQSPQoFcXVlcnkYASABKAsyLi5BbGFjaGlzb2Z0Lk5vc0RCLkNvbW1vbi5Q", 
              "cm90b2J1Zi5RdWVyeUJ1aWxkZXJCPQokY29tLmFsYWNoaXNvZnQubm9zZGIu", 
            "Y29tbW9uLnByb3RvYnVmQhVVcGRhdGVDb21tYW5kUHJvdG9jb2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand, global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateCommand__Descriptor,
                  new string[] { "Query", });
          return null;
        };
        pbd::FileDescriptor.InternalBuildGeneratedFileFrom(descriptorData,
            new pbd::FileDescriptor[] {
            global::Alachisoft.NosDB.Common.Protobuf.Proto.QueryBuilder.Descriptor, 
            }, assigner);
      }
      #endregion
      
    }
  }
  #region Messages
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class UpdateCommand : pb::GeneratedMessage<UpdateCommand, UpdateCommand.Builder> {
    private UpdateCommand() { }
    private static readonly UpdateCommand defaultInstance = new UpdateCommand().MakeReadOnly();
    private static readonly string[] _updateCommandFieldNames = new string[] { "query" };
    private static readonly uint[] _updateCommandFieldTags = new uint[] { 10 };
    public static UpdateCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override UpdateCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override UpdateCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.UpdateCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<UpdateCommand, UpdateCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.UpdateCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_UpdateCommand__FieldAccessorTable; }
    }
    
    public const int QueryFieldNumber = 1;
    private bool hasQuery;
    private global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder query_;
    public bool HasQuery {
      get { return hasQuery; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder Query {
      get { return query_ ?? global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.DefaultInstance; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _updateCommandFieldNames;
      if (hasQuery) {
        output.WriteMessage(1, field_names[0], Query);
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
      if (hasQuery) {
        size += pb::CodedOutputStream.ComputeMessageSize(1, Query);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static UpdateCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static UpdateCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static UpdateCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static UpdateCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static UpdateCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static UpdateCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static UpdateCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static UpdateCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static UpdateCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static UpdateCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private UpdateCommand MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(UpdateCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<UpdateCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(UpdateCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private UpdateCommand result;
      
      private UpdateCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          UpdateCommand original = result;
          result = new UpdateCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override UpdateCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.Descriptor; }
      }
      
      public override UpdateCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.DefaultInstance; }
      }
      
      public override UpdateCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is UpdateCommand) {
          return MergeFrom((UpdateCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(UpdateCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.UpdateCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasQuery) {
          MergeQuery(other.Query);
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
            int field_ordinal = global::System.Array.BinarySearch(_updateCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _updateCommandFieldTags[field_ordinal];
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
              global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.Builder subBuilder = global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.CreateBuilder();
              if (result.hasQuery) {
                subBuilder.MergeFrom(Query);
              }
              input.ReadMessage(subBuilder, extensionRegistry);
              Query = subBuilder.BuildPartial();
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasQuery {
       get { return result.hasQuery; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder Query {
        get { return result.Query; }
        set { SetQuery(value); }
      }
      public Builder SetQuery(global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasQuery = true;
        result.query_ = value;
        return this;
      }
      public Builder SetQuery(global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.hasQuery = true;
        result.query_ = builderForValue.Build();
        return this;
      }
      public Builder MergeQuery(global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        if (result.hasQuery &&
            result.query_ != global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.DefaultInstance) {
            result.query_ = global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.CreateBuilder(result.query_).MergeFrom(value).BuildPartial();
        } else {
          result.query_ = value;
        }
        result.hasQuery = true;
        return this;
      }
      public Builder ClearQuery() {
        PrepareBuilder();
        result.hasQuery = false;
        result.query_ = null;
        return this;
      }
    }
    static UpdateCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.UpdateCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
