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
    public static partial class ReadQueryCommand {
    
      #region Extension registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_ReadQueryCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand, global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_ReadQueryCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static ReadQueryCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChZSZWFkUXVlcnlDb21tYW5kLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNv", 
              "bW1vbi5Qcm90b2J1ZhoSUXVlcnlCdWlsZGVyLnByb3RvIlEKEFJlYWRRdWVy", 
              "eUNvbW1hbmQSPQoFcXVlcnkYASABKAsyLi5BbGFjaGlzb2Z0Lk5vc0RCLkNv", 
              "bW1vbi5Qcm90b2J1Zi5RdWVyeUJ1aWxkZXJCQAokY29tLmFsYWNoaXNvZnQu", 
              "bm9zZGIuY29tbW9uLnByb3RvYnVmQhhSZWFkUXVlcnlDb21tYW5kUHJvdG9j", 
            "b2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_ReadQueryCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_ReadQueryCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand, global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_ReadQueryCommand__Descriptor,
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
  public sealed partial class ReadQueryCommand : pb::GeneratedMessage<ReadQueryCommand, ReadQueryCommand.Builder> {
    private ReadQueryCommand() { }
    private static readonly ReadQueryCommand defaultInstance = new ReadQueryCommand().MakeReadOnly();
    private static readonly string[] _readQueryCommandFieldNames = new string[] { "query" };
    private static readonly uint[] _readQueryCommandFieldTags = new uint[] { 10 };
    public static ReadQueryCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override ReadQueryCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override ReadQueryCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.ReadQueryCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_ReadQueryCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<ReadQueryCommand, ReadQueryCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.ReadQueryCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_ReadQueryCommand__FieldAccessorTable; }
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
      string[] field_names = _readQueryCommandFieldNames;
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
    public static ReadQueryCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static ReadQueryCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static ReadQueryCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static ReadQueryCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static ReadQueryCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static ReadQueryCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static ReadQueryCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static ReadQueryCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static ReadQueryCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static ReadQueryCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private ReadQueryCommand MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(ReadQueryCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<ReadQueryCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(ReadQueryCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private ReadQueryCommand result;
      
      private ReadQueryCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          ReadQueryCommand original = result;
          result = new ReadQueryCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override ReadQueryCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.Descriptor; }
      }
      
      public override ReadQueryCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.DefaultInstance; }
      }
      
      public override ReadQueryCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is ReadQueryCommand) {
          return MergeFrom((ReadQueryCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(ReadQueryCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.ReadQueryCommand.DefaultInstance) return this;
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
            int field_ordinal = global::System.Array.BinarySearch(_readQueryCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _readQueryCommandFieldTags[field_ordinal];
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
    static ReadQueryCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.ReadQueryCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
