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
    public static partial class QueryBuilder {
    
      #region EXTENSION registration
      public static void RegisterAllExtensions(pb::ExtensionRegistry registry) {
      }
      #endregion
      #region Static variables
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_QueryBuilder__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder, global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_QueryBuilder__FieldAccessorTable;
      internal static pbd::MessageDescriptor internal__static_Alachisoft_NosDB_Common_Protobuf_Parameter__Descriptor;
      internal static pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.Parameter, global::Alachisoft.NosDB.Common.Protobuf.Parameter.Builder> internal__static_Alachisoft_NosDB_Common_Protobuf_Parameter__FieldAccessorTable;
      #endregion
      #region Descriptor
      public static pbd::FileDescriptor Descriptor {
        get { return descriptor; }
      }
      private static pbd::FileDescriptor descriptor;
      
      static QueryBuilder() {
        byte[] descriptorData = global::System.Convert.FromBase64String(
            string.Concat(
              "ChJRdWVyeUJ1aWxkZXIucHJvdG8SIEFsYWNoaXNvZnQuTm9zREIuQ29tbW9u", 
              "LlByb3RvYnVmIl4KDFF1ZXJ5QnVpbGRlchINCgVxdWVyeRgBIAEoCRI/Cgpw", 
              "YXJhbWV0ZXJzGAIgAygLMisuQWxhY2hpc29mdC5Ob3NEQi5Db21tb24uUHJv", 
              "dG9idWYuUGFyYW1ldGVyIkMKCVBhcmFtZXRlchIRCglhdHRyaWJ1dGUYASAB", 
              "KAkSDQoFdmFsdWUYAiABKAkSFAoManNvbkRhdGFUeXBlGAMgASgFQjwKJGNv", 
              "bS5hbGFjaGlzb2Z0Lm5vc2RiLmNvbW1vbi5wcm90b2J1ZkIUUXVlcnlCdWls", 
            "ZGVyUHJvdG9jb2w="));
        pbd::FileDescriptor.InternalDescriptorAssigner assigner = delegate(pbd::FileDescriptor root) {
          descriptor = root;
          internal__static_Alachisoft_NosDB_Common_Protobuf_QueryBuilder__Descriptor = Descriptor.MessageTypes[0];
          internal__static_Alachisoft_NosDB_Common_Protobuf_QueryBuilder__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder, global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_QueryBuilder__Descriptor,
                  new string[] { "Query", "Parameters", });
          internal__static_Alachisoft_NosDB_Common_Protobuf_Parameter__Descriptor = Descriptor.MessageTypes[1];
          internal__static_Alachisoft_NosDB_Common_Protobuf_Parameter__FieldAccessorTable = 
              new pb::FieldAccess.FieldAccessorTable<global::Alachisoft.NosDB.Common.Protobuf.Parameter, global::Alachisoft.NosDB.Common.Protobuf.Parameter.Builder>(internal__static_Alachisoft_NosDB_Common_Protobuf_Parameter__Descriptor,
                  new string[] { "Attribute", "Value", "JsonDataType", });
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
  public sealed partial class QueryBuilder : pb::GeneratedMessage<QueryBuilder, QueryBuilder.Builder> {
    private QueryBuilder() { }
    private static readonly QueryBuilder defaultInstance = new QueryBuilder().MakeReadOnly();
    private static readonly string[] _queryBuilderFieldNames = new string[] { "parameters", "query" };
    private static readonly uint[] _queryBuilderFieldTags = new uint[] { 18, 10 };
    public static QueryBuilder DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override QueryBuilder DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override QueryBuilder ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.QueryBuilder.internal__static_Alachisoft_NosDB_Common_Protobuf_QueryBuilder__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<QueryBuilder, QueryBuilder.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.QueryBuilder.internal__static_Alachisoft_NosDB_Common_Protobuf_QueryBuilder__FieldAccessorTable; }
    }
    
    public const int QueryFieldNumber = 1;
    private bool hasQuery;
    private string query_ = "";
    public bool HasQuery {
      get { return hasQuery; }
    }
    public string Query {
      get { return query_; }
    }
    
    public const int ParametersFieldNumber = 2;
    private pbc::PopsicleList<global::Alachisoft.NosDB.Common.Protobuf.Parameter> parameters_ = new pbc::PopsicleList<global::Alachisoft.NosDB.Common.Protobuf.Parameter>();
    public scg::IList<global::Alachisoft.NosDB.Common.Protobuf.Parameter> ParametersList {
      get { return parameters_; }
    }
    public int ParametersCount {
      get { return parameters_.Count; }
    }
    public global::Alachisoft.NosDB.Common.Protobuf.Parameter GetParameters(int index) {
      return parameters_[index];
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _queryBuilderFieldNames;
      if (hasQuery) {
        output.WriteString(1, field_names[1], Query);
      }
      if (parameters_.Count > 0) {
        output.WriteMessageArray(2, field_names[0], parameters_);
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
        size += pb::CodedOutputStream.ComputeStringSize(1, Query);
      }
      foreach (global::Alachisoft.NosDB.Common.Protobuf.Parameter element in ParametersList) {
        size += pb::CodedOutputStream.ComputeMessageSize(2, element);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static QueryBuilder ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static QueryBuilder ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static QueryBuilder ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static QueryBuilder ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static QueryBuilder ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static QueryBuilder ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static QueryBuilder ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static QueryBuilder ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static QueryBuilder ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static QueryBuilder ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private QueryBuilder MakeReadOnly() {
      parameters_.MakeReadOnly();
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(QueryBuilder prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<QueryBuilder, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(QueryBuilder cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private QueryBuilder result;
      
      private QueryBuilder PrepareBuilder() {
        if (resultIsReadOnly) {
          QueryBuilder original = result;
          result = new QueryBuilder();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override QueryBuilder MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.Descriptor; }
      }
      
      public override QueryBuilder DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.DefaultInstance; }
      }
      
      public override QueryBuilder BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is QueryBuilder) {
          return MergeFrom((QueryBuilder) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(QueryBuilder other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.QueryBuilder.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasQuery) {
          Query = other.Query;
        }
        if (other.parameters_.Count != 0) {
          result.parameters_.Add(other.parameters_);
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
            int field_ordinal = global::System.Array.BinarySearch(_queryBuilderFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _queryBuilderFieldTags[field_ordinal];
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
              result.hasQuery = input.ReadString(ref result.query_);
              break;
            }
            case 18: {
              input.ReadMessageArray(tag, field_name, result.parameters_, global::Alachisoft.NosDB.Common.Protobuf.Parameter.DefaultInstance, extensionRegistry);
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
      public string Query {
        get { return result.Query; }
        set { SetQuery(value); }
      }
      public Builder SetQuery(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasQuery = true;
        result.query_ = value;
        return this;
      }
      public Builder ClearQuery() {
        PrepareBuilder();
        result.hasQuery = false;
        result.query_ = "";
        return this;
      }
      
      public pbc::IPopsicleList<global::Alachisoft.NosDB.Common.Protobuf.Parameter> ParametersList {
        get { return PrepareBuilder().parameters_; }
      }
      public int ParametersCount {
        get { return result.ParametersCount; }
      }
      public global::Alachisoft.NosDB.Common.Protobuf.Parameter GetParameters(int index) {
        return result.GetParameters(index);
      }
      public Builder SetParameters(int index, global::Alachisoft.NosDB.Common.Protobuf.Parameter value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.parameters_[index] = value;
        return this;
      }
      public Builder SetParameters(int index, global::Alachisoft.NosDB.Common.Protobuf.Parameter.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.parameters_[index] = builderForValue.Build();
        return this;
      }
      public Builder AddParameters(global::Alachisoft.NosDB.Common.Protobuf.Parameter value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.parameters_.Add(value);
        return this;
      }
      public Builder AddParameters(global::Alachisoft.NosDB.Common.Protobuf.Parameter.Builder builderForValue) {
        pb::ThrowHelper.ThrowIfNull(builderForValue, "builderForValue");
        PrepareBuilder();
        result.parameters_.Add(builderForValue.Build());
        return this;
      }
      public Builder AddRangeParameters(scg::IEnumerable<global::Alachisoft.NosDB.Common.Protobuf.Parameter> values) {
        PrepareBuilder();
        result.parameters_.Add(values);
        return this;
      }
      public Builder ClearParameters() {
        PrepareBuilder();
        result.parameters_.Clear();
        return this;
      }
    }
    static QueryBuilder() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.QueryBuilder.Descriptor, null);
    }
  }
  
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  public sealed partial class Parameter : pb::GeneratedMessage<Parameter, Parameter.Builder> {
    private Parameter() { }
    private static readonly Parameter defaultInstance = new Parameter().MakeReadOnly();
    private static readonly string[] _parameterFieldNames = new string[] { "attribute", "jsonDataType", "value" };
    private static readonly uint[] _parameterFieldTags = new uint[] { 10, 24, 18 };
    public static Parameter DefaultInstance {
      get { return defaultInstance; }
    }
    
    public override Parameter DefaultInstanceForType {
      get { return DefaultInstance; }
    }
    
    protected override Parameter ThisMessage {
      get { return this; }
    }
    
    public static pbd::MessageDescriptor Descriptor {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.QueryBuilder.internal__static_Alachisoft_NosDB_Common_Protobuf_Parameter__Descriptor; }
    }
    
    protected override pb::FieldAccess.FieldAccessorTable<Parameter, Parameter.Builder> InternalFieldAccessors {
      get { return global::Alachisoft.NosDB.Common.Protobuf.Proto.QueryBuilder.internal__static_Alachisoft_NosDB_Common_Protobuf_Parameter__FieldAccessorTable; }
    }
    
    public const int AttributeFieldNumber = 1;
    private bool hasAttribute;
    private string attribute_ = "";
    public bool HasAttribute {
      get { return hasAttribute; }
    }
    public string Attribute {
      get { return attribute_; }
    }
    
    public const int ValueFieldNumber = 2;
    private bool hasValue;
    private string value_ = "";
    public bool HasValue {
      get { return hasValue; }
    }
    public string Value {
      get { return value_; }
    }
    
    public const int JsonDataTypeFieldNumber = 3;
    private bool hasJsonDataType;
    private int jsonDataType_;
    public bool HasJsonDataType {
      get { return hasJsonDataType; }
    }
    public int JsonDataType {
      get { return jsonDataType_; }
    }
    
    public override bool IsInitialized {
      get {
        return true;
      }
    }
    
    public override void WriteTo(pb::ICodedOutputStream output) {
      CalcSerializedSize();
      string[] field_names = _parameterFieldNames;
      if (hasAttribute) {
        output.WriteString(1, field_names[0], Attribute);
      }
      if (hasValue) {
        output.WriteString(2, field_names[2], Value);
      }
      if (hasJsonDataType) {
        output.WriteInt32(3, field_names[1], JsonDataType);
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
      if (hasAttribute) {
        size += pb::CodedOutputStream.ComputeStringSize(1, Attribute);
      }
      if (hasValue) {
        size += pb::CodedOutputStream.ComputeStringSize(2, Value);
      }
      if (hasJsonDataType) {
        size += pb::CodedOutputStream.ComputeInt32Size(3, JsonDataType);
      }
      size += UnknownFields.SerializedSize;
      memoizedSerializedSize = size;
      return size;
    }
    public static Parameter ParseFrom(pb::ByteString data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Parameter ParseFrom(pb::ByteString data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Parameter ParseFrom(byte[] data) {
      return ((Builder) CreateBuilder().MergeFrom(data)).BuildParsed();
    }
    public static Parameter ParseFrom(byte[] data, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(data, extensionRegistry)).BuildParsed();
    }
    public static Parameter ParseFrom(global::System.IO.Stream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Parameter ParseFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    public static Parameter ParseDelimitedFrom(global::System.IO.Stream input) {
      return CreateBuilder().MergeDelimitedFrom(input).BuildParsed();
    }
    public static Parameter ParseDelimitedFrom(global::System.IO.Stream input, pb::ExtensionRegistry extensionRegistry) {
      return CreateBuilder().MergeDelimitedFrom(input, extensionRegistry).BuildParsed();
    }
    public static Parameter ParseFrom(pb::ICodedInputStream input) {
      return ((Builder) CreateBuilder().MergeFrom(input)).BuildParsed();
    }
    public static Parameter ParseFrom(pb::ICodedInputStream input, pb::ExtensionRegistry extensionRegistry) {
      return ((Builder) CreateBuilder().MergeFrom(input, extensionRegistry)).BuildParsed();
    }
    private Parameter MakeReadOnly() {
      return this;
    }
    
    public static Builder CreateBuilder() { return new Builder(); }
    public override Builder ToBuilder() { return CreateBuilder(this); }
    public override Builder CreateBuilderForType() { return new Builder(); }
    public static Builder CreateBuilder(Parameter prototype) {
      return new Builder(prototype);
    }
    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public sealed partial class Builder : pb::GeneratedBuilder<Parameter, Builder> {
      protected override Builder ThisBuilder {
        get { return this; }
      }
      public Builder() {
        result = DefaultInstance;
        resultIsReadOnly = true;
      }
      internal Builder(Parameter cloneFrom) {
        result = cloneFrom;
        resultIsReadOnly = true;
      }
      
      private bool resultIsReadOnly;
      private Parameter result;
      
      private Parameter PrepareBuilder() {
        if (resultIsReadOnly) {
          Parameter original = result;
          result = new Parameter();
          resultIsReadOnly = false;
          MergeFrom(original);
        }
        return result;
      }
      
      public override bool IsInitialized {
        get { return result.IsInitialized; }
      }
      
      protected override Parameter MessageBeingBuilt {
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
        get { return global::Alachisoft.NosDB.Common.Protobuf.Parameter.Descriptor; }
      }
      
      public override Parameter DefaultInstanceForType {
        get { return global::Alachisoft.NosDB.Common.Protobuf.Parameter.DefaultInstance; }
      }
      
      public override Parameter BuildPartial() {
        if (resultIsReadOnly) {
          return result;
        }
        resultIsReadOnly = true;
        return result.MakeReadOnly();
      }
      
      public override Builder MergeFrom(pb::IMessage other) {
        if (other is Parameter) {
          return MergeFrom((Parameter) other);
        } else {
          base.MergeFrom(other);
          return this;
        }
      }
      
      public override Builder MergeFrom(Parameter other) {
        if (other == global::Alachisoft.NosDB.Common.Protobuf.Parameter.DefaultInstance) return this;
        PrepareBuilder();
        if (other.HasAttribute) {
          Attribute = other.Attribute;
        }
        if (other.HasValue) {
          Value = other.Value;
        }
        if (other.HasJsonDataType) {
          JsonDataType = other.JsonDataType;
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
            int field_ordinal = global::System.Array.BinarySearch(_parameterFieldNames, field_name, global::System.StringComparer.Ordinal);
            if(field_ordinal >= 0)
              tag = _parameterFieldTags[field_ordinal];
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
              result.hasAttribute = input.ReadString(ref result.attribute_);
              break;
            }
            case 18: {
              result.hasValue = input.ReadString(ref result.value_);
              break;
            }
            case 24: {
              result.hasJsonDataType = input.ReadInt32(ref result.jsonDataType_);
              break;
            }
          }
        }
        
        if (unknownFields != null) {
          this.UnknownFields = unknownFields.Build();
        }
        return this;
      }
      
      
      public bool HasAttribute {
        get { return result.hasAttribute; }
      }
      public string Attribute {
        get { return result.Attribute; }
        set { SetAttribute(value); }
      }
      public Builder SetAttribute(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasAttribute = true;
        result.attribute_ = value;
        return this;
      }
      public Builder ClearAttribute() {
        PrepareBuilder();
        result.hasAttribute = false;
        result.attribute_ = "";
        return this;
      }
      
      public bool HasValue {
        get { return result.hasValue; }
      }
      public string Value {
        get { return result.Value; }
        set { SetValue(value); }
      }
      public Builder SetValue(string value) {
        pb::ThrowHelper.ThrowIfNull(value, "value");
        PrepareBuilder();
        result.hasValue = true;
        result.value_ = value;
        return this;
      }
      public Builder ClearValue() {
        PrepareBuilder();
        result.hasValue = false;
        result.value_ = "";
        return this;
      }
      
      public bool HasJsonDataType {
        get { return result.hasJsonDataType; }
      }
      public int JsonDataType {
        get { return result.JsonDataType; }
        set { SetJsonDataType(value); }
      }
      public Builder SetJsonDataType(int value) {
        PrepareBuilder();
        result.hasJsonDataType = true;
        result.jsonDataType_ = value;
        return this;
      }
      public Builder ClearJsonDataType() {
        PrepareBuilder();
        result.hasJsonDataType = false;
        result.jsonDataType_ = 0;
        return this;
      }
    }
    static Parameter() {
      object.ReferenceEquals(global::Alachisoft.NosDB.Common.Protobuf.Proto.QueryBuilder.Descriptor, null);
    }
  }
  
  #endregion
  
}

#endregion Designer generated code
