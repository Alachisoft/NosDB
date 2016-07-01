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
    public static partial class DropIndexCommand {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_DropIndexCommand__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand, global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_DropIndexCommand__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static DropIndexCommand() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChZEcm9wSW5kZXhDb21tYW5kLnByb3RvEiBBbGFjaGlzb2Z0Lk5vc0RCLkNv", 
              "bW1vbi5Qcm90b2J1ZiJWChBEcm9wSW5kZXhDb21tYW5kEhIKCmF0dHJpYnV0", 
              "ZXMYASADKAkSDwoHaXNBc3luYxgCIAEoCBIdChVJbmRleERyb3BlZENhbGxi", 
              "YWNrSWQYAyABKBFCQAokY29tLmFsYWNoaXNvZnQubm9zZGIuY29tbW9uLnBy", 
            "b3RvYnVmQhhEcm9wSW5kZXhDb21tYW5kUHJvdG9jb2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_DropIndexCommand__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_DropIndexCommand__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand, global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_DropIndexCommand__Descriptor,
                  new string[] { "Attributes", "IsAsync", "IndexDropedCallbackId", });
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
  public sealed partial class DropIndexCommand : pb::GeneratedMessage<DropIndexCommand, DropIndexCommand.Builder> {
    private DropIndexCommand() { }
    private static readonly DropIndexCommand defaultInstance = new DropIndexCommand().MakeReadOnly();
    private static readonly string[] _dropIndexCommandFieldNames = new string[] { "IndexDropedCallbackId", "attributes", "isAsync" };
    private static readonly uint[] _dropIndexCommandFieldTags = new uint[] { 24, 10, 16 };
    public static DropIndexCommand DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override DropIndexCommand DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override DropIndexCommand ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DropIndexCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_DropIndexCommand__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<DropIndexCommand, DropIndexCommand.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.DropIndexCommand.internal__static_Alachisoft_NosDB_Common_Protobuf_DropIndexCommand__FieldAccessorTable; }
    }
    
    public const int AttributesFieldNumber = 1;
    private pbc::PopsicleList<string> attributes_ = new pbc::PopsicleList<string>();
    public scg::IList<string> AttributesList {
      get { return pbc::Lists.AsReadOnly(attributes_); }
    }
    public int AttributesCount {
      get { return attributes_.Count; }
    }
    public string GetAttributes(int index) {
      return attributes_[index];
    }
    
    public const int IsAsyncFieldNumber = 2;
    private bool hasIsAsync;
    private bool isAsync_;
    public bool HasIsAsync {
      get { return hasIsAsync; }
    }
    public bool IsAsync {
      get { return isAsync_; }
    }
    
    public const int IndexDropedCallbackIdFieldNumber = 3;
    private bool hasIndexDropedCallbackId;
    private int indexDropedCallbackId_;
    public bool HasIndexDropedCallbackId {
      get { return hasIndexDropedCallbackId; }
    }
    public int IndexDropedCallbackId {
      get { return indexDropedCallbackId_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _dropIndexCommandFieldNames;
      if (attributes_.Count > 0) {
        output.WriteStringArray(1, field_names[1], attributes_);
      }
      if (hasIsAsync) {
        output.WriteBool(2, field_names[2], IsAsync);
      }
      if (hasIndexDropedCallbackId) {
        output.WriteSInt32(3, field_names[0], IndexDropedCallbackId);
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
      {
        int dataSize = 0;
        foreach (string element in AttributesList) {
          dataSize += pb::CodedOutputStream.ComputeStringSizeNoTag(element);
        }
        size += dataSize;
        size += 1 * attributes_.Count;
      }
      if (hasIsAsync) {
        size += pb::CodedOutputStream.ComputeBoolSize(2, IsAsync);
      }
      if (hasIndexDropedCallbackId) {
        size += pb::CodedOutputStream.ComputeSInt32Size(3, IndexDropedCallbackId);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static DropIndexCommand ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DropIndexCommand ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DropIndexCommand ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static DropIndexCommand ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static DropIndexCommand ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DropIndexCommand ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static DropIndexCommand ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static DropIndexCommand ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static DropIndexCommand ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static DropIndexCommand ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private DropIndexCommand MakeReadOnly() {
      attributes_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(DropIndexCommand prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<DropIndexCommand, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(DropIndexCommand cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private DropIndexCommand result;
      
      private DropIndexCommand PrepareBuilder() {
        if (resultIsReadOnly) {
          DropIndexCommand original = result;
          result = new DropIndexCommand();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override DropIndexCommand MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.Descriptor; }
      }
      
      public override DropIndexCommand DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.DefaultInstance; }
      }
      
      public override DropIndexCommand BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is DropIndexCommand) {
          return MergeFrom((DropIndexCommand) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(DropIndexCommand other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.DropIndexCommand.DefaultInstance) return this;
        PrepareBuilder();
        if (other.attributes_.Count != 0) {
          result.attributes_.Add(other.attributes_);
        }
        if (other.HasIsAsync) {
          IsAsync = other.IsAsync;
        }
        if (other.HasIndexDropedCallbackId) {
          IndexDropedCallbackId = other.IndexDropedCallbackId;
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
            int field_ordinal = global::System.Array.BinarySearch(_dropIndexCommandFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _dropIndexCommandFieldTags[field_ordinal];
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
              input.ReadStringArray(tag, field_name, result.attributes_);
              break;
            }
            case 16: {
              result.hasIsAsync = input.ReadBool(ref result.isAsync_);
              break;
            }
            case 24: {
              result.hasIndexDropedCallbackId = input.ReadSInt32(ref result.indexDropedCallbackId_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public pbc::IPopsicleList<string> AttributesList {
        get { return PrepareBuilder().attributes_; }
      }
      public int AttributesCount {
        get { return result.AttributesCount; }
      }
      public string GetAttributes(int index) {
        return result.GetAttributes(index);
      }
      public Builder SetAttributes(int index, string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.attributes_[index] = value;
        return this;
      }
      public Builder AddAttributes(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.attributes_.Add(value);
        return this;
      }
      public Builder AddRangeAttributes(scg::IEnumerable<string> values) {
        PrepareBuilder();
        result.attributes_.Add(values);
        return this;
      }
      public Builder ClearAttributes() {
        PrepareBuilder();
        result.attributes_.Clear();
        return this;
      }
      
      public bool HasIsAsync {
        get { return result.hasIsAsync; }
      }
      public bool IsAsync {
        get { return result.IsAsync; }
        set { SetIsAsync(value); }
      }
      public Builder SetIsAsync(bool value) {
        PrepareBuilder();
        result.hasIsAsync = true;
        result.isAsync_ = value;
        return this;
      }
      public Builder ClearIsAsync() {
        PrepareBuilder();
        result.hasIsAsync = false;
        result.isAsync_ = false;
        return this;
      }
      
      public bool HasIndexDropedCallbackId {
        get { return result.hasIndexDropedCallbackId; }
      }
      public int IndexDropedCallbackId {
        get { return result.IndexDropedCallbackId; }
        set { SetIndexDropedCallbackId(value); }
      }
      public Builder SetIndexDropedCallbackId(int value) {
        PrepareBuilder();
        result.hasIndexDropedCallbackId = true;
        result.indexDropedCallbackId_ = value;
        return this;
      }
      public Builder ClearIndexDropedCallbackId() {
        PrepareBuilder();
        result.hasIndexDropedCallbackId = false;
        result.indexDropedCallbackId_ = 0;
        return this;
      }
    }
    static DropIndexCommand() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.DropIndexCommand.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
